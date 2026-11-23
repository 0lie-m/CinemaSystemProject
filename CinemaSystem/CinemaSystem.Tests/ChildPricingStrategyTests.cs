using CinemaSystem.Pricing.Api.Strategies;

namespace CinemaSystem.Tests;

public class ChildPricingStrategyTests
{
    [Fact]
    public void CalculatePrice_TwoChildTicketsAtTenEuro_ReturnsFourteenEuro()
    {
        var strategy = new ChildPricingStrategy();

        decimal total = strategy.CalculatePrice(10m, 2);

        Assert.Equal(14m, total);
    }
}