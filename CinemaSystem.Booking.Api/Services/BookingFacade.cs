using CinemaSystem.Booking.Api.Data;
using CinemaSystem.Booking.Api.Pricing;
using CinemaSystem.Core.Creators;
using Microsoft.EntityFrameworkCore;
using BookingEntity = CinemaSystem.Booking.Api.Models.Booking;
using BookedTicket = CinemaSystem.Booking.Api.Models.BookedTicket;

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

    public async Task<BookingEntity> BookAsync(
        int showingId,
        string ticketType,
        int quantity,
        CancellationToken cancellationToken = default)
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

        var normalisedType = ticketType?.Trim().ToLowerInvariant();

        TicketCreator creator = normalisedType switch
        {
            "adult" => new AdultTicketCreator(),
            "student" => new StudentTicketCreator(),
            "child" => new ChildTicketCreator(),
            _ => throw new ArgumentException(
                "Ticket type must be adult, student or child.",
                nameof(ticketType))
        };

        var showing = await _db.Showings
            .AsNoTracking()
            .Include(item => item.Movie)
            .Include(item => item.Screen)
            .SingleOrDefaultAsync(
                item => item.Id == showingId,
                cancellationToken);

        if (showing is null)
        {
            throw new KeyNotFoundException("Showing was not found.");
        }

        var soldTickets = await _db.BookedTickets.CountAsync(
            ticket => ticket.Booking.ShowingId == showingId,
            cancellationToken);

        var availableSeats = showing.Screen.Capacity - soldTickets;

        if (quantity > availableSeats)
        {
            throw new InvalidOperationException(
                "There are not enough seats available.");
        }

        var occupancyRate =
            (decimal)soldTickets / showing.Screen.Capacity;

        var request = new PricingQuoteRequest(
            normalisedType!,
            showing.BasePrice,
            quantity,
            showing.Movie.IsPremium,
            occupancyRate);

        // Complete the HTTP request before taking a database write lock.
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

        // With the current SQLite provider, this transaction protects
        // the final availability check and booking save.
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var currentShowing = await _db.Showings
            .AsNoTracking()
            .Include(item => item.Movie)
            .Include(item => item.Screen)
            .SingleOrDefaultAsync(
                item => item.Id == showingId,
                cancellationToken);

        if (currentShowing is null)
        {
            throw new KeyNotFoundException("Showing was not found.");
        }

        var currentSoldTickets = await _db.BookedTickets.CountAsync(
            ticket => ticket.Booking.ShowingId == showingId,
            cancellationToken);

        if (quantity > currentShowing.Screen.Capacity - currentSoldTickets)
        {
            throw new InvalidOperationException(
                "There are not enough seats available.");
        }

        // Demand or showing changes can affect the quoted price.
        if (currentSoldTickets != soldTickets ||
            currentShowing.Screen.Capacity != showing.Screen.Capacity ||
            currentShowing.BasePrice != showing.BasePrice ||
            currentShowing.Movie.IsPremium != showing.Movie.IsPremium)
        {
            throw new InvalidOperationException(
                "Showing availability or pricing changed. Please try again.");
        }

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
                TicketType = normalisedType!,
                PricePaid = ticket.PricePaid
            });
        }

        _db.Bookings.Add(booking);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return booking;
    }
}