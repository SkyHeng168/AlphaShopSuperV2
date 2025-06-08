using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly AlphaShopContext _context;
        private readonly ILogger<ProductController> _logger;
        public OrderItemController(AlphaShopContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var orderItem = await _context.OrderItem
                .Include(o => o.Product)
                .Include(o => o.PriceVariant)
                .Include(o => o.Order)
                .ToListAsync();
            return View(orderItem);
        }
        [HttpGet]
        public async Task<IActionResult> CreateOrderItem()
        {
            ViewBag.Products = await _context.Product.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ProductName
            }).ToListAsync();
            ViewBag.ProductVariants = await _context.ProductVaraint.Select(pv => new SelectListItem
            {
                Value = pv.Id.ToString(),
                Text = pv.attributeName
            }).ToListAsync();
            ViewBag.Orders = await _context.Order.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Customer!.FirstName + " " + o.Customer!.LastName
            }).ToListAsync();
            return View(new OrderItemDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderItem(OrderItemDto orderItemDto)
        {
            if (orderItemDto == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _context.Product.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                }).ToListAsync();
                ViewBag.ProductVariants = await _context.ProductVaraint.Select(pv => new SelectListItem
                {
                    Value = pv.Id.ToString(),
                    Text = pv.attributeName
                }).ToListAsync();
                ViewBag.Orders = await _context.Order.Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Customer!.FirstName + " " + o.Customer!.LastName
                }).ToListAsync();
                return View(orderItemDto);
            }

            try
            {
                var product = await _context.Product.FindAsync(orderItemDto.ProductId);
                var productVariant = await _context.ProductVaraint.FindAsync(orderItemDto.PriceVariantId);

                if (product == null || productVariant == null)
                {
                    ModelState.AddModelError("", "Product or variant not found");
                    return View(orderItemDto);
                }
                decimal productPrice = product.Price ?? 0;
                decimal productDiscount = product.DisccoutPrice ?? 0;
                decimal variantPriceModifier = productVariant.price_modifier ?? 0;
                decimal subtotal = (productPrice + variantPriceModifier - productDiscount) * orderItemDto.Quantity;
                var orderItem = new OrderItem
                {
                    Quantity = orderItemDto.Quantity,
                    Price = productPrice,
                    Discount = orderItemDto.Discount + productDiscount,
                    SubTotal = (productPrice - productDiscount) * orderItemDto.Quantity + (productVariant.price_modifier ?? 0),
                    ProductId = orderItemDto.ProductId,
                    PriceVariantId = orderItemDto.PriceVariantId,
                    OrderId = orderItemDto.OrderId,
                    CreatedAt = DateTime.Now,
                };
 
                _context.OrderItem.Add(orderItem);
                await _context.SaveChangesAsync();

                var order = await _context.Order
                     .Include(o => o.OrderItems)
                     .FirstOrDefaultAsync(o => o.Id == orderItemDto.OrderId);

                if (order != null)
                {
                    order.TotalAmount = order.OrderItems.Sum(item => item.SubTotal);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "OrderItem");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order item");
                ModelState.AddModelError("", "An error occurred while creating the order item. Please try again.");

                ViewBag.Products = await _context.Product.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                }).ToListAsync();
                ViewBag.ProductVariants = await _context.ProductVaraint.Select(pv => new SelectListItem
                {
                    Value = pv.Id.ToString(),
                    Text = pv.attributeName
                }).ToListAsync();
                ViewBag.Orders = await _context.Order.Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Customer!.FirstName + " " + o.Customer!.LastName
                }).ToListAsync();

                return View(orderItemDto);
            }
        }

    }
}
