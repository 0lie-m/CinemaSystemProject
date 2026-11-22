using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaSystem.Web.Models;

public class BookingFormViewModel
{
    [Required(ErrorMessage = "Choose a showing.")]
    [Range(1, int.MaxValue)]
    [Display(Name = "Showing")]
    public int? ShowingId { get; set; }

    [Required]
    [RegularExpression("^(adult|student|child)$",
        ErrorMessage = "Choose adult, student or child.")]
    [Display(Name = "Ticket type")]
    public string TicketType { get; set; } = "adult";

    [Range(1, int.MaxValue,
        ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; } = 1;

    public decimal? ExpectedTotalPrice { get; set; }

    [BindNever]
    [ValidateNever]
    public List<SelectListItem> Showings { get; set; } = new();

    [BindNever]
    [ValidateNever]
    public BookingQuoteViewModel? Quote { get; set; }

    [BindNever]
    [ValidateNever]
    public string ShowingDescription { get; set; } = string.Empty;
}

public class BookingQuoteViewModel
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}