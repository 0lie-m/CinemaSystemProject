using CinemaSystem.Booking.Api.Data;
using CinemaSystem.Booking.Api.Models;
using CinemaSystem.Booking.Api.Pricing;
using CinemaSystem.Core.Creators;
using Microsoft.EntityFrameworkCore;
using BookingEntity = CinemaSystem.Booking.Api.Models.Booking;

namespace CinemaSystem.Booking.Api.Services;

public class BookingFacade
{
    private readonly CinemaDbContext _db;
    private readonly IPricingClient _pricingClient;

    public BookingFacade(
        CinemaDbContext db,
        IPricingClient pricingClient)
    {
        _db = db;
        _pricingClient = pricingClient;
    }

    // Preview the price without creating a booking or reserving seats.
    public async Task<PricingQuoteResponse> QuoteAsync(
        int showingId,
        string ticketType,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var context = await PrepareQuoteAsync(
            showingId,
            ticketType,
            quantity,
            cancellationToken);

        return context.Quote;
    }

    public async Task<BookingEntity> BookAsync(
        int showingId,
        string ticketType,
        int quantity,
        CancellationToken cancellationToken = default,
        decimal? expectedTotalPrice = null)
    {
        if (expectedTotalPrice.HasValue &&
            expectedTotalPrice.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedTotalPrice),
                "Expected total price cannot be negative.");
        }

        // Request a fresh price before confirming the booking.
        var context = await PrepareQuoteAsync(
            showingId,
            ticketType,
            quantity,
            cancellationToken);

        var showing = context.Showing;
        var quote = context.Quote;

        // The supplied price is only compared with the server's price.
        // It is never used to calculate or save a cheaper booking.
        if (expectedTotalPrice.HasValue &&
            quote.TotalPrice != expectedTotalPrice.Value)
        {
            throw new InvalidOperationException(
                "The ticket price has changed. Request a new quote before confirming.");
        }

        // Keep the final availability check and save together.
        // The HTTP pricing request finishes before this transaction starts.
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var currentShowing = await LoadShowingAsync(
            showingId,
            cancellationToken);

        var currentSoldTickets = await CountSoldTicketsAsync(
            showingId,
            cancellationToken);

        EnsureAvailable(currentShowing, currentSoldTickets, quantity);

        // Reject a quote if relevant data changed during the pricing request.
        if (currentSoldTickets != context.SoldTickets ||
            currentShowing.ScreenId != showing.ScreenId ||
            currentShowing.MovieId != showing.MovieId ||
            currentShowing.StartTime != showing.StartTime ||
            currentShowing.EndTime != showing.EndTime ||
            currentShowing.Screen.Capacity != showing.Screen.Capacity ||
            currentShowing.BasePrice != showing.BasePrice ||
            currentShowing.Movie.IsPremium != showing.Movie.IsPremium)
        {
            throw new InvalidOperationException(
                "Showing availability or pricing changed. Request a new quote.");
        }

        var creator = CreateTicketCreator(context.TicketType);

        var booking = new BookingEntity
        {
            ShowingId = showingId,
            CreatedAtUtc = DateTime.UtcNow,
            TotalPrice = quote.TotalPrice
        };

        for (var index = 0; index < quantity; index++)
        {
            var ticket = creator.IssueTicket(
                showingId,
                quote.UnitPrice);

            booking.Tickets.Add(new BookedTicket
            {
                Id = ticket.Id,
                TicketType = context.TicketType,
                PricePaid = ticket.PricePaid
            });
        }

        _db.Bookings.Add(booking);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return booking;
    }

    private async Task<QuoteContext> PrepareQuoteAsync(
        int showingId,
        string ticketType,
        int quantity,
        CancellationToken cancellationToken)
    {
        if (showingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(showingId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        var normalisedType =
            ticketType?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalisedType != "adult" &&
            normalisedType != "student" &&
            normalisedType != "child")
        {
            throw new ArgumentException(
                "Ticket type must be adult, student or child.",
                nameof(ticketType));
        }

        var showing = await LoadShowingAsync(
            showingId,
            cancellationToken);

        var soldTickets = await CountSoldTicketsAsync(
            showingId,
            cancellationToken);

        EnsureAvailable(showing, soldTickets, quantity);

        var occupancyRate =
            (decimal)soldTickets / showing.Screen.Capacity;

        var request = new PricingQuoteRequest(
            normalisedType,
            showing.BasePrice,
            quantity,
            showing.Movie.IsPremium,
            occupancyRate);

        var quote = await _pricingClient.GetQuoteAsync(
            request,
            cancellationToken);

        if (quote.Quantity != quantity ||
            quote.UnitPrice < 0 ||
            decimal.Round(quote.UnitPrice, 2) != quote.UnitPrice ||
            quote.TotalPrice != quote.UnitPrice * quantity)
        {
            throw new InvalidOperationException(
                "The Pricing API returned an invalid quote.");
        }

        return new QuoteContext(
            showing,
            soldTickets,
            normalisedType,
            quote);
    }

    private async Task<Showing> LoadShowingAsync(
        int showingId,
        CancellationToken cancellationToken)
    {
        var showing = await _db.Showings
            .AsNoTracking()
            .Include(item => item.Movie)
            .Include(item => item.Screen)
            .SingleOrDefaultAsync(
                item => item.Id == showingId,
                cancellationToken);

        return showing
            ?? throw new KeyNotFoundException("Showing was not found.");
    }

    private Task<int> CountSoldTicketsAsync(
        int showingId,
        CancellationToken cancellationToken)
    {
        return _db.BookedTickets.CountAsync(
            ticket => ticket.Booking.ShowingId == showingId,
            cancellationToken);
    }

    private static void EnsureAvailable(
        Showing showing,
        int soldTickets,
        int quantity)
    {
        if (showing.Screen.Capacity <= 0)
        {
            throw new InvalidOperationException(
                "The screen has an invalid seating capacity.");
        }

        if (quantity > showing.Screen.Capacity - soldTickets)
        {
            throw new InvalidOperationException(
                "There are not enough seats available.");
        }
    }

    private static TicketCreator CreateTicketCreator(string ticketType)
    {
        return ticketType switch
        {
            "adult" => new AdultTicketCreator(),
            "student" => new StudentTicketCreator(),
            "child" => new ChildTicketCreator(),
            _ => throw new ArgumentException("Invalid ticket type.")
        };
    }

    private sealed record QuoteContext(
        Showing Showing,
        int SoldTickets,
        string TicketType,
        PricingQuoteResponse Quote);
}