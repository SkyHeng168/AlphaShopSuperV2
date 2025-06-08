using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class WareHouseController : Controller
    {
        private readonly ILogger<WareHouseController> _logger;
        private readonly AlphaShopContext _context;
        public WareHouseController(ILogger<WareHouseController> logger, AlphaShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var wareHouses = await _context.WareHouse
                .Include(w => w.Products)
                .ToListAsync();

            return View(wareHouses);
        }
        [HttpGet]
        public IActionResult CreateWareHouse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateWareHouse(WareHouseDto wareHouseDto)
        {
            if(wareHouseDto == null)
            {
                ModelState.AddModelError("", "Invalid Warehouse data");
                return View(wareHouseDto);
            }
            if (!ModelState.IsValid)
            {
                return View(wareHouseDto);
            }
            var wareHouse = new WareHouse
            {
                WareHouseName = wareHouseDto.WareHouseName,
                WareHouseCode = wareHouseDto.WareHouseCode,
                WareHouseLocation = wareHouseDto.WareHouseLocation,
                WareHousePhone = wareHouseDto.WareHousePhone
            };
            try
            {
                _context.WareHouse.Add(wareHouse);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "WareHouse");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erorr Creating Warehouse: {Message}", ex.Message);
                ModelState.AddModelError("", "An Erorr occurred while creating the Warehouse");
                return View(wareHouseDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditWareHouse(int id)
        {
            var editWareHouse = await _context.WareHouse.FindAsync(id);
            if (editWareHouse == null)
            {
                return NotFound();
            }
            var wareHouseDto = new WareHouseDto
            {
                WareHouseName = editWareHouse.WareHouseName,
                WareHouseCode = editWareHouse.WareHouseCode,
                WareHouseLocation = editWareHouse.WareHouseLocation,
                WareHousePhone = editWareHouse.WareHousePhone
            };
            ViewData["Id"] = id;
            return View(wareHouseDto);
        }
        public async Task<IActionResult> EditWareHouse(int id, WareHouseDto wareHouseDto)
        {
            if(wareHouseDto == null)
            {
                return NotFound();
            }
            var editWareHosue = await _context.WareHouse.FindAsync(id);
            if(editWareHosue == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Id"] = id;
                return View(wareHouseDto);
            }
            try
            {
                editWareHosue.WareHouseName = wareHouseDto.WareHouseName;
                editWareHosue.WareHouseLocation = wareHouseDto.WareHouseLocation;
                editWareHosue.WareHousePhone = wareHouseDto.WareHousePhone;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "WareHouse");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Editing error {Id}", id);
                ModelState.AddModelError("", "An error occurred while saving. Please trying again!");
                ViewData["Id"] = id;
                return View(wareHouseDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWareHouse(int id)
        {
            var waeHouse = await _context.WareHouse
                .Include(w => w.Products)
                .FirstOrDefaultAsync(wh => wh.Id == id);
            if(waeHouse == null)
            {
                return NotFound();
            }
            _context.Remove(waeHouse);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "WareHouse");
        }

        public async Task<IActionResult> DetailWareHouse(int id)
        {
            var detailWarehouse = await _context.WareHouse
                .Include(w => w.Products)
                .FirstOrDefaultAsync(w => w.Id == id);
            if(detailWarehouse == null)
            {
                return NotFound();
            }
            return View(detailWarehouse);
        }
    }
}
