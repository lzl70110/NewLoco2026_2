using System.ComponentModel.DataAnnotations;
using static NewLoco.GCommon.EntityValidationConstants.Axle;
using static GCommon.Messages.Axle;

namespace NewLoco.Web.ViewModels.Axles
{
    public class AxleMeasurementValueViewModel
    {
        public int AxleNumber { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(ArMin, ArMax, ErrorMessage = Error_Ar_Range)]
        public double? Ar { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(ShMin, ShMax, ErrorMessage = Error_Sh_Range)]
        public double? Sh_Left { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(ShMin, ShMax, ErrorMessage = Error_Sh_Range)]
        public double? Sh_Right { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(SdMin, SdMax, ErrorMessage = Error_Sd_Range)]
        public double? Sd_Left { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(SdMin, SdMax, ErrorMessage = Error_Sd_Range)]
        public double? Sd_Right { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(QRMin, QRMax, ErrorMessage = Error_QR_Range)]
        public double? QR_Left { get; set; }

        [Required(ErrorMessage = Msg_FieldRequired)]
        [Range(QRMin, QRMax, ErrorMessage = Error_QR_Range)]
        public double? QR_Right { get; set; }

        public double? Sr { get; set; } // Calculated on client and server
    }
}
