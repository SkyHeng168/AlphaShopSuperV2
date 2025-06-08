using System.Diagnostics;
using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;

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
            DateTime saveDaysAgo = DateTime.Now.AddDays(-7);
            var model = new HomeViewModel
            {
                TotalCustomer = _context.Customer.Count(),
                TotalSupplier = _context.Supplier.Count(),
                Customers = _context.Customer
                            .Where(c => c.RegisterDate >= saveDaysAgo)
                            .OrderByDescending(c => c.RegisterDate)
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
