namespace AlphaShop.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }
        public required string TypeName { get; set; }
        public StockDirection Direction { get; set; }
        public DateTime StockTransactionDate { get; set; }
        public bool IsActive { get; set; }
        
        public List<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    }
    public enum StockDirection
    {
        In,
        Out
    }
    public class StockTransactionDto
    {
        public int Id { get; set; }
        public required string TypeName { get; set; }
        public StockDirection Direction { get; set; }
        public DateTime StockTransactionDate { get; set; }
        public bool IsActive { get; set; }
    }
}
