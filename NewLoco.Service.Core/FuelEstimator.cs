using System;
using Microsoft.Extensions.Options;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using static GCommon.Messages.Fuel;

namespace NewLoco.Service.Core
{
    public sealed class FuelEstimator(IOptions<FuelPoliciesOptions> policies) : IFuelEstimator
    {
        private readonly FuelPoliciesOptions _policies =
            (policies ?? throw new ArgumentNullException(nameof(policies))).Value
            ?? throw new ArgumentException(Error_PoliciesNotConfigured);

        public FuelEstimate EstimateDefault(LocomotiveType type, decimal amount, MeasuringUnits unit)
        {
            // Km → manual fuel entry only
            if (unit == MeasuringUnits.Km)
                return new FuelEstimate(0m, 0m, 0m);

            // Mh → automatic estimation
            if (amount <= 0)
                return new FuelEstimate(0m, 0m, 0m);

            var p = (type == LocomotiveType.Shunter ? _policies.Shunter : _policies.Mainline)
                ?? throw new InvalidOperationException(string.Format(Error_PolicyMissingFmt, type));

            var rate = Math.Max(p.MinIdleLph, p.MinLoadLph);
            var liters = Math.Round(rate * amount, 1, MidpointRounding.AwayFromZero);

            return new FuelEstimate(liters, rate, p.FullLoadLphHint);
        }
    }
}