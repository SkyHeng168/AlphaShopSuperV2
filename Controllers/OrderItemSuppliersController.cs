using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class OrderItemSuppliersController : Controller
    {
        private readonly ILogger<OrderItemSuppliersController> _logger;
        private readonly AlphaShopContext _context;
        public OrderItemSuppliersController(ILogger<OrderItemSuppliersController> logger, AlphaShopContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CreateOrderItemSuppliers(int? purchaseOrderId)
        {
            ViewBag.Products = await _context.Product
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                }).ToListAsync();
            ViewBag.Orders = await _context.PurchaseOrder
                  .Select(pu => new SelectListItem
                  {
                      Value = pu.Id.ToString(),
                      Text = pu.Supplier!.ContactPerson
                  }).ToListAsync();
            return View(new OrderItemSuppliersDto { PurchaseOrderId = purchaseOrderId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrderItemSuppliers(OrderItemSuppliersDto orderItemSuppliersDto)
        {
            if (orderItemSuppliersDto == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _context.Product
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    }).ToListAsync();
                ViewBag.Orders = await _context.PurchaseOrder
                   .Select(pu => new SelectListItem
                   {
                       Value = pu.Id.ToString(),
                       Text = pu.Supplier!.ContactPerson
                   }).ToListAsync();
                return View(orderItemSuppliersDto);
            }

            try
            {
                var orderItemSuppliers = new OrderItemSuppliers
                {
                    Quantity = orderItemSuppliersDto.Quantity,
                    UnitPrice = orderItemSuppliersDto.UnitPrice,
                    StatusOrderItemSupplier = StatusOrderItemSupplier.Pending,
                    ProductId = orderItemSuppliersDto.ProductId,
                    PurchaseOrderId = orderItemSuppliersDto.PurchaseOrderId,
                };

                _context.OrderItemSuppliers.Add(orderItemSuppliers);
                await _context.SaveChangesAsync();

                // Now calculate the total amount for the purchase order
                var totalAmount = await _context.OrderItemSuppliers
                    .Where(ois => ois.PurchaseOrderId == orderItemSuppliersDto.PurchaseOrderId)
                    .SumAsync(ois => ois.Quantity * ois.UnitPrice);

                // Update the purchase order's total amount
                var purchaseOrder = await _context.PurchaseOrder
                    .FirstOrDefaultAsync(po => po.Id == orderItemSuppliersDto.PurchaseOrderId);
                var supplierId = purchaseOrder.SupplierId;
                if (purchaseOrder != null)
                {
                    purchaseOrder.TotalAmount = totalAmount;
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToAction("DetailsSuppliers", "Suppliers", new { id = supplierId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order item for supplier");
                ModelState.AddModelError("", "An error occurred while creating the order item for supplier. Please try again.");

                ViewBag.Products = await _context.Product
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    }).ToListAsync();
                ViewBag.Orders = await _context.PurchaseOrder
                  .Select(pu => new SelectListItem
                  {
                      Value = pu.Id.ToString(),
                      Text = pu.Supplier!.ContactPerson
                  }).ToListAsync();
                return View(orderItemSuppliersDto);
            }
        }
    }
}
