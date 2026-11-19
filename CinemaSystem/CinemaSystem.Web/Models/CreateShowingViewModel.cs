using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaSystem.Web.Models;

public class CreateShowingViewModel
{
    [Required(ErrorMessage = "Choose a movie.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Movie")]
    public int? MovieId { get; set; }

    [Required(ErrorMessage = "Choose a screen.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Screen")]
    public int? ScreenId { get; set; }

    [Required(ErrorMessage = "Choose a start date and time.")]
    [Display(Name = "Start time")]
    public DateTime? StartTime { get; set; }

    [Required(ErrorMessage = "Enter a base price.")]
    [Range(typeof(decimal), "0", "79228162514264337593543950335",
        ErrorMessage = "Base price cannot be negative.")]
    [Display(Name = "Base price (€)")]
    public decimal? BasePrice { get; set; }

    [BindNever]
    [ValidateNever]
    public List<SelectListItem> Movies { get; set; } = new();

    [BindNever]
    [ValidateNever]
    public List<SelectListItem> Screens { get; set; } = new();
}