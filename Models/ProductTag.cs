namespace AlphaShop.Models
{
    public class ProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
