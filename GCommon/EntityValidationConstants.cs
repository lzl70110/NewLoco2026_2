namespace NewLoco.GCommon;


public static class EntityValidationConstants
{
    public static class Locomotive
        {
        public const int LocomotiveNumberLength = 6;
        public const string LocomotiveNumberPattern = @"^[0-9]{2}\-[0-9]{3}$";

        public const double MinFuelCapacity = 0.1;
        public const double MaxFuelCapacity = 5000.0;
        public const string Dec = "decimal(9,2)";
        public const string DateTimeFormat = "dd-MM-yyyy HH:mm";

        }

    public static class BaseEntity
    {
        public const int CreatedByMinLength = 3;
        public const int CreatedByMaxLength = 150;
        public const int ModifiedByMinLength = 3;
        public const int ModifiedByMaxLength = 150;
        public const int NoteMaxLength = 1000;
        public const int NoteMinLength = 5;
        }
    public static class ShiftWork
    {
        public const string Dec = "decimal(9,2)";
        public const double ValueMin = 0.0;
        public const double ValueMax = 999999.9;
     
    }

    public static class Fuel
    {
        public const string Dec = "decimal(9,2)";
        public const double ValueMin =- 0.1;
        public const double ValueMax = 5000.0;
        public const int NoteMaxLength = 1000; 
    }
    public static class ApplicationUser
    {
        public const int FullNameMinLength = 3;
        public const int FullNameMaxLength = 100;
        
        public const int WorkNumberLength = 4;
        public const string WorkNumberPattern = @"^\d{2,4}$";
    }
    public static class ApplicationRole
    { 
        public const int LabelMaxLength = 50;
    }
    public static class Axle
    {
        public const double ArMin = 1357;
        public const double ArMax = 1363;

        public const double ShMin = 25;
        public const double ShMax = 36;

        public const double SdMin = 22;
        public const double  SdMax = 33.15;

        public const double QRMin = 6.5;
        public const double QRMax = 22;

        public const double SrMin = 22;
        public const double SrMax = 22;
    }
}