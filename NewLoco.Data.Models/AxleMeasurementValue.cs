namespace NewLoco.Data.Models
{
    public class AxleMeasurementValue
    {
        public int Id { get; set; }

        public int AxleMeasurementCardId { get; set; }
        public AxleMeasurementCard? AxleMeasurementCard { get; set; }

        // Axle number (1, 2, 3, …)
        public int AxleNumber { get; set; }

        // qR — wheel radius measurement (left / right)
        public double? qR_Left { get; set; }
        public double? qR_Right { get; set; }

        // Sd — lateral deviation (left / right)
        public double? Sd_Left { get; set; }
        public double? Sd_Right { get; set; } 

        // Sh — axlebox height (left / right)
        public double? Sh_Left { get; set; }
        public double? Sh_Right { get; set; }

        // Ar — manually entered measurement
        public double? Ar { get; set; }

        // Sr — calculated: SR = AR + Sd_Left + Sd_Right (stored in the DB)
        public double? Sr { get; set; }

       
       
    }
}