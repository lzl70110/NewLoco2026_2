using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NewLoco.Data.Models;

namespace NewLoco.Web.Auth
{
    //  Copies role claims (type "permission") into the user principal on sign-in
    public class AppClaimsPrincipalFactory<TUser, TRole>(
        UserManager<TUser> userManager,
        RoleManager<TRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : UserClaimsPrincipalFactory<TUser, TRole>(userManager, roleManager, optionsAccessor)
        where TUser : ApplicationUser
        where TRole : ApplicationRole
    {
        private readonly UserManager<TUser> _userManager = userManager;
        private readonly RoleManager<TRole> _roleManager = roleManager;

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            //  Get user roles (short-circuit if none)
            var roleNames = await _userManager.GetRolesAsync(user);
            if (roleNames is null || roleNames.Count == 0)
                return identity;

            //  Collect existing permission claims from identity to avoid duplicates
            var existing = new HashSet<string>(
                identity.Claims
                        .Where(c => c.Type == Perm.ClaimType)
                        .Select(c => c.Value),
                StringComparer.Ordinal // exact match
            );

            //  Copy role permission-claims to user identity
            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var rc in roleClaims)
                {
                    //   Only our permission claims are propagated
                    if (rc.Type != Perm.ClaimType) continue;

                    if (!existing.Contains(rc.Value))
                    {
                        identity.AddClaim(new Claim(Perm.ClaimType, rc.Value));
                        existing.Add(rc.Value);
                    }
                }
            }

            return identity;
        }
    }
}