using Microsoft.AspNetCore.Authorization;

namespace NewLoco.Web.Auth
{
    // Requirement carries the permission name
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}