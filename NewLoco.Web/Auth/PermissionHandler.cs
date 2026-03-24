using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NewLoco.Web.Auth
{
    // Note: Supports both exact and wildcard matching (Perm.*.* and Perm.Area.*)
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User is null)
                return Task.CompletedTask;

            // collect all permission claims of the user
            var perms = context.User.Claims
                .Where(c => c.Type == Perm.ClaimType)
                .Select(c => c.Value)
                .ToArray();

            // 1) exact match
            if (perms.Any(v => string.Equals(v, requirement.Permission, StringComparison.Ordinal)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2) wildcard match:
            // - Perm.*.*  -> matches everything
            // - Perm.X.*  -> matches any "Perm.X.Y"
            if (perms.Any(v =>
                    v == Perm.Any.All || // full access
                    (v.EndsWith(".*", StringComparison.Ordinal) &&
                     requirement.Permission.StartsWith(v[..^2] + ".", StringComparison.Ordinal))))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}