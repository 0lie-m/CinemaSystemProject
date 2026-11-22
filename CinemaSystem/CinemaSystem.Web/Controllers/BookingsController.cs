using System.Net;
using System.Net.Http.Json;
using CinemaSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaSystem.Web.Controllers;

public class BookingsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BookingsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            var bookings = await client
                .GetFromJsonAsync<List<BookingDetailsViewModel>>(
                    "api/bookings", cancellationToken);

            if (bookings is null)
            {
                throw new HttpRequestException("Empty bookings response.");
            }

            return View(bookings);
        }
        catch (HttpRequestException)
        {
            ViewBag.ApiError =
                "Bookings could not be loaded. Check that the Booking API is running.";
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.ApiError =
                "Loading bookings timed out. Please try again.";
        }

        return View(new List<BookingDetailsViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = new BookingFormViewModel();

        await LoadShowingsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Quote(
        BookingFormViewModel model,
        CancellationToken cancellationToken)
    {
        await LoadShowingsAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            using var response = await client.PostAsJsonAsync(
                "api/bookings/quote",
                new
                {
                    ShowingId = model.ShowingId!.Value,
                    model.TicketType,
                    model.Quantity
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                AddApiError(response.StatusCode);
                return View("Create", model);
            }

            var quote = await response.Content
                .ReadFromJsonAsync<BookingQuoteViewModel>(
                    cancellationToken: cancellationToken);

            if (quote is null)
            {
                throw new HttpRequestException("Empty pricing response.");
            }

            model.Quote = quote;
            model.ExpectedTotalPrice = quote.TotalPrice;

            // Use the server's total rather than a previously posted value.
            ModelState.Remove(nameof(model.ExpectedTotalPrice));

            model.ShowingDescription = model.Showings
                .FirstOrDefault(item =>
                    item.Value == model.ShowingId.Value.ToString())
                ?.Text ?? $"Showing {model.ShowingId}";

            return View("Review", model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The price could not be loaded. Please try again.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(
                string.Empty,
                "The price request timed out. Please try again.");
        }

        return View("Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(
        BookingFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!model.ExpectedTotalPrice.HasValue ||
            model.ExpectedTotalPrice.Value < 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Request a price quote before confirming.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("BookingApi");

                using var response = await client.PostAsJsonAsync(
                    "api/bookings",
                    new
                    {
                        ShowingId = model.ShowingId!.Value,
                        model.TicketType,
                        model.Quantity,
                        model.ExpectedTotalPrice
                    },
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var booking = await response.Content
                        .ReadFromJsonAsync<BookingDetailsViewModel>(
                            cancellationToken: cancellationToken);

                    if (booking is null)
                    {
                        TempData["WarningMessage"] =
                            "The booking was saved, but its details could not be read. Check the list before making another booking.";

                        return RedirectToAction(nameof(Index));
                    }

                    TempData["SuccessMessage"] =
                        "Booking confirmed. Your tickets have been saved.";

                    return RedirectToAction(
                        nameof(Details), new { id = booking.Id });
                }

                AddApiError(response.StatusCode);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to confirm whether the booking was saved. Open Bookings and check before submitting again.");
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The request timed out. Open Bookings and check before submitting again.");
            }
        }

        // A failed confirmation requires a fresh quote.
        model.ExpectedTotalPrice = null;
        ModelState.Remove(nameof(model.ExpectedTotalPrice));

        await LoadShowingsAsync(model, cancellationToken);

        return View("Create", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            using var response = await client.GetAsync(
                $"api/bookings/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["WarningMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }

            response.EnsureSuccessStatusCode();

            var booking = await response.Content
                .ReadFromJsonAsync<BookingDetailsViewModel>(
                    cancellationToken: cancellationToken);

            if (booking is null)
            {
                throw new HttpRequestException("Empty booking response.");
            }

            return View(booking);
        }
        catch (HttpRequestException)
        {
            TempData["WarningMessage"] =
                $"Booking {id} could not be loaded. Try opening its details again; do not create another booking.";
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TempData["WarningMessage"] =
                $"Loading booking {id} timed out. Try opening its details again.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadShowingsAsync(
        BookingFormViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            var showings = await client
                .GetFromJsonAsync<List<ShowingViewModel>>(
                    "api/showings", cancellationToken);

            if (showings is null)
            {
                throw new HttpRequestException("Empty showings response.");
            }

            model.Showings = showings
                .Where(showing => showing.AvailableSeats > 0)
                .Select(showing => new SelectListItem
                {
                    Value = showing.Id.ToString(),
                    Text =
                        $"{showing.MovieTitle} - {showing.ScreenName} - " +
                        $"{showing.StartTime:dd MMM yyyy HH:mm} - " +
                        $"{showing.AvailableSeats} seats available"
                })
                .ToList();
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Showings could not be loaded. Check the Booking API and reload this page.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(
                string.Empty,
                "Loading showings timed out. Please reload this page.");
        }
    }

    private void AddApiError(HttpStatusCode status)
    {
        var message = status switch
        {
            HttpStatusCode.Conflict =>
                "Availability or pricing prevents this booking. Request a fresh quote, or choose another showing or quantity.",

            HttpStatusCode.NotFound =>
                "The showing no longer exists.",

            HttpStatusCode.ServiceUnavailable =>
                "The pricing service is unavailable. Please try again later.",

            HttpStatusCode.GatewayTimeout =>
                "The pricing service timed out. Please try again later.",

            HttpStatusCode.BadRequest =>
                "Check your showing, ticket type and quantity, then request a new quote.",

            _ =>
                "The request failed. If you were confirming, open Bookings and check before trying again."
        };

        ModelState.AddModelError(string.Empty, message);
    }
}