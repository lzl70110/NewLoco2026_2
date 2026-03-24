using System;
using System.Collections.Generic;

namespace NewLoco.Web.Areas.Admin.Models
{
    public class RoleClaimsViewModel
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> AvailablePermissions { get; set; } = [];
        public HashSet<string> SelectedPermissions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
