using System;
using Microsoft.Extensions.Options;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using static GCommon.Messages.Fuel;

namespace NewLoco.Service.Core
{
    public sealed class FuelEstimator(IOptions<FuelPoliciesOptions> policies) : IFuelEstimator
    {
        private readonly FuelPoliciesOptions _policies = (policies ?? throw new ArgumentNullException(nameof(policies))).Value
                        ?? throw new ArgumentException(Error_PoliciesNotConfigured);

        public FuelEstimate EstimateDefault(LocomotiveType type, decimal hours)
        {
            if (hours <= 0)
                return new FuelEstimate(0m, 0m, 0m);

            var p = (type == LocomotiveType.Shunter ? _policies.Shunter : _policies.Mainline)
                    ?? throw new InvalidOperationException(string.Format(Error_PolicyMissingFmt, type)); // changed

            var minLph = Math.Max(p.MinIdleLph, p.MinLoadLph);
            var liters = Math.Round(minLph * hours, 1, MidpointRounding.AwayFromZero);

            return new FuelEstimate(liters, minLph, p.FullLoadLphHint);
        }
    }
}