using System.Net.Http.Json;

namespace CinemaSystem.Booking.Api.Pricing;

public class PricingClient : IPricingClient
{
    private readonly HttpClient _httpClient;

    public PricingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PricingQuoteResponse> GetQuoteAsync(
        PricingQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/pricing/quote",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var quote = await response.Content
            .ReadFromJsonAsync<PricingQuoteResponse>(
                cancellationToken: cancellationToken);

        return quote ?? throw new InvalidOperationException(
            "The Pricing API returned an empty response.");
    }
}