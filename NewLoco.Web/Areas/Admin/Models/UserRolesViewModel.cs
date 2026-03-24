namespace NewLoco.Web.Areas.Admin.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }  
        public string? Email { get; set; }
        public string[] Roles { get; set; } = [];
    }
}