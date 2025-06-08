using System.ComponentModel.DataAnnotations;

namespace AlphaShop.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Slug { get; set; }
        public List<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }

    public class TagDto
    {
        public required string Name { get; set; }
        public string? Slug { get; set; }
        public List<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}
