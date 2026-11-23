using CinemaSystem.Booking.Api.Models;
using BookingEntity = CinemaSystem.Booking.Api.Models.Booking;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Booking.Api.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<BookedTicket> BookedTickets => Set<BookedTicket>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Showing> Showings => Set<Showing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingEntity>()
            .HasOne(booking => booking.Showing)
            .WithMany()
            .HasForeignKey(booking => booking.ShowingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BookingEntity>()
            .HasMany(booking => booking.Tickets)
            .WithOne(ticket => ticket.Booking)
            .HasForeignKey(ticket => ticket.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookedTicket>()
            .Property(ticket => ticket.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<BookedTicket>()
            .Property(ticket => ticket.TicketType)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<Movie>()
            .Property(movie => movie.Title)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Screen>()
            .Property(screen => screen.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Showing>()
            .HasOne(showing => showing.Movie)
            .WithMany()
            .HasForeignKey(showing => showing.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Showing>()
            .HasOne(showing => showing.Screen)
            .WithMany()
            .HasForeignKey(showing => showing.ScreenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Showing>()
            .HasIndex(showing => new
            {
                showing.ScreenId,
                showing.StartTime
            });
    }
    // Allows a provider-specific context to reuse the shared model.
    protected CinemaDbContext(DbContextOptions options)
    : base(options)
{
}
}