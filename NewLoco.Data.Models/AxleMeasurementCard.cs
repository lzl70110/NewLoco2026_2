using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewLoco.Data.Models;

[Index(nameof(Year), nameof(SequenceNumber), IsUnique = true)]
public class AxleMeasurementCard : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int SelectedLocomotiveId { get; set; }

    [ForeignKey(nameof(SelectedLocomotiveId))]
    public Locomotive Locomotive { get; set; } = null!;

    public int Year { get; set; }

    public int SequenceNumber { get; set; }

    public int AxleCount { get; set; }

    public string DocumentNumber => $"{SequenceNumber}/{Year % 100:D2}";

    public DateTime MeasurementDate { get; set; }

    public ICollection<AxleMeasurementValue> Axles { get; set; }
        = [];
}