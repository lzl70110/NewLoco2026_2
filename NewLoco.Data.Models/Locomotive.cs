using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using static NewLoco.GCommon.EntityValidationConstants.Locomotive;
using NewLoco.GCommon.Enums;
namespace NewLoco.Data.Models;

[Index(nameof(Number), IsUnique = true)]
public class Locomotive : BaseEntity
{
    [Key] public int Id { get; set; }

    [Required]
    [StringLength(LocomotiveNumberLength)]
    public string Number { get; set; } = null!;

    public LocomotiveType LocomotiveType { get; set; } = LocomotiveType.Shunter;

    [Comment(" Mh or Km")] 
    public MeasuringUnits MeasuringUnit { get; set; } = MeasuringUnits.Mh;
 
    public virtual ICollection<ShiftWork> ShiftWorks { get; set; }
        = new HashSet<ShiftWork>();

    public virtual ICollection<Fuel.Fuel> Fuels { get; set; }
        = new HashSet<Fuel.Fuel>();
}