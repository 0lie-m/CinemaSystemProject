using CinemaSystem.Booking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Booking.Api.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Showing> Showings => Set<Showing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
}