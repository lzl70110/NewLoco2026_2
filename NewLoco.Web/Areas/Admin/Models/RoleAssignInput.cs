namespace NewLoco.Web.Areas.Admin.Models
{
    public class RoleAssignInput
    {
        public string EmailOrUserName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}