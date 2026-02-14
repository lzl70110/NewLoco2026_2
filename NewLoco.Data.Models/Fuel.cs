using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.Fuel;

namespace NewLoco.Data.Models.Fuel
    {
    using NewLoco.Data.Models;

    public class Fuel : BaseEntity
        {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Locomotive))]
        public int LocoId { get; set; }

        [Required]
        public Locomotive Locomotive { get; set; } = null!;

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public Shift Shift { get; set; } = Shift.Day;

        [Column(TypeName = Dec)]
        public decimal InitialFuel { get; set; }

        [Column(TypeName = Dec)]
        public decimal FinalFuel { get; set; }

        [Column(TypeName = Dec)]
        public decimal Consumption { get; set; }

        [Column(TypeName = Dec)]
        public decimal Refueled { get; set; }

        }
    }