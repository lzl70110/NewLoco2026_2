using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data.Models;
using NewLoco.Web.Auth;                        // Perm
using NewLoco.Web.Areas.Admin.Models;          // RoleCreateInput, RoleClaimsViewModel, RoleClaimsPostModel, Users VM-s
using GCommon;                                  // Messages

namespace NewLoco.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Keep claims-based policies
    [Authorize(Policy = Perm.Any.AdminAll)]
    // Optional: stable routing (safe to keep existing links intact if they target the default area/controller/action)
    [Route("Admin/[controller]/[action]")]
    public class RbacController(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly RoleManager<ApplicationRole> roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        private readonly UserManager<ApplicationUser> userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        private const string PermissionClaimType = "permission";

        // ==============
        // Roles listing
        // ==============
        [HttpGet]
        [Authorize(Policy = Perm.Admin.Roles.Read)]
        public async Task<IActionResult> Roles()
        {
            // Use async + AsNoTracking for read-only listing
            var roles = await roleManager.Roles
                                         .AsNoTracking()
                                         .OrderBy(r => r.Name)
                                         .ToListAsync();

            return View(roles);
        }

        [HttpGet]
        [Authorize(Policy = Perm.Admin.Roles.Edit)]
        public IActionResult CreateRole() => View(new RoleCreateInput());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Admin.Roles.Edit)]
        public async Task<IActionResult> CreateRole(RoleCreateInput input)
        {
            // Normalize input
            var name = (input?.Name ?? string.Empty).Trim();
            var label = (input?.Label ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(input.Name), Messages.Rbac.Error_RoleName_Required);
                return View(input);
            }

            if (await roleManager.RoleExistsAsync(name))
            {
                ModelState.AddModelError(string.Empty, Messages.Rbac.Error_Role_AlreadyExists);
                return View(input);
            }

            var role = new ApplicationRole
            {
                Name = name,
                Label = string.IsNullOrWhiteSpace(label) ? name : label
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                ModelState.AddModelError(string.Empty, string.Format(Messages.Rbac.Error_Role_CreateFailed, error));
                return View(input);
            }

            TempData["Msg"] = string.Format(Messages.Rbac.Info_Role_Created, name);
            return RedirectToAction(nameof(Roles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Admin.Roles.Edit)]
        public async Task<IActionResult> DeleteRole(string name)
        {
            // Normalize
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = Messages.Rbac.Error_RoleName_Missing;
                return RedirectToAction(nameof(Roles));
            }

            var role = await roleManager.FindByNameAsync(name);
            if (role == null)
            {
                TempData["Error"] = Messages.Rbac.Error_Role_NotFound;
                return RedirectToAction(nameof(Roles));
            }

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                TempData["Error"] = string.Format(Messages.Rbac.Error_Role_DeleteFailed, error);
            }
            else
            {
                TempData["Msg"] = string.Format(Messages.Rbac.Info_Role_Deleted, name);
            }

            return RedirectToAction(nameof(Roles));
        }

        // =========================
        // Role Claims (permissions)
        // =========================
        [HttpGet]
        [Authorize(Policy = Perm.Admin.Roles.Read)]
        public async Task<IActionResult> RoleClaims(string roleName)
        {
            roleName = (roleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(roleName)) return NotFound();

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();

            var claims = await roleManager.GetClaimsAsync(role);
            var selected = claims
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var vm = new RoleClaimsViewModel
            {
                RoleName = roleName,
                AvailablePermissions = [.. Perm.All().OrderBy(x => x)],
                SelectedPermissions = selected
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Admin.Roles.Edit)]
        public async Task<IActionResult> RoleClaims(RoleClaimsPostModel post)
        {
            var roleName = (post?.RoleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(roleName)) return NotFound();

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();

            var allClaims = await roleManager.GetClaimsAsync(role);

            var current = allClaims
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var requested = (post?.Selected ?? [])
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Add missing
            foreach (var add in requested.Except(current))
            {
                var res = await roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, add));
                if (!res.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, string.Format(
                        Messages.Rbac.Error_Claim_AddFailed,
                        string.Join(", ", res.Errors.Select(e => e.Description))));
                }
            }

            // Remove extra
            foreach (var rem in current.Except(requested))
            {
                var claim = allClaims.FirstOrDefault(c =>
                    c.Type == PermissionClaimType &&
                    string.Equals(c.Value, rem, StringComparison.OrdinalIgnoreCase));

                if (claim != null)
                {
                    var res = await roleManager.RemoveClaimAsync(role, claim);
                    if (!res.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, string.Format(
                            Messages.Rbac.Error_Claim_RemoveFailed,
                            string.Join(", ", res.Errors.Select(e => e.Description))));
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var vm = new RoleClaimsViewModel
                {
                    RoleName = roleName,
                    AvailablePermissions = [.. Perm.All().OrderBy(x => x)],
                    SelectedPermissions = requested
                };
                return View(vm);
            }

            TempData["Msg"] = requested.SetEquals(current)
                ? Messages.Rbac.Info_Nothing_Changed
                : string.Format(Messages.Rbac.Info_Permissions_Updated, roleName);

            return RedirectToAction(nameof(Roles));
        }

        // =========================
        // Users ↔ Roles management
        // =========================

        [HttpGet]
        [Authorize(Policy = Perm.Admin.Users.Read)]
        public async Task<IActionResult> Users()
        {
            // Use async + AsNoTracking to avoid tracking overhead in read-only list
            var users = await userManager.Users
                .AsNoTracking()
                .ToListAsync();

            var models = new List<UserRolesViewModel>(users.Count);

            // Note: This is N+1 calls for roles. With Identity it's typical; batching requires custom store/queries.
            foreach (var u in users)
            {
                var roles = await userManager.GetRolesAsync(u);
                models.Add(new UserRolesViewModel
                {
                    UserId = u.Id.ToString(),                           // Identity default key is string; ToString() not needed
                    UserName = u.UserName,                   // expose username explicitly
                    Email = string.IsNullOrWhiteSpace(u.Email) ? u.UserName : u.Email,
                    Roles = roles?.ToArray() ?? []
                });
            }

            var allRoles = await roleManager.Roles
                .AsNoTracking()
                .Select(r => r.Name!)
                .OrderBy(n => n)
                .ToArrayAsync();

            var vm = new UsersWithRolesPageViewModel
            {
                Users = models,
                AllRoles = allRoles
            };

            // View must declare: @model UsersWithRolesPageViewModel and iterate Model.Users
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Admin.Users.Edit)]
        public async Task<IActionResult> AddUserToRole(RoleAssignInput input)
        {
            var userKey = (input?.EmailOrUserName ?? string.Empty).Trim();
            var roleName = (input?.RoleName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(userKey) || string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction(nameof(Users));
            }

            // Try email, then username
            var user = await userManager.FindByEmailAsync(userKey)
                       ?? await userManager.FindByNameAsync(userKey);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Users));
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var res = await userManager.AddToRoleAsync(user, roleName);
                if (!res.Succeeded)
                {
                    TempData["Error"] = string.Join("; ", res.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Users));
                }

                // Security stamp refresh after role change
                await userManager.UpdateSecurityStampAsync(user);
            }

            TempData["Success"] = $"User '{userKey}' added to role '{roleName}'.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Admin.Users.Edit)]
        public async Task<IActionResult> RemoveUserFromRole(RoleAssignInput input)
        {
            var userKey = (input?.EmailOrUserName ?? string.Empty).Trim();
            var roleName = (input?.RoleName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(userKey) || string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction(nameof(Users));
            }

            var user = await userManager.FindByEmailAsync(userKey)
                       ?? await userManager.FindByNameAsync(userKey);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Users));
            }

            if (await userManager.IsInRoleAsync(user, roleName))
            {
                var res = await userManager.RemoveFromRoleAsync(user, roleName);
                if (!res.Succeeded)
                {
                    TempData["Error"] = string.Join("; ", res.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Users));
                }

                // Security stamp refresh after role change
                await userManager.UpdateSecurityStampAsync(user);
            }

            TempData["Success"] = $"User '{userKey}' removed from role '{roleName}'.";
            return RedirectToAction(nameof(Users));
        }
    }
}