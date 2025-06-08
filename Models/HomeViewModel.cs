namespace AlphaShop.Models
{
    public class HomeViewModel
    {
        public int TotalCustomer { get; set; }
        public int TotalSupplier { get; set; }
        public List<Customer>? Customers { get; set; }
    }
}
