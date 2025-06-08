using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaShop.Models
{
    public class InventoryLog
    {
        public int Id { get; set; }
        public int Qunatity { get; set; }
        public string? Note { get; set; }
        public DateTime Log_Date { get; set; }
        public int StockTransacitionId { get; set; }
        public StockTransaction? StockTransacition { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public int PurchaseOrderId { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder? PurchaseOrder { get; set; }
    }
    public class InventoryLogDto
    {
        public int Qunatity { get; set; }
        public string? Note { get; set; }
        public DateTime Log_Date { get; set; }
        public int StockTransacitionId { get; set; }
        public int ProductId { get; set; }
        public int PurchaseOrderId { get; set; }
    }
}
