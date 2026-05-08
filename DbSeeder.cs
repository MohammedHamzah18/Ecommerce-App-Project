using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myntra.Models;

namespace myntra.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Men" },
                new() { Name = "Women" },
                new() { Name = "Accessories" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var products = new List<Product>
            {
                new() { Name = "Slim Fit T-Shirt", Description = "Soft cotton T-shirt for everyday wear.", Price = 799, Stock = 50, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab" },
                new() { Name = "Casual Denim Jacket", Description = "Classic style denim jacket.", Price = 2499, Stock = 30, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1551537482-f2075a1d41f2" },
                new() { Name = "Floral Summer Dress", Description = "Lightweight and breathable summer dress.", Price = 1899, Stock = 40, CategoryId = categories[1].Id, ImageUrl = "https://images.unsplash.com/photo-1496747611176-843222e1e57c" },
                new() { Name = "Leather Handbag", Description = "Elegant handbag with spacious compartments.", Price = 3299, Stock = 25, CategoryId = categories[1].Id, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3" },
                new() { Name = "Sport Watch", Description = "Water-resistant watch with premium build.", Price = 1599, Stock = 35, CategoryId = categories[2].Id, ImageUrl = "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3" }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        const string adminEmail = "admin@myntra.com";
        const string adminPassword = "Admin@12345";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, adminPassword);
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
