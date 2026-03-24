using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NewLoco.Data.Models;   // ApplicationUser, ApplicationRole
using NewLoco.Web.Auth;      // Perm

namespace NewLoco.Web.Infrastructure
{
    // Options bound from configuration / user-secrets
    public class BootstrapAdminOptions
    {
        public bool Enabled { get; set; } = false;   // toggled via secrets/env
        public string? Email { get; set; }           // from secrets/env
        public string? Password { get; set; }        // from secrets/env

        // NOTE: align with your usage in UI/Sidebar (User.IsInRole("SysAdmin"))
        public string RoleName { get; set; } = "SysAdmin";

        public string? FullName { get; set; }
    }

    /// <summary>
    /// Idempotent startup seeding:
    ///  - Ensures admin role exists and holds ONLY admin permissions
    ///  - Removes 'permission' claims from ALL non-admin roles
    ///  - (Optional but recommended) Removes 'permission' claims from USERS who are NOT in admin role
    ///  - Bootstraps an admin user when Enabled=true in configuration
    /// </summary>
    public static class AppSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;

            // ---------------------------------------------------------------------
            // 1) Ensure admin role exists
            // ---------------------------------------------------------------------
            var adminRoleName = string.IsNullOrWhiteSpace(options.RoleName) ? "SysAdmin" : options.RoleName.Trim();

            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                var createRoleRes = await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = adminRoleName,
                    Label = adminRoleName // DB NOT NULL constraint safe
                });

                if (!createRoleRes.Succeeded)
                    throw new InvalidOperationException("Cannot create admin role: " +
                        string.Join(", ", createRoleRes.Errors.Select(e => e.Description)));
            }

            var adminRole = await roleManager.FindByNameAsync(adminRoleName)
                            ?? throw new InvalidOperationException($"Role '{adminRoleName}' was not found after ensure/create.");

            // ---------------------------------------------------------------------
            // 2) Ensure admin role holds admin permissions
            //    (a) Keep or add wildcard Perm.Any.All (your PermissionHandler supports it)
            //    (b) Optionally add concrete Perm.* keys (handy if wildcard is ever removed)
            // ---------------------------------------------------------------------
            var adminClaims = await roleManager.GetClaimsAsync(adminRole);
            var adminClaimValues = adminClaims
                .Where(c => c.Type == Perm.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.Ordinal);

            // (a) Full access wildcard for admin role only
            if (!adminClaimValues.Contains(Perm.Any.All))
            {
                var addFull = await roleManager.AddClaimAsync(adminRole, new Claim(Perm.ClaimType, Perm.Any.All));
                if (!addFull.Succeeded)
                    throw new InvalidOperationException("Cannot add full-access permission claim to admin role: " +
                        string.Join(", ", addFull.Errors.Select(e => e.Description)));
            }

            // (b) Ensure concrete permissions exist too (optional safety)
            foreach (var p in Perm.All())
            {
                if (!adminClaimValues.Contains(p))
                {
                    var addRes = await roleManager.AddClaimAsync(adminRole, new Claim(Perm.ClaimType, p));
                    if (!addRes.Succeeded)
                        throw new InvalidOperationException("Cannot add permission claim to admin role: " +
                            string.Join(", ", addRes.Errors.Select(e => e.Description)));
                }
            }

            // ---------------------------------------------------------------------
            // 3) HARDEN: Remove ANY 'permission' claims from NON-admin roles
            //    This prevents leaking permissions via legacy seed or misconfiguration.
            // ---------------------------------------------------------------------
            var allRoles = roleManager.Roles.ToList();
            foreach (var role in allRoles.Where(r => !string.Equals(r.Name, adminRoleName, StringComparison.Ordinal)))
            {
                var claims = await roleManager.GetClaimsAsync(role);
                var permClaims = claims.Where(c => c.Type == Perm.ClaimType).ToList();

                if (permClaims.Count == 0) continue;

                foreach (var c in permClaims)
                {
                    // Remove any permission claim from non-admin roles
                    var rm = await roleManager.RemoveClaimAsync(role, c);
                    if (!rm.Succeeded)
                        throw new InvalidOperationException($"Cannot remove claim '{c.Value}' from role '{role.Name}': " +
                            string.Join(", ", rm.Errors.Select(e => e.Description)));
                }
            }

            // ---------------------------------------------------------------------
            // 4) HARDEN (optional but recommended):
            //    Remove ANY 'permission' claims directly assigned to USERS
            //    (unless the user is in admin role). We keep permissions ONLY on roles.
            // ---------------------------------------------------------------------
            var adminRoleId = (await roleManager.FindByNameAsync(adminRoleName))?.Id;
            var users = userManager.Users.ToList();
            foreach (var u in users)
            {
                var isAdminUser = await userManager.IsInRoleAsync(u, adminRoleName);
                var userClaims = await userManager.GetClaimsAsync(u);
                var permClaims = userClaims.Where(c => c.Type == Perm.ClaimType).ToList();

                if (permClaims.Count == 0) continue;

                if (!isAdminUser)
                {
                    // Strip any direct permission claims on non-admin users
                    foreach (var c in permClaims)
                    {
                        var rm = await userManager.RemoveClaimAsync(u, c);
                        if (!rm.Succeeded)
                            throw new InvalidOperationException($"Cannot remove claim '{c.Value}' from user '{u.Email ?? u.UserName}': " +
                                string.Join(", ", rm.Errors.Select(e => e.Description)));
                    }

                    // Refresh stamp so the auth cookie drops stale claims
                    await userManager.UpdateSecurityStampAsync(u);
                }
                // If you want admin users to rely purely on role-claims (not direct), uncomment:
                // else
                // {
                //     foreach (var c in permClaims)
                //         await userManager.RemoveClaimAsync(u, c);
                //     await userManager.UpdateSecurityStampAsync(u);
                // }
            }

            // ---------------------------------------------------------------------
            // 5) Bootstrap Admin user (only when explicitly enabled)
            // ---------------------------------------------------------------------
            if (options.Enabled)
            {
                if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
                    throw new InvalidOperationException("BootstrapAdmin.Enabled=true but Email/Password are missing in secrets/config.");

                var email = options.Email.Trim();
                var password = options.Password;

                var admin = await userManager.FindByEmailAsync(email);
                if (admin == null)
                {
                    var fullName = string.IsNullOrWhiteSpace(options.FullName)
                        ? "System Administrator"
                        : options.FullName.Trim();

                    admin = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName
                    };

                    var createUser = await userManager.CreateAsync(admin, password);
                    if (!createUser.Succeeded)
                        throw new InvalidOperationException("Cannot create admin user: " +
                            string.Join(", ", createUser.Errors.Select(e => e.Description)));
                }

                if (!await userManager.IsInRoleAsync(admin, adminRoleName))
                {
                    var addToRole = await userManager.AddToRoleAsync(admin, adminRoleName);
                    if (!addToRole.Succeeded)
                        throw new InvalidOperationException("Cannot add admin to role: " +
                            string.Join(", ", addToRole.Errors.Select(e => e.Description)));
                }

                await userManager.UpdateSecurityStampAsync(admin);
            }
        }
    }
}