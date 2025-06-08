using Microsoft.EntityFrameworkCore;

namespace AlphaShop.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public required string attributeName { get; set; }
        public string? attributeValue { get; set; }

        [Precision(18, 2)]
        public decimal? price_modifier { get; set; }
        public required string sku_suffix { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string attributeName { get; set; }
        public string? attributeValue { get; set; }
        public decimal price_modifier { get; set; }
        public int ProductId { get; set; }
    }
}
