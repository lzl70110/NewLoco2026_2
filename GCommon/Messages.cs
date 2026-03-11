namespace GCommon;
public static class Messages
{

    public static class FuelServiceKeys
    {
        // Validation / Domain messages
        public const string Msg_FuelInFuture = "Msg_FuelInFuture";
        public const string Msg_FinalFuelTooHigh = "Msg_FinalFuelTooHigh";
        public const string Msg_NoFuelRecordForLoco = "Msg_NoFuelRecordForLoco";
        public const string Msg_NotEnoughFuel = "Msg_NotEnoughFuel";
    }
    public static class Locomotive
    {
        public const string Error_Number_Required = "Number requered";
        public const string Error_Number_Length = "Number must be exactly 6 chars (NN-NNN)";
        public const string Error_Number_Format = "Number must be in format  NN-NNN";
        public const string Error_Note_Length = "Note length must be between {1} and {0}";

    }
}
