namespace CinemaSystem.Booking.Api.Pricing;

public interface IPricingClient
{
    Task<PricingQuoteResponse> GetQuoteAsync(
        PricingQuoteRequest request,
        CancellationToken cancellationToken = default);
}