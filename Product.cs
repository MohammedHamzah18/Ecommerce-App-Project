using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myntra.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(1, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    [Url]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
