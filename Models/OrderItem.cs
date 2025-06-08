using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaShop.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }          // Price per unit

        public decimal Discount { get; set; }       // Discount amount for this item

        public decimal SubTotal { get; set; }       // (Price * Quantity) - Discount

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public StatusOrderItem Status { get; set; } = StatusOrderItem.Pending;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int PriceVariantId { get; set; }
        public ProductVariant? PriceVariant { get; set; }

        public int? OrderId { get; set; }
        public Order? Order { get; set; }
    }
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public int ProductId { get; set; }
        public int PriceVariantId { get; set; }
        public int OrderId { get; set; }
    }

    public enum StatusOrderItem
    {
        Pending,
        Shipped,
        Completed,
        Cancelled
    }
}
