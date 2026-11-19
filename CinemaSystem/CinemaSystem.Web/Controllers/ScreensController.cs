using System.Net.Http.Json;
using CinemaSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaSystem.Web.Controllers;

public class ScreensController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ScreensController(IHttpClientFactory httpClientFactory)
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

            var screens = await client
                .GetFromJsonAsync<List<ScreenViewModel>>(
                    "api/screens",
                    cancellationToken);

            if (screens is null)
            {
                throw new HttpRequestException(
                    "The Booking API returned an empty response.");
            }

            return View(screens);
        }
        catch (HttpRequestException)
        {
            ViewBag.ApiError =
                "Screens could not be loaded. Check that the Booking API is running.";
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.ApiError =
                "Loading screens took too long. Please try again.";
        }

        return View(new List<ScreenViewModel>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ScreenViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ScreenViewModel model,
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
                Name = model.Name.Trim(),
                model.Capacity
            };

            using var response = await client.PostAsJsonAsync(
                "api/screens",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Screen added successfully.";

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(
                string.Empty,
                "The Booking API could not save the screen. Check the details and try again.");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to confirm whether the screen was saved. Check the Screens list before trying again.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(
                string.Empty,
                "The request timed out. Check the Screens list before trying again.");
        }

        return View(model);
    }
}