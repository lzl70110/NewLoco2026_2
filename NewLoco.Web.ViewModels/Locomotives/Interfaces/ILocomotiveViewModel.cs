namespace NewLoco.Web.ViewModels.Locomotives.Interfaces;
using System.ComponentModel.DataAnnotations;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;
public interface ILocomotiveViewModel
    {
    [Required]
    public DateTime CreatedOn { get; set; }

    [Required]
    [StringLength(CreatedByMaxLength, MinimumLength = CreatedByMinLength )]
    public string CreatedBy { get; set; }  


    [StringLength(ModifiedByMaxLength, MinimumLength =ModifiedByMinLength)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    [StringLength(NoteMaxLength, MinimumLength =NoteMinLength)]
    public string? Note { get; set; }
    }
    