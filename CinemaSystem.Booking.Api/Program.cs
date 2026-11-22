using CinemaSystem.Booking.Api.Contracts;
using CinemaSystem.Booking.Api.Data;
using CinemaSystem.Booking.Api.Models;
using CinemaSystem.Booking.Api.Pricing;
using CinemaSystem.Booking.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = Path.Combine(
        builder.Environment.ContentRootPath,
        "cinema.db")
};

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlite(connectionString.ToString()));

builder.Services.AddHttpClient<IPricingClient, PricingClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:PricingApi"]
        ?? "http://localhost:5104");

    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<BookingFacade>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () =>
    Results.Ok(new { service = "Booking API", status = "Running" }));

app.MapGet("/api/movies", async (CinemaDbContext db) =>
{
    var movies = await db.Movies
        .AsNoTracking()
        .OrderBy(movie => movie.Title)
        .ToListAsync();

    return Results.Ok(movies);
});

app.MapGet("/api/movies/{id:int}", async Task<IResult> (
    int id, CinemaDbContext db) =>
{
    var movie = await db.Movies
        .AsNoTracking()
        .FirstOrDefaultAsync(movie => movie.Id == id);

    if (movie is null)
    {
        return Results.NotFound(new { error = "Movie not found." });
    }

    return Results.Ok(movie);
});

app.MapPost("/api/movies", async Task<IResult> (
    CreateMovieRequest request, CinemaDbContext db) =>
{
    string title = request.Title?.Trim() ?? string.Empty;

    if (title.Length == 0 || title.Length > 200)
    {
        return Results.BadRequest(
            new { error = "Movie title must contain 1 to 200 characters." });
    }

    if (request.DurationMinutes <= 0 || request.DurationMinutes > 1440)
    {
        return Results.BadRequest(
            new { error = "Duration must be between 1 and 1440 minutes." });
    }

    var movie = new Movie
    {
        Title = title,
        DurationMinutes = request.DurationMinutes,
        IsPremium = request.IsPremium
    };

    db.Movies.Add(movie);
    await db.SaveChangesAsync();

    return Results.Created($"/api/movies/{movie.Id}", movie);
});

app.MapGet("/api/screens", async (CinemaDbContext db) =>
{
    var screens = await db.Screens
        .AsNoTracking()
        .OrderBy(screen => screen.Name)
        .ToListAsync();

    return Results.Ok(screens);
});

app.MapGet("/api/screens/{id:int}", async Task<IResult> (
    int id, CinemaDbContext db) =>
{
    var screen = await db.Screens
        .AsNoTracking()
        .FirstOrDefaultAsync(screen => screen.Id == id);

    if (screen is null)
    {
        return Results.NotFound(new { error = "Screen not found." });
    }

    return Results.Ok(screen);
});

app.MapPost("/api/screens", async Task<IResult> (
    CreateScreenRequest request, CinemaDbContext db) =>
{
    string name = request.Name?.Trim() ?? string.Empty;

    if (name.Length == 0 || name.Length > 100)
    {
        return Results.BadRequest(
            new { error = "Screen name must contain 1 to 100 characters." });
    }

    if (request.Capacity <= 0)
    {
        return Results.BadRequest(
            new { error = "Screen capacity must be greater than zero." });
    }

    var screen = new Screen
    {
        Name = name,
        Capacity = request.Capacity
    };

    db.Screens.Add(screen);
    await db.SaveChangesAsync();

    return Results.Created($"/api/screens/{screen.Id}", screen);
});

app.MapGet("/api/showings", async (CinemaDbContext db) =>
{
    var showings = await db.Showings
        .AsNoTracking()
        .OrderBy(showing => showing.StartTime)
        .Select(showing => new
        {
            showing.Id,
            showing.MovieId,
            MovieTitle = showing.Movie.Title,
            showing.ScreenId,
            ScreenName = showing.Screen.Name,
            showing.StartTime,
            showing.EndTime,
            showing.BasePrice,
            Capacity = showing.Screen.Capacity,
            SeatsSold = db.BookedTickets.Count(
                ticket => ticket.Booking.ShowingId == showing.Id),
            AvailableSeats = showing.Screen.Capacity -
                db.BookedTickets.Count(
                    ticket => ticket.Booking.ShowingId == showing.Id)
        })
        .ToListAsync();

    return Results.Ok(showings);
});

app.MapPost("/api/showings", async Task<IResult> (
    CreateShowingRequest request, CinemaDbContext db) =>
{
    if (request.MovieId <= 0 || request.ScreenId <= 0)
    {
        return Results.BadRequest(
            new { error = "Movie and screen IDs must be greater than zero." });
    }

    if (request.StartTime == default ||
        request.StartTime.Kind != DateTimeKind.Unspecified)
    {
        return Results.BadRequest(
            new { error = "Provide a cinema local start time without a timezone suffix." });
    }

    if (request.BasePrice < 0 ||
        decimal.Round(request.BasePrice, 2) != request.BasePrice)
    {
        return Results.BadRequest(
            new { error = "Base price must be non-negative with at most two decimal places." });
    }

    await using var transaction =
        await db.Database.BeginTransactionAsync();

    var movie = await db.Movies.FindAsync(request.MovieId);

    if (movie is null)
    {
        return Results.NotFound(new { error = "Movie not found." });
    }

    var screen = await db.Screens.FindAsync(request.ScreenId);

    if (screen is null)
    {
        return Results.NotFound(new { error = "Screen not found." });
    }

    if (request.StartTime >
        DateTime.MaxValue.AddMinutes(-movie.DurationMinutes))
    {
        return Results.BadRequest(
            new { error = "Start time is outside the supported range." });
    }

    DateTime endTime =
        request.StartTime.AddMinutes(movie.DurationMinutes);

    bool overlaps = await db.Showings.AnyAsync(showing =>
        showing.ScreenId == request.ScreenId &&
        showing.StartTime < endTime &&
        showing.EndTime > request.StartTime);

    if (overlaps)
    {
        return Results.Conflict(
            new { error = "This screen already has a showing during that time." });
    }

    var showing = new Showing
    {
        MovieId = request.MovieId,
        ScreenId = request.ScreenId,
        StartTime = request.StartTime,
        EndTime = endTime,
        BasePrice = request.BasePrice
    };

    db.Showings.Add(showing);
    await db.SaveChangesAsync();
    await transaction.CommitAsync();

    return Results.Created("/api/showings", new
    {
        showing.Id,
        showing.MovieId,
        showing.ScreenId,
        showing.StartTime,
        showing.EndTime,
        showing.BasePrice
    });
});

app.MapGet("/api/bookings", async (CinemaDbContext db) =>
{
    var bookings = await db.Bookings
        .AsNoTracking()
        .OrderByDescending(booking => booking.CreatedAtUtc)
        .Select(booking => new
        {
            booking.Id,
            booking.ShowingId,
            MovieTitle = booking.Showing.Movie.Title,
            ScreenName = booking.Showing.Screen.Name,
            booking.Showing.StartTime,
            booking.CreatedAtUtc,
            booking.TotalPrice,
            Quantity = booking.Tickets.Count
        })
        .ToListAsync();

    return Results.Ok(bookings);
});

app.MapGet("/api/bookings/{id:int}", async Task<IResult> (
    int id, CinemaDbContext db) =>
{
    var booking = await db.Bookings
        .AsNoTracking()
        .Where(item => item.Id == id)
        .Select(item => new
        {
            item.Id,
            item.ShowingId,
            MovieTitle = item.Showing.Movie.Title,
            ScreenName = item.Showing.Screen.Name,
            item.Showing.StartTime,
            item.CreatedAtUtc,
            item.TotalPrice,
            Quantity = item.Tickets.Count,
            Tickets = item.Tickets.Select(ticket => new
            {
                ticket.Id,
                ticket.TicketType,
                ticket.PricePaid
            }).ToList()
        })
        .SingleOrDefaultAsync();

    if (booking is null)
    {
        return Results.NotFound(new { error = "Booking not found." });
    }

    return Results.Ok(booking);
});

app.MapPost("/api/bookings/quote", async Task<IResult> (
    CreateBookingQuoteRequest request,
    BookingFacade facade,
    CancellationToken cancellationToken) =>
{
    try
    {
        var quote = await facade.QuoteAsync(
            request.ShowingId,
            request.TicketType ?? string.Empty,
            request.Quantity,
            cancellationToken);

        return Results.Ok(quote);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { error = exception.Message });
    }
    catch (HttpRequestException)
    {
        return Results.Json(
            new
            {
                error = "The pricing service is unavailable. Please try again."
            },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (OperationCanceledException)
        when (!cancellationToken.IsCancellationRequested)
    {
        return Results.Json(
            new
            {
                error = "The pricing service timed out. Please try again."
            },
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
});

app.MapPost("/api/bookings", async Task<IResult> (
    CreateBookingRequest request,
    BookingFacade facade,
    CancellationToken cancellationToken) =>
{
    if (!request.ExpectedTotalPrice.HasValue ||
        request.ExpectedTotalPrice.Value < 0)
    {
        return Results.BadRequest(new
        {
            error = "Provide the non-negative total price accepted from a quote."
        });
    }

    try
    {
        var booking = await facade.BookAsync(
            request.ShowingId,
            request.TicketType ?? string.Empty,
            request.Quantity,
            cancellationToken,
            expectedTotalPrice: request.ExpectedTotalPrice.Value);

        return Results.Created($"/api/bookings/{booking.Id}", new
        {
            booking.Id,
            booking.ShowingId,
            booking.CreatedAtUtc,
            booking.TotalPrice,
            Quantity = booking.Tickets.Count,
            Tickets = booking.Tickets.Select(ticket => new
            {
                ticket.Id,
                ticket.TicketType,
                ticket.PricePaid
            }).ToArray()
        });
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { error = exception.Message });
    }
    catch (HttpRequestException)
    {
        return Results.Json(
            new
            {
                error = "The pricing service is unavailable. Please try again."
            },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (OperationCanceledException)
        when (!cancellationToken.IsCancellationRequested)
    {
        return Results.Json(
            new
            {
                error = "The pricing service timed out. Please try again."
            },
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
});

app.Run();