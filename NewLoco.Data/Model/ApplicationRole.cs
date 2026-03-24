using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static NewLoco.GCommon.EntityValidationConstants.ApplicationRole;

namespace NewLoco.Data.Models;
public class ApplicationRole: IdentityRole<Guid>
{
    [Required]
    [MaxLength(LabelMaxLength)]
    public string Label { get; set; } = string.Empty;

}
