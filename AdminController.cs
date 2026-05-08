using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myntra.Data;
using myntra.Models;

namespace myntra.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalOrders = await _context.Orders.CountAsync();
        ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        return View();
    }

    public async Task<IActionResult> Products()
    {
        var products = await _context.Products.Include(p => p.Category).OrderByDescending(p => p.Id).ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> CreateProduct()
    {
        await SetCategoryDropdownAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        if (!ModelState.IsValid)
        {
            await SetCategoryDropdownAsync(product.CategoryId);
            return View(product);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await SetCategoryDropdownAsync(product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Product product)
    {
        if (!ModelState.IsValid)
        {
            await SetCategoryDropdownAsync(product.CategoryId);
            return View(product);
        }

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Categories()
    {
        return View(await _context.Categories.OrderBy(c => c.Name).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _context.Categories.Add(new Category { Name = name.Trim() });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        if (category is not null && !category.Products.Any())
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Categories));
    }

    private async Task SetCategoryDropdownAsync(int selectedId = 0)
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Categories = new SelectList(categories, nameof(Category.Id), nameof(Category.Name), selectedId);
    }
}
