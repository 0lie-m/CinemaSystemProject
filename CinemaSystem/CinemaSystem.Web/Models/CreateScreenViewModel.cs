using System.ComponentModel.DataAnnotations;

namespace CinemaSystem.Web.Models;

// The screen ID belongs to saved records, not the creation form.
public class CreateScreenViewModel
{
    [Required(ErrorMessage = "Enter a screen name.")]
    [StringLength(100)]
    [Display(Name = "Screen name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the seating capacity.")]
    [Range(1, int.MaxValue,
        ErrorMessage = "Capacity must be greater than zero.")]
    [Display(Name = "Seating capacity")]
    public int? Capacity { get; set; }
}