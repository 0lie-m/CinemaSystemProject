using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Web.Models;

public class ScreenViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a screen name.")]
    [StringLength(100)]
    [Display(Name = "Screen name")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue,
        ErrorMessage = "Capacity must be greater than zero.")]
    [Display(Name = "Seating capacity")]
    public int Capacity { get; set; }
}