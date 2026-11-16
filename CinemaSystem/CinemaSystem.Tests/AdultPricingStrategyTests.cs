using CinemaSystem.Pricing.Api.Strategies;

namespace CinemaSystem.Tests;

public class AdultPricingStrategyTests
{
    [Fact]
    public void CalculatePrice_TwoAdultTicketsAtTenEuro_ReturnsTwentyEuro()
    {
        var strategy = new AdultPricingStrategy();

        decimal total = strategy.CalculatePrice(10m, 2);

        Assert.Equal(20m, total);
    }

    [Fact]
public void CalculatePrice_NegativeQuantity_ThrowsArgumentOutOfRangeException()
{
    var strategy = new AdultPricingStrategy();

    Assert.Throws<ArgumentOutOfRangeException>(
        () => strategy.CalculatePrice(10m, -1));
}
}


