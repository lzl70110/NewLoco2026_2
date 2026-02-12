using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.ShiftWork;
namespace NewLoco.Data.Models;

public class ShiftWork : BaseEntity
    {
    [Key] public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Locomotive))]
    public int LocoId { get; set; }

    [Required]
    public Locomotive Locomotive { get; set; } = null!;


    public DateOnly Date { get; set; }

    public Shift Shift { get; set; } = Shift.Day;

    [Required]
    [Column(TypeName = Dec)]
    public decimal InitialValue { get; set; }

    [Required]
    [Column(TypeName = Dec)]
    public decimal FinalValue { get; set; }

    [Required]
    [Column(TypeName = Dec)]
    public decimal Amount { get; set; }

    }