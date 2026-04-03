using System;
using Microsoft.Extensions.Options;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core;
using NewLoco.Service.Core.Contracts;
using Xunit;

namespace NewLoco.Tests;
public class FuelEstimatorTests
{
    private static IOptions<FuelPoliciesOptions> Opt(FuelPoliciesOptions p)
        => Options.Create(p);

    [Fact]
    public void EstimateDefault_ShouldReturnZero_WhenUnitIsKm()
    {
        var policies = new FuelPoliciesOptions
        {
            Mainline = new FuelPolicy { MinIdleLph = 1, MinLoadLph = 2, FullLoadLphHint = 3 }
        };

        var est = new FuelEstimator(Opt(policies));

        var result = est.EstimateDefault(LocomotiveType.Mainline, 100, MeasuringUnits.Km);

        Assert.Equal(0, result.SuggestedLiters);
        Assert.Equal(0, result.PolicyRate);
        Assert.Equal(0, result.PolicyFullHint);
    }

    [Fact]
    public void EstimateDefault_ShouldReturnZero_WhenAmountIsZero()
    {
        var policies = new FuelPoliciesOptions
        {
            Mainline = new FuelPolicy { MinIdleLph = 1, MinLoadLph = 2, FullLoadLphHint = 3 }
        };

        var est = new FuelEstimator(Opt(policies));

        var result = est.EstimateDefault(LocomotiveType.Mainline, 0, MeasuringUnits.Mh);

        Assert.Equal(0, result.SuggestedLiters);
        Assert.Equal(0, result.PolicyRate);
        Assert.Equal(0, result.PolicyFullHint);
    }

    [Fact]
    public void EstimateDefault_ShouldUseMainlinePolicy()
    {
        var policies = new FuelPoliciesOptions
        {
            Mainline = new FuelPolicy { MinIdleLph = 5, MinLoadLph = 10, FullLoadLphHint = 20 }
        };

        var est = new FuelEstimator(Opt(policies));

        var result = est.EstimateDefault(LocomotiveType.Mainline, 2, MeasuringUnits.Mh);

        Assert.Equal(20, result.PolicyFullHint);
        Assert.Equal(10, result.PolicyRate);
        Assert.Equal(20m, result.SuggestedLiters);
    }

    [Fact]
    public void EstimateDefault_ShouldUseShunterPolicy()
    {
        var policies = new FuelPoliciesOptions
        {
            Shunter = new FuelPolicy { MinIdleLph = 3, MinLoadLph = 4, FullLoadLphHint = 8 }
        };

        var est = new FuelEstimator(Opt(policies));

        var result = est.EstimateDefault(LocomotiveType.Shunter, 2, MeasuringUnits.Mh);

        Assert.Equal(8, result.PolicyFullHint);
        Assert.Equal(4, result.PolicyRate);
        Assert.Equal(8m, result.SuggestedLiters);
    }

    [Fact]
    public void EstimateDefault_ShouldRoundLitersCorrectly()
    {
        var policies = new FuelPoliciesOptions
        {
            Mainline = new FuelPolicy { MinIdleLph = 1.23m, MinLoadLph = 1.27m, FullLoadLphHint = 3 }
        };

        var est = new FuelEstimator(Opt(policies));

        var result = est.EstimateDefault(LocomotiveType.Mainline, 3.5m, MeasuringUnits.Mh);

        Assert.Equal(3, result.PolicyFullHint);
        Assert.Equal(1.27m, result.PolicyRate);
        Assert.Equal(4.4m, result.SuggestedLiters);
    }

 
    [Fact]
    public void EstimateDefault_ShouldNotThrow_WhenPolicyIsEmpty()
    {
        var policies = new FuelPoliciesOptions
        {
            Mainline = new FuelPolicy() // empty but present
        };

        var est = new FuelEstimator(Options.Create(policies));

        var result = est.EstimateDefault(LocomotiveType.Mainline, 10, MeasuringUnits.Mh);

        Assert.NotNull(result);
        Assert.Equal(0m, result.SuggestedLiters);
        Assert.Equal(0m, result.PolicyRate);
        Assert.Equal(0m, result.PolicyFullHint);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FuelEstimator(null!));

    }
}