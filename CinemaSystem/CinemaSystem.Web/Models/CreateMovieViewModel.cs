using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CinemaSystem.Web.Models;

// Creation accepts only editable fields; the API generates the ID.
public class CreateMovieViewModel
{
    [Required(ErrorMessage = "Enter a movie title.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    // Nullable distinguishes a missing value from an entered number.
    [Required(ErrorMessage = "Enter the movie duration.")]
    [Display(Name = "Duration in minutes")]
    [Range(1, 1440,
        ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int? DurationMinutes { get; set; }

    // Require the form to submit true or false.
    [BindRequired]
    [Display(Name = "Premium movie")]
    public required bool IsPremium { get; set; }
}