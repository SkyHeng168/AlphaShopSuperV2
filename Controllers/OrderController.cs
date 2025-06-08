using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly AlphaShopContext _context;
        public OrderController(AlphaShopContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var order = await _context.Order
                .Include(o => o.Customer)
                .OrderBy(c => c.CustomerId)
                .ToListAsync();
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder()
        {
            try
            {
                ViewBag.CustomerId = await _context.Customer
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FirstName + " " + c.LastName
                    })
                    .ToListAsync();
                return View();
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while loading form!");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(OrderDto orderDto)
        {
            if (orderDto == null)
            {
                ModelState.AddModelError("", "Order cannot be null.");
                return View(orderDto);
            }
            if (!ModelState.IsValid)
            {
                return View(orderDto);
            }
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = 0, // Or calculate based on items if needed
                TrackingNumber = "TRK-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // will generate a random tracking number or it's will from API when it's in real time
                Customer = null, // Set to null if not needed
                CreatedAt = DateTime.Now,
                CustomerId = orderDto.customerId
            };
            try
            {
                _context.Order.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Order");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while saving the order: {ex.Message}");
                ViewBag.CustomerId = await _context.Customer
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FirstName + " " + c.LastName
                })
                .ToListAsync();
                return View(orderDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateOrder(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if(order == null)
            {
                return NotFound();
            }
            var orderDto = new Order
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                TrackingNumber = order.TrackingNumber,
                CustomerId = order.CustomerId
            };
            ViewBag.CustomerId = await _context.Customer
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FirstName + " " + c.LastName
                    })
                    .ToListAsync();
            return View(orderDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(int id, OrderDto orderDto)
        {
            if(orderDto == null)
            {
                return NotFound();
            }
            var order = await _context.Order.FindAsync(id);
            if(order == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewBag.CustomerId = await _context.Customer
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FirstName + " " + c.LastName
                    })
                    .ToListAsync();
                return View(orderDto);
            }
            try
            {
                order.OrderDate = DateTime.UtcNow;
                order.TotalAmount = orderDto.totalAmount;
                order.TrackingNumber = orderDto.trackingNumber;
                order.CustomerId = orderDto.customerId;
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Order");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Error editing order Id");
                ViewBag.CustomerId = await _context.Customer
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FirstName + " " + c.LastName
                    })
                    .ToListAsync();
                return View(orderDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var orderDelete = await _context.Order.FirstOrDefaultAsync(od => od.Id == id);
            if(orderDelete == null)
            {
                return NotFound();
            }
            _context.Remove(orderDelete);
            _context.SaveChanges();
            return RedirectToAction("Index", "Order");
        }

        [HttpGet]
        public IActionResult DetailOrder(int id)
        {
            var order = _context.Order
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }
}
