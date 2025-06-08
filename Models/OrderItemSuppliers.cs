namespace AlphaShop.Models
{
    public class OrderItemSuppliers
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int RecievedQuantity { get; set; }
        public StatusOrderItemSupplier StatusOrderItemSupplier { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
    public enum StatusOrderItemSupplier
    {
        Pending,
        Recieved,
        Canceled,
        Returned,
        Backorder
    }
    public class OrderItemSuppliersDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int RecievedQuantity { get; set; }
        public string? StatusOrderItemSupplier { get; set; }
        public int ProductId { get; set; }
        public int PurchaseOrderId { get; set; }
    }
}
