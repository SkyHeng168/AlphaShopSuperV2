using System.Diagnostics;
using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlphaShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly AlphaShopContext _context;
        public HomeController(AlphaShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);

            var model = new HomeViewModel
            {
                TotalCustomer = _context.Customer.Count(),
                TotalSupplier = _context.Supplier.Count(),
                Customers = _context.Customer
                            .Where(c => c.RegisterDate >= sevenDaysAgo)
                            .OrderByDescending(c => c.RegisterDate)
                            .Take(10)
                            .ToList(),
                StockTransactions = _context.StockTransactions
                            .Include(st => st.InventoryLogs)
                            .Where(st => st.IsActive)
                            .OrderBy(st => st.StockTransactionDate)
                            .ToList()
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
