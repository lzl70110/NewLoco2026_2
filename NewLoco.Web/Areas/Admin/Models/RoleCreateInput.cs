using System.ComponentModel.DataAnnotations;

namespace NewLoco.Web.Areas.Admin.Models
{
    public class RoleCreateInput
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(128)]
        public string Label { get; set; } = null!;
    }
}