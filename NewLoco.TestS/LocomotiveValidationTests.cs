using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;
using NewLoco.GCommon;

namespace NewLoco.Tests;

public class LocomotiveValidationTests
{
    [Theory]
    [InlineData("55-001")]
    [InlineData("12-999")]
    public void LocomotiveNumber_ShouldBeValid(string number)
    {
        number.Length.Should().Be(EntityValidationConstants.Locomotive.LocomotiveNumberLength);

        Regex.IsMatch(number, EntityValidationConstants.Locomotive.LocomotiveNumberPattern)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData("5-001")]
    [InlineData("55001")]
    [InlineData("AA-001")]
    [InlineData("12-00A")]
    public void LocomotiveNumber_ShouldBeInvalid(string number)
    {
        Regex.IsMatch(number, EntityValidationConstants.Locomotive.LocomotiveNumberPattern)
            .Should().BeFalse();
    }
}