using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myntra.Data;
using myntra.Models;
using myntra.ViewModels;

namespace myntra.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var items = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var vm = new CartViewModel
        {
            Items = items,
            GrandTotal = items.Sum(i => (i.Product?.Price ?? 0) * i.Quantity)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var userId = _userManager.GetUserId(User)!;
        var product = await _context.Products.FindAsync(productId);
        if (product is null || product.Stock <= 0)
        {
            return NotFound();
        }

        var existing = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        if (existing is null)
        {
            existing = new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = Math.Clamp(quantity, 1, product.Stock)
            };
            _context.CartItems.Add(existing);
        }
        else
        {
            existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, product.Stock);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var userId = _userManager.GetUserId(User)!;
        var cartItem = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (cartItem is null || cartItem.Product is null)
        {
            return NotFound();
        }

        cartItem.Quantity = Math.Clamp(quantity, 1, cartItem.Product.Stock);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
        if (item is not null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
