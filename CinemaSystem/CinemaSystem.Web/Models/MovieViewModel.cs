using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Web.Models;

public class MovieViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a movie title.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Duration in minutes")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int DurationMinutes { get; set; }

    [Display(Name = "Premium movie")]
    public bool IsPremium { get; set; }
}