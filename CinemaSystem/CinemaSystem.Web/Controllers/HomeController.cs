using System.Net.Http.Json;
using CinemaSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IHttpClientFactory httpClientFactory,
        ILogger<HomeController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BookingApi");

            var showings = await client
                .GetFromJsonAsync<List<ShowingViewModel>>(
                    "api/showings",
                    cancellationToken);

            if (showings is null)
            {
                throw new HttpRequestException(
                    "The Booking API returned an empty response.");
            }

            return View(showings);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Unable to load showings from the Booking API.");

            ViewBag.ApiError =
                "We couldn't load the showings. Please check that the Booking API is running and try again.";
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.ApiError =
                "Loading showings took too long. Please try again.";
        }

        return View(new List<ShowingViewModel>());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return Problem("An unexpected error occurred.");
    }
}