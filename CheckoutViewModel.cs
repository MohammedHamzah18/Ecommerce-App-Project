using System.ComponentModel.DataAnnotations;

namespace myntra.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Shipping Address")]
    public string ShippingAddress { get; set; } = string.Empty;
}
