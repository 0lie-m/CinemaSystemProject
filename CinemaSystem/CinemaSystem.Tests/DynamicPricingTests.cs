using CinemaSystem.Pricing.Api.Services;
using CinemaSystem.Pricing.Api.Strategies;

namespace CinemaSystem.Tests;

public class DynamicPricingTests
{
    [Fact]
    public void CalculatePrice_NormalMovieTwoAdultTickets_ReturnsTwentyEuro()
    {
        var service = new DynamicPricingService();

        decimal total = service.CalculatePrice(
            new AdultPricingStrategy(), 10m, 2, false);

        Assert.Equal(20m, total);
    }

    [Fact]
    public void CalculatePrice_PremiumMovieTwoAdultTickets_ReturnsTwentyFourEuro()
    {
        var service = new DynamicPricingService();

        decimal total = service.CalculatePrice(
            new AdultPricingStrategy(), 10m, 2, true);

        Assert.Equal(24m, total);
    }

    [Fact]
    public void CalculatePrice_PremiumMovieTwoStudentTickets_ReturnsNineteenTwenty()
    {
        var service = new DynamicPricingService();

        decimal total = service.CalculatePrice(
            new StudentPricingStrategy(), 10m, 2, true);

        Assert.Equal(19.20m, total);
    }

    [Fact]
    public void CalculatePrice_ShowingEightyPercentFull_ReturnsTwentyTwoEuro()
    {
        var service = new DynamicPricingService();

        decimal total = service.CalculatePrice(
            new AdultPricingStrategy(), 10m, 2, false, 0.8m);

        Assert.Equal(22m, total);
    }

    [Fact]
public void CalculatePrice_BelowEightyPercentFull_NoDemandIncrease()
{
    var service = new DynamicPricingService();

    decimal total = service.CalculatePrice(
        new AdultPricingStrategy(), 10m, 2, false, 0.79m);

    Assert.Equal(20m, total);
}

[Fact]
public void CalculatePrice_PremiumStudentTicketsAtHighDemand_ReturnsTwentyOneTwelve()
{
    var service = new DynamicPricingService();

    decimal total = service.CalculatePrice(
        new StudentPricingStrategy(), 10m, 2, true, 0.8m);

    Assert.Equal(21.12m, total);
}

[Fact]
public void CalculatePrice_RoundsUnitPriceBeforeMultiplyingQuantity()
{
    var service = new DynamicPricingService();

    decimal total = service.CalculatePrice(
        new StudentPricingStrategy(), 10.03m, 3, false);

    Assert.Equal(24.06m, total);
}

[Fact]
public void CalculatePrice_NegativeBasePrice_ThrowsException()
{
    var service = new DynamicPricingService();

    Assert.Throws<ArgumentOutOfRangeException>(() =>
        service.CalculatePrice(
            new AdultPricingStrategy(), -10m, 2, false));
}

[Theory]
[InlineData(-1)]
[InlineData(101)]
public void CalculatePrice_InvalidOccupancy_ThrowsException(
    int occupancyPercentage)
{
    var service = new DynamicPricingService();

    Assert.Throws<ArgumentOutOfRangeException>(() =>
        service.CalculatePrice(
            new AdultPricingStrategy(),
            10m,
            2,
            false,
            occupancyPercentage / 100m));
}

[Fact]
public void CalculatePrice_NegativeQuantity_ThrowsException()
{
    var service = new DynamicPricingService();

    Assert.Throws<ArgumentOutOfRangeException>(() =>
        service.CalculatePrice(
            new StudentPricingStrategy(), 10m, -1, false));
}

}