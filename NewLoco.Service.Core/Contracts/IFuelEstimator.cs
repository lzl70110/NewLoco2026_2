using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core.Contracts
{
    public interface IFuelEstimator
    {
        FuelEstimate EstimateDefault(
            LocomotiveType type,
            decimal amount,
            MeasuringUnits unit
        );
    }

    public readonly record struct FuelEstimate(
        decimal SuggestedLiters,
        decimal PolicyRate,
        decimal PolicyFullHint
    );
}