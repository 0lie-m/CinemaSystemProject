using CinemaSystem.Booking.Api.Data;
using CinemaSystem.Booking.Api.Models;
using CinemaSystem.Booking.Api.Pricing;
using CinemaSystem.Booking.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CinemaSystem.Tests;

public class BookingFacadeTests
{
    [Fact]
    public async Task BookAsync_EnoughSeats_SavesBookingAndTickets()
    {
        using var database = new TestDatabase();
        var pricing = new FakePricingClient();
        var facade = new BookingFacade(database.Db, pricing);

        // Booking exactly the remaining capacity should be allowed.
        var booking = await facade.BookAsync(
            database.ShowingId, "adult", 2);

        Assert.True(booking.Id > 0);
        Assert.Equal(database.ShowingId, booking.ShowingId);
        Assert.Equal(24m, booking.TotalPrice);
        Assert.Equal(2, booking.Tickets.Count);

        Assert.All(booking.Tickets, ticket =>
        {
            Assert.NotEqual(Guid.Empty, ticket.Id);
            Assert.Equal("adult", ticket.TicketType);
            Assert.Equal(12m, ticket.PricePaid);
        });

        Assert.Equal(
            2,
            booking.Tickets.Select(ticket => ticket.Id).Distinct().Count());

        Assert.Equal(1, pricing.CallCount);
        Assert.NotNull(pricing.LastRequest);
        Assert.Equal("adult", pricing.LastRequest!.TicketType);
        Assert.Equal(10m, pricing.LastRequest.BasePrice);
        Assert.Equal(2, pricing.LastRequest.Quantity);
        Assert.True(pricing.LastRequest.IsPremiumMovie);
        Assert.Equal(0m, pricing.LastRequest.OccupancyRate);

        // Read back from the database to verify persistence.
        database.Db.ChangeTracker.Clear();

        var saved = await database.Db.Bookings
            .Include(item => item.Tickets)
            .SingleAsync();

        Assert.Equal(24m, saved.TotalPrice);
        Assert.Equal(2, saved.Tickets.Count);
    }

    [Fact]
    public async Task BookAsync_NotEnoughSeats_DoesNotSaveOrRequestPrice()
    {
        using var database = new TestDatabase();
        var pricing = new FakePricingClient();
        var facade = new BookingFacade(database.Db, pricing);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => facade.BookAsync(database.ShowingId, "adult", 3));

        Assert.Equal(0, pricing.CallCount);
        Assert.Equal(0, await database.Db.Bookings.CountAsync());
        Assert.Equal(0, await database.Db.BookedTickets.CountAsync());
    }

    [Fact]
    public async Task BookAsync_PricingServiceFails_DoesNotSaveBooking()
    {
        using var database = new TestDatabase();
        var pricing = new FakePricingClient
        {
            ShouldFail = true
        };

        var facade = new BookingFacade(database.Db, pricing);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => facade.BookAsync(database.ShowingId, "adult", 2));

        Assert.Equal(1, pricing.CallCount);
        Assert.Equal(0, await database.Db.Bookings.CountAsync());
        Assert.Equal(0, await database.Db.BookedTickets.CountAsync());
    }

    private sealed class FakePricingClient : IPricingClient
    {
        public bool ShouldFail { get; set; }
        public int CallCount { get; private set; }
        public PricingQuoteRequest? LastRequest { get; private set; }

        public Task<PricingQuoteResponse> GetQuoteAsync(
            PricingQuoteRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastRequest = request;

            if (ShouldFail)
            {
                throw new HttpRequestException(
                    "Pricing service is unavailable.");
            }

            return Task.FromResult(
                new PricingQuoteResponse(
                    12m,
                    request.Quantity,
                    12m * request.Quantity));
        }
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public CinemaDbContext Db { get; }
        public int ShowingId { get; }

        public TestDatabase()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<CinemaDbContext>()
                .UseSqlite(_connection)
                .Options;

            Db = new CinemaDbContext(options);
            Db.Database.EnsureCreated();

            var startTime = DateTime.Today.AddDays(7).AddHours(18);

            var showing = new Showing
            {
                Movie = new Movie
                {
                    Title = "Test Movie",
                    DurationMinutes = 120,
                    IsPremium = true
                },
                Screen = new Screen
                {
                    Name = "Test Screen",
                    Capacity = 2
                },
                StartTime = startTime,
                EndTime = startTime.AddMinutes(120),
                BasePrice = 10m
            };

            Db.Showings.Add(showing);
            Db.SaveChanges();

            ShowingId = showing.Id;
        }

        public void Dispose()
        {
            Db.Dispose();
            _connection.Dispose();
        }
    }
}