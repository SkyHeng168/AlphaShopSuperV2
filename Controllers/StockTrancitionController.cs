using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlphaShop.Controllers
{
    public class StockTrancitionController : Controller
    {
        private readonly ILogger<StockTrancitionController> _logger;
        private readonly AlphaShopContext _context;
        public StockTrancitionController(ILogger<StockTrancitionController> logger, AlphaShopContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var stockTrancition = await _context.StockTransactions.ToListAsync();
            return View(stockTrancition);
        }

        [HttpGet]
        public IActionResult CreateStcokTransaction()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStcokTransaction(StockTransactionDto stockTransactionDto)
        {
            if(stockTransactionDto == null)
            {
                ModelState.AddModelError("", "Invalid stock transaction data.");
                return View(stockTransactionDto);
            }
            if (!ModelState.IsValid)
            {
                return View(stockTransactionDto);
            }
            var stockTransaction = new StockTransaction
            {
                TypeName = stockTransactionDto.TypeName,
                Direction = stockTransactionDto.Direction,
                StockTransactionDate = stockTransactionDto.StockTransactionDate,
                IsActive = stockTransactionDto.IsActive
            };
            try
            {
                await _context.StockTransactions.AddAsync(stockTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "StockTrancition");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Creating Stock Transaction: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the stock transaction. Please try again.");
                return View(stockTransactionDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditsStockTransacition(int id)
        {
            var stockTransacition = await _context.StockTransactions.FindAsync(id);
            if(stockTransacition == null)
            {
                return NotFound();
            }
            var stockTransacitionDto = new StockTransactionDto
            { 
                TypeName = stockTransacition.TypeName,
                Direction = stockTransacition.Direction,
                StockTransactionDate = stockTransacition.StockTransactionDate,
                IsActive = stockTransacition.IsActive
            };
            ViewData["Id"] = id;
            return View(stockTransacitionDto);
        }

        public async Task<IActionResult> EditsStockTransacition(int id, StockTransactionDto stockTransactionDto)
        {
            if(stockTransactionDto == null)
            {
                return NotFound();
            }
            var stockTransacition = await _context.StockTransactions.FindAsync(id);
            if(stockTransacition == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Id"] = id;
                return View(stockTransactionDto);
            }
            try
            {
                stockTransacition.TypeName = stockTransactionDto.TypeName;
                stockTransacition.Direction = stockTransactionDto.Direction;
                stockTransacition.StockTransactionDate = stockTransacition.StockTransactionDate;
                stockTransacition.IsActive = stockTransactionDto.IsActive;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "StockTrancition");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Erorr editing branding Id {Id}", id);
                ModelState.AddModelError("", "An erorr occurred while saving. Please try again.");
                ViewData["Id"] = id;
                return View(stockTransactionDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStockTransacition(int id)
        {
            var stockTransaction = await _context.StockTransactions.FirstOrDefaultAsync(tr => tr.Id == id);
            if (stockTransaction == null)
            {
                ModelState.AddModelError("", "Stock transaction not found.");
                return NotFound();
            }
            _context.StockTransactions.Remove(stockTransaction);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "StockTrancition");
        }
    }
}
