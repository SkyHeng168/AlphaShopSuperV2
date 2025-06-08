namespace AlphaShop.Models
{
    public class WareHouse
    {
        public int Id { get; set; }
        public string? WareHouseName { get; set; }
        public string? WareHouseCode { get; set; }
        public string? WareHouseLocation { get; set; }
        public string? WareHousePhone { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }
    public class WareHouseDto
    {
        public int Id { get; set; }
        public string? WareHouseName { get; set; }
        public string? WareHouseCode { get; set; }
        public string? WareHouseLocation { get; set; }
        public string? WareHousePhone { get; set; }
    }
}
