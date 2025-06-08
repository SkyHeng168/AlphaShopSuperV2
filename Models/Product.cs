using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaShop.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DisccoutPrice { get; set; }

        public string? SKU { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Price { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public Brand? Brand { get; set; }

        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        public int? WareHouseId { get; set; }

        [ForeignKey("WareHouseId")]
        public WareHouse? WareHouse { get; set; }

        public List<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
        public List<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
        public List<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public List<OrderItemSuppliers> OrderItemSuppliers { get; set; } = new List<OrderItemSuppliers>();

    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public IFormFile? ProductImage { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DisccoutPrice { get; set; }
        public string? SKU { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Price { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? SupplierId { get; set; }
        public int? WareHouseId { get; set; }
    }
}
