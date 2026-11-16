using CinemaSystem.Pricing.Api.Strategies;

namespace CinemaSystem.Tests;

public class TicketPriceCalculatorTests
{
    [Fact]
    public void SetStrategy_ChangesFromAdultToStudent_UsesSelectedPricing()
    {
        var calculator = new TicketPriceCalculator(
            new AdultPricingStrategy());

        Assert.Equal(20m, calculator.CalculatePrice(10m, 2));

        calculator.SetStrategy(new StudentPricingStrategy());

        Assert.Equal(16m, calculator.CalculatePrice(10m, 2));
    }
}