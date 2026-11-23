using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CinemaSystem.Booking.Api.Data;

public class SqlServerCinemaDbContextFactory
    : IDesignTimeDbContextFactory<SqlServerCinemaDbContext>
{
    public SqlServerCinemaDbContext CreateDbContext(string[] args)
    {
        var options =
            new DbContextOptionsBuilder<SqlServerCinemaDbContext>();

        // Used for generating migrations only.
        // Generating a migration does not connect to this server.
        options.UseSqlServer(
            "Server=localhost;Database=CinemaMigrationDesign;" +
            "Integrated Security=True;Encrypt=True;");

        return new SqlServerCinemaDbContext(options.Options);
    }
}