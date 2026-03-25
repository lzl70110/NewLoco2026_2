// File: NewLoco.Service.Core/Contracts/IFuelEstimator.cs
using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core.Contracts
{
    /// <summary>Provides a default fuel suggestion based on policy and worked motohours.</summary>
    public interface IFuelEstimator
    {
        FuelEstimate EstimateDefault(LocomotiveType type, decimal hours);
    }

    /// <summary>Value object with the suggested liters and policy hints.</summary>
    public readonly record struct FuelEstimate(
        decimal SuggestedLiters,
        decimal PolicyMinLph,
        decimal PolicyFullHint
    );
}