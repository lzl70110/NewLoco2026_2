namespace NewLoco.Service.Core.Contracts
{
    public sealed class FuelPoliciesOptions
    {
        public int DepotStepLiters { get; set; } = 10;
        public FuelPolicy Shunter { get; set; } = new();
        public FuelPolicy Mainline { get; set; } = new();

        // NEW: thresholds per class code, e.g., "52","55","06"
        public Dictionary<string, FuelSafetyOptions> PerClassSafety { get; set; } = [];
    }

    public sealed class FuelSafetyOptions
    {
        public int SoftWarningLiters { get; set; }
        public int HardFloorLiters { get; set; }
    }

    public sealed class FuelPolicy
    {
        public decimal MinIdleLph { get; set; }
        public decimal MinLoadLph { get; set; }
        public decimal FullLoadLphHint { get; set; }
    }
}