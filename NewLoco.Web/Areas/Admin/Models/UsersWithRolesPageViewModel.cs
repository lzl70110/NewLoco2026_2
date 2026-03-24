using System.Collections.Generic;

namespace NewLoco.Web.Areas.Admin.Models
{
    public class UsersWithRolesPageViewModel
    {
        public List<UserRolesViewModel> Users { get; set; } = [];
        public string[] AllRoles { get; set; } = [];
    }
}