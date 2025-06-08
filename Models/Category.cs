namespace AlphaShop.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; } = new List<Category>();
        public string? IconUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<Product> Products { get; set; }= new List<Product>();
    }
    public class CategoryDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public IFormFile? IconUrl { get; set; }
    }
}
