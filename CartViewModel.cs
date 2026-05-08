using myntra.Models;

namespace myntra.ViewModels;

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal GrandTotal { get; set; }
}
