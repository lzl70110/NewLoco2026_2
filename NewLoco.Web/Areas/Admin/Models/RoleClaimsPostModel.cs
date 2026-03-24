using System.Collections.Generic;

namespace NewLoco.Web.Areas.Admin.Models
{
    public class RoleClaimsPostModel
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string>? Selected { get; set; } // checkbox values
    }
}