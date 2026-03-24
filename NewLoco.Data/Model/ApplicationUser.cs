using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static NewLoco.GCommon.EntityValidationConstants.ApplicationUser;

namespace NewLoco.Data.Models;
public class ApplicationUser: IdentityUser<Guid>
{
    [Required]
    [PersonalData]
    [StringLength(FullNameMaxLength)]
    public string FullName { get; set; } = null!;

    [PersonalData]
    [StringLength(WorkNumberLength)]
    public string? WorkNumber {  get; set; }

}
