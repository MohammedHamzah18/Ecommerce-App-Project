using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myntra.Data;
using myntra.Models;
using myntra.ViewModels;

namespace myntra.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync();

        return View(orders);
    }

    public IActionResult Checkout()
    {
        return View(new CheckoutViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var userId = _userManager.GetUserId(User)!;
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
            return View(vm);
        }

        foreach (var item in cartItems)
        {
            if (item.Product is null || item.Product.Stock < item.Quantity)
            {
                ModelState.AddModelError(string.Empty, $"Insufficient stock for {item.Product?.Name ?? "an item"}.");
                return View(vm);
            }
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = vm.ShippingAddress,
            OrderedAt = DateTime.UtcNow,
            TotalAmount = cartItems.Sum(c => c.Quantity * (c.Product?.Price ?? 0))
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var orderItems = cartItems.Select(c => new OrderItem
        {
            OrderId = order.Id,
            ProductId = c.ProductId,
            Quantity = c.Quantity,
            UnitPrice = c.Product!.Price
        }).ToList();

        _context.OrderItems.AddRange(orderItems);

        foreach (var cartItem in cartItems)
        {
            cartItem.Product!.Stock -= cartItem.Quantity;
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Success), new { id = order.Id });
    }

    public async Task<IActionResult> Success(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }
}
