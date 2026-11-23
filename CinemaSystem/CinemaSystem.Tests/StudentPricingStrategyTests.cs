using CinemaSystem.Pricing.Api.Strategies;

namespace CinemaSystem.Tests;

public class StudentPricingStrategyTests
{
    [Fact]
    public void CalculatePrice_TwoStudentTicketsAtTenEuro_ReturnsSixteenEuro()
    {
        var strategy = new StudentPricingStrategy();

        decimal total = strategy.CalculatePrice(10m, 2);

        Assert.Equal(16m, total);
    }
}