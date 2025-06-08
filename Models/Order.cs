namespace AlphaShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string? TrackingNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime orderDate { get; set; }
        public decimal totalAmount { get; set; }
        public string? trackingNumber { get; set; }
        public int customerId { get; set; }
        public string? Status { get; set; }
    }
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}
