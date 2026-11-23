using CinemaSystem.Pricing.Api.Services;
using CinemaSystem.Pricing.Api.Strategies;
using CinemaSystem.Pricing.Api.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<DynamicPricingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/pricing/quote", IResult (
    PricingRequest request,
    DynamicPricingService pricingService) =>
{
    if (request.Quantity <= 0)
    {
        return Results.BadRequest(
            new { error = "Quantity must be greater than zero." });
    }

    if (request.BasePrice < 0)
    {
        return Results.BadRequest(
            new { error = "Base price cannot be negative." });
    }

    if (request.OccupancyRate < 0m || request.OccupancyRate > 1m)
    {
        return Results.BadRequest(
            new { error = "Occupancy must be between 0 and 1." });
    }

    ITicketPricingStrategy? strategy =
        request.TicketType?.Trim().ToLowerInvariant() switch
        {
            "adult" => new AdultPricingStrategy(),
            "student" => new StudentPricingStrategy(),
            "child" => new ChildPricingStrategy(),
            _ => null
        };

    if (strategy is null)
    {
        return Results.BadRequest(
            new { error = "Ticket type must be adult, student or child." });
    }

    decimal totalPrice = pricingService.CalculatePrice(
        strategy,
        request.BasePrice,
        request.Quantity,
        request.IsPremiumMovie,
        request.OccupancyRate);

    decimal unitPrice = totalPrice / request.Quantity;

    return Results.Ok(new PricingResponse(
        unitPrice,
        request.Quantity,
        totalPrice));
})
.WithName("GetTicketPriceQuote");

await app.RunAsync();

