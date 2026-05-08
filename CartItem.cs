using System.ComponentModel.DataAnnotations;

namespace myntra.Models;

public class CartItem
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, 20)]
    public int Quantity { get; set; } = 1;
}
