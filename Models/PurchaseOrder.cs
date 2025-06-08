using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaShop.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public required string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public Status OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        public List<OrderItemSuppliers> OrderItemSuppliers { get; set; } = new List<OrderItemSuppliers>();
        public List<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
    public enum Status
    {
        Draft,         // Created but not submitted
        Submitted,     // Submitted for approval
        Approved,      // Approved by manager
        Rejected,      // Rejected by manager
        Ordered,       // Order sent to supplier
        Shipped,       // Supplier shipped the order
        Received,      // Goods received
        Cancelled      // Order was cancelled
    }
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public required string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public Status OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int SupplierId { get; set; }
    }
}