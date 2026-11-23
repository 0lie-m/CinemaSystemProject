using CinemaSystem.Booking.Api.Models;
using Microsoft.EntityFrameworkCore;
using BookingEntity = CinemaSystem.Booking.Api.Models.Booking;

namespace CinemaSystem.Booking.Api.Data;

public class SqlServerCinemaDbContext : CinemaDbContext
{
    public SqlServerCinemaDbContext(
        DbContextOptions<SqlServerCinemaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store monetary values with two decimal places.
        modelBuilder.Entity<Showing>()
            .Property(showing => showing.BasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingEntity>()
            .Property(booking => booking.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookedTicket>()
            .Property(ticket => ticket.PricePaid)
            .HasPrecision(18, 2);
    }
}