using System.Net.Http.Json;
using CinemaSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Web.Controllers;

public class MoviesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MoviesController(IHttpClientFactory httpClientFactory)
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

            var movies = await client
                .GetFromJsonAsync<List<MovieViewModel>>(
                    "api/movies",
                    cancellationToken);

            if (movies is null)
            {
                throw new HttpRequestException(
                    "The Booking API returned an empty response.");
            }

            return View(movies);
        }
        catch (HttpRequestException)
        {
            ViewBag.ApiError =
                "Movies could not be loaded. Check that the Booking API is running.";
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.ApiError =
                "Loading movies took too long. Please try again.";
        }

        return View(new List<MovieViewModel>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new MovieViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        MovieViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            var request = new
            {
                Title = model.Title.Trim(),
                model.DurationMinutes,
                model.IsPremium
            };

            using var response = await client.PostAsJsonAsync(
                "api/movies",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Movie added successfully.";

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(
                string.Empty,
                "The Booking API could not save the movie. Check the details and try again.");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to confirm whether the movie was saved. Check the Movies list before trying again.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(
                string.Empty,
                "The request timed out. Check the Movies list before trying again.");
        }

        return View(model);
    }
}