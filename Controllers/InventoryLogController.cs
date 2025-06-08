using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class InventoryLogController : Controller
    {
        private readonly ILogger<StockTrancitionController> _logger;
        private readonly AlphaShopContext _context;
        public InventoryLogController(ILogger<StockTrancitionController> logger, AlphaShopContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var inventoryLogs = await _context.InventoryLog
                .Include(il => il.Product)
                .Include(il => il.StockTransacition)
                .ToListAsync();
            return View(inventoryLogs);
        }

        [HttpGet]
        public async Task<IActionResult> CreateInventory()
        {
            try
            {
                ViewBag.Products = await _context.Product
                    .Where(p => _context.PurchaseOrder
                        .Any(pod => pod.Id == p.Id &&
                                   _context.PurchaseOrder
                                       .Any(po => po.Id == pod.Id && po.SupplierId != null)))
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    })
                    .ToListAsync();

                ViewBag.StockTransactions = await _context.StockTransactions
                    .Select(st => new SelectListItem
                    {
                        Value = st.Id.ToString(),
                        Text = st.TypeName
                    })
                    .ToListAsync();
                ViewBag.PurchaseOrders = await _context.PurchaseOrder
                    .Where(po => po.OrderStatus == Status.Draft)
                    .Select(po => new SelectListItem
                    {
                        Value = po.Id.ToString(),
                        Text = po.OrderNumber
                    })
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create inventory view: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while loading the form. Please try again.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInventory(InventoryLogDto inventoryLogDto)
        {
            if (inventoryLogDto == null)
            {
                return NotFound();
            }

            // Custom validation for quantity against supplier availability
            if (inventoryLogDto.PurchaseOrderId > 0 && inventoryLogDto.ProductId > 0)
            {
                var orderItemSupplier = await _context.OrderItemSuppliers
                    .FirstOrDefaultAsync(o => o.PurchaseOrderId == inventoryLogDto.PurchaseOrderId &&
                                             o.ProductId == inventoryLogDto.ProductId);
                if (orderItemSupplier != null)
                {
                    // Fix: Change Qunatity to Quantity
                    if (inventoryLogDto.Qunatity > orderItemSupplier.Quantity)
                    {
                        ModelState.AddModelError("Quantity",
                            $"Quantity cannot exceed supplier availability. Maximum available: {orderItemSupplier.Quantity}");
                    }
                    if (orderItemSupplier.StatusOrderItemSupplier == StatusOrderItemSupplier.Recieved)
                    {
                        ModelState.AddModelError("", "This item has already been received.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Selected product is not found in the selected purchase order.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadViewBagData();
                return View(inventoryLogDto);
            }

            try
            {
                var inventory = new InventoryLog
                {
                    Qunatity = inventoryLogDto.Qunatity,
                    Note = inventoryLogDto.Note,
                    Log_Date = inventoryLogDto.Log_Date,
                    StockTransacitionId = inventoryLogDto.StockTransacitionId,
                    PurchaseOrderId = inventoryLogDto.PurchaseOrderId,
                    ProductId = inventoryLogDto.ProductId
                };

                _context.InventoryLog.Add(inventory);

                var orderItemSupplier = await _context.OrderItemSuppliers
                    .FirstOrDefaultAsync(o => o.PurchaseOrderId == inventoryLogDto.PurchaseOrderId &&
                                             o.ProductId == inventoryLogDto.ProductId);

                if (orderItemSupplier != null)
                {
                    orderItemSupplier.StatusOrderItemSupplier = StatusOrderItemSupplier.Recieved;
                }

                var purchaseOrder = await _context.PurchaseOrder
                    .FirstOrDefaultAsync(po => po.Id == inventoryLogDto.PurchaseOrderId);

                if (purchaseOrder != null)
                {
                    purchaseOrder.OrderStatus = Status.Received;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "InventoryLog");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Creating Inventory Log!");
                ModelState.AddModelError("", "An error occurred while creating the inventory");

                await LoadViewBagData();
                return View(inventoryLogDto);
            }
        }
        private async Task LoadViewBagData()
        {
            ViewBag.Products = await _context.Product
                .Where(p => _context.OrderItemSuppliers
                    .Any(ois => ois.ProductId == p.Id &&
                               _context.PurchaseOrder
                                   .Any(po => po.Id == ois.PurchaseOrderId &&
                                             po.SupplierId != null &&
                                             po.OrderStatus == Status.Draft)))
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();

            ViewBag.StockTransactions = await _context.StockTransactions
                .Select(st => new SelectListItem
                {
                    Value = st.Id.ToString(),
                    Text = st.TypeName
                })
                .ToListAsync();

            ViewBag.PurchaseOrders = await _context.PurchaseOrder
                .Where(po => po.OrderStatus == Status.Draft)
                .Select(po => new SelectListItem
                {
                    Value = po.Id.ToString(),
                    Text = po.OrderNumber
                })
                .ToListAsync();
        }

        public async Task<IActionResult> EditingInventory(int id)
        {
            var inventoryLog = await _context.InventoryLog.FindAsync(id);
            if (inventoryLog == null)
            {
                return NotFound();
            }
            var inventoryLogDto = new InventoryLogDto
            {
                Qunatity = inventoryLog.Qunatity,
                Note = inventoryLog.Note,
                Log_Date = inventoryLog.Log_Date,
                StockTransacitionId = inventoryLog.StockTransacitionId,
                ProductId = inventoryLog.ProductId
            };
            ViewBag.Products = await _context.Product
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();
            ViewBag.StockTransactions = await _context.StockTransactions
                .Select(st => new SelectListItem
                {
                    Value = st.Id.ToString(),
                    Text = st.TypeName
                })
                .ToListAsync();
            ViewData["Id"] = id;
            return View(inventoryLog);
        }
        [HttpPost]
        public async Task<IActionResult> EditingInventory(int id, InventoryLogDto inventoryLogDto)
        {
            if (inventoryLogDto == null)
                return NotFound();

            var inventory = await _context.InventoryLog.FindAsync(id);
            if (inventory == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                ViewData["Id"] = id;
                return View(inventoryLogDto);
            }

            try
            {
                inventory.Qunatity = inventoryLogDto.Qunatity;
                inventory.Note = inventoryLogDto.Note;
                inventory.Log_Date = inventoryLogDto.Log_Date;
                inventory.StockTransacitionId = inventoryLogDto.StockTransacitionId;
                inventory.ProductId = inventoryLogDto.ProductId;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "InventoryLog");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating Inventory Log: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while updating the inventory log. Please try again.");
                await LoadDropdowns();
                return View(inventoryLogDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.InventoryLog.FirstOrDefaultAsync(i => i.Id == id);
            if(inventory == null)
            {
                return NotFound();
            }
            _context.Remove(inventory);
            _context.SaveChanges();
            return RedirectToAction("Index", "InventoryLog");
        }

        public async Task<IActionResult> DetailsInventory(int id)
        {
            var inventoryLog = await _context.InventoryLog
                .Include(il => il.Product)
                .Include(il => il.StockTransacition)
                .FirstOrDefaultAsync(il => il.Id == id);
            if (inventoryLog == null)
            {
                return NotFound();
            }
            return View(inventoryLog);
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Products = await _context.Product
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();
            ViewBag.StockTransactions = await _context.StockTransactions
                .Select(st => new SelectListItem
                {
                    Value = st.Id.ToString(),
                    Text = st.TypeName
                })
                .ToListAsync();
        }
    }
}
