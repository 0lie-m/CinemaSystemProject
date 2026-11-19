using System.Net;
using System.Net.Http.Json;
using CinemaSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaSystem.Web.Controllers;

public class ShowingsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ShowingsController(IHttpClientFactory httpClientFactory)
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

            var showings = await client
                .GetFromJsonAsync<List<ShowingViewModel>>(
                    "api/showings", cancellationToken);

            return View(showings
                ?? throw new HttpRequestException("Empty response."));
        }
        catch (HttpRequestException)
        {
            ViewBag.ApiError =
                "Showings could not be loaded. Please try again.";
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.ApiError =
                "Loading showings took too long. Please try again.";
        }

        return View(new List<ShowingViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = new CreateShowingViewModel();

        await LoadOptionsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateShowingViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.BasePrice.HasValue &&
            decimal.Round(model.BasePrice.Value, 2) != model.BasePrice.Value)
        {
            ModelState.AddModelError(
                nameof(model.BasePrice),
                "Use no more than two decimal places.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("BookingApi");

                var request = new
                {
                    MovieId = model.MovieId!.Value,
                    ScreenId = model.ScreenId!.Value,
                    StartTime = DateTime.SpecifyKind(
                        model.StartTime!.Value,
                        DateTimeKind.Unspecified),
                    BasePrice = model.BasePrice!.Value
                };

                using var response = await client.PostAsJsonAsync(
                    "api/showings",
                    request,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Showing scheduled successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var error = response.StatusCode switch
                {
                    HttpStatusCode.Conflict =>
                        "This screen already has a showing during that time. Choose another time or screen.",

                    HttpStatusCode.NotFound =>
                        "The selected movie or screen no longer exists.",

                    _ =>
                        "The showing could not be saved. Check the details and try again."
                };

                ModelState.AddModelError(string.Empty, error);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to confirm whether the showing was saved. Check the Showings list before trying again.");
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The request timed out. Check the Showings list before trying again.");
            }
        }

        await LoadOptionsAsync(model, cancellationToken);

        return View(model);
    }

    private async Task LoadOptionsAsync(
        CreateShowingViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            var movies = await client
                .GetFromJsonAsync<List<MovieViewModel>>(
                    "api/movies", cancellationToken);

            var screens = await client
                .GetFromJsonAsync<List<ScreenViewModel>>(
                    "api/screens", cancellationToken);

            if (movies is null || screens is null)
            {
                throw new HttpRequestException("Empty response.");
            }

            model.Movies = movies.Select(movie => new SelectListItem
            {
                Value = movie.Id.ToString(),
                Text = $"{movie.Title} ({movie.DurationMinutes} minutes)"
            }).ToList();

            model.Screens = screens.Select(screen => new SelectListItem
            {
                Value = screen.Id.ToString(),
                Text = $"{screen.Name} ({screen.Capacity} seats)"
            }).ToList();
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Movies and screens could not be loaded. Please reload this page.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(
                string.Empty,
                "Loading movies and screens timed out. Please reload this page.");
        }
    }
}