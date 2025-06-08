namespace AlphaShop.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? SocialMediaUrl { get; set; }
        public string? Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class BrandDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? SocialMediaUrl { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
    }
}
