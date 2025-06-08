namespace AlphaShop.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public required string SupplierName { get; set; }
        public string? ImageSupplier { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public required string ContactPerson { get; set; }
        public DateTime ContractDate { get; set; }
        public StatusSupplier Status { get; set; } = StatusSupplier.Active;

        public List<Product> Products { get; set; } = new List<Product>();
        public List<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class SupplierDto
    {
        public required string SupplierName { get; set; }
        public IFormFile? ImageSupplier { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public required string ContactPerson { get; set; }
        public DateTime ContractDate { get; set; }
        public StatusSupplier Status { get; set; } = StatusSupplier.Active;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum StatusSupplier
    {
        Active,
        Inactive
    }
}
