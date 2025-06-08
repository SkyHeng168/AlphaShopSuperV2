using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly AlphaShopContext _context;
        private readonly ILogger<PurchaseOrderController> _logger;
        public PurchaseOrderController(ILogger<PurchaseOrderController> logger, AlphaShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var purchaseOrders = await _context.PurchaseOrder
                .Include(po => po.Supplier)
                .Include(po => po.OrderItemSuppliers)
                .ToListAsync();
            return View(purchaseOrders);
        }
        public async Task<IActionResult> DetailsOrder(int Id)
        {
            var purchaseOrder = await _context.PurchaseOrder
                .Include(po => po.Supplier)
                .Include(po => po.OrderItemSuppliers)
                .FirstOrDefaultAsync(po => po.Id == Id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            return View(purchaseOrder);
        }
        public async Task<IActionResult> CreatePurchasingOrder(int id)
        {
            var supplierId = await _context.Supplier.FirstOrDefaultAsync(s => s.Id == id);
            if (supplierId == null)
            {
                _logger.LogWarning("Supplier with ID {SupplierId} not found.", id);
                return NotFound();
            }
            var purchasingOrder = new PurchaseOrder
            {
                OrderNumber = await GenerateOrderNumber(),
                OrderDate = DateTime.Now,
                ExpectedDate = DateTime.Now, // Default expected date is 7 days from now
                TotalAmount = 0, // Default total amount is 0
                Note = string.Empty, // Default note is empty
                OrderStatus = Status.Draft, // Default status is Draft
                SupplierId = supplierId.Id,
            };
            _context.PurchaseOrder.Add(purchasingOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction("CreateOrderItemSuppliers", "OrderItemSuppliers", new { purchaseOrderId = purchasingOrder.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CancelPurchaseOrder(int id)
        {
            var purchaseOrder = await _context.PurchaseOrder
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }
            purchaseOrder.OrderStatus = Status.Cancelled;
            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var order = await _context.PurchaseOrder.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = Status.Received;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = order.SupplierId });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.PurchaseOrder.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = Status.Cancelled;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = order.SupplierId });
        }

        private async Task<String> GenerateOrderNumber()
        {
            string orderNumber;
            bool isUnique;
            int attempts = 0;
            const int maxAttempts = 10;
            do
            {
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomNumber = new Random().Next(100, 999);
                orderNumber = $"PO-{timestamp}-{randomNumber}";
                isUnique = !await _context.PurchaseOrder.AnyAsync(po => po.OrderNumber == orderNumber);
                attempts++;
                if (attempts >= maxAttempts)
                {
                    throw new Exception("Unable to generate a unique Order Number after multiple attempts.");
                }
            } while (!isUnique);
            return orderNumber;
        }
    }
}
