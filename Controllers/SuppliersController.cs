using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ILogger<SuppliersController> _logger;
        private readonly AlphaShopContext _context;
        private readonly IWebHostEnvironment _environment;
        public SuppliersController(ILogger<SuppliersController> logger, AlphaShopContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Supplier.ToListAsync();
            return View(suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSuppliers()
        {
            ViewBag.Products = await _context.Product
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ProductName
            }).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSuppliers(SupplierDto supplierDto)
        {
            if(supplierDto == null)
            {
                ModelState.AddModelError("", "Suppliers data is required.");
                return View(supplierDto);
            }
            if (!ModelState.IsValid)
            {
                return View(supplierDto);
            }
            string? Image = null;
            if (supplierDto.ImageSupplier != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(supplierDto.ImageSupplier.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageSupplier", "Only image files are allowed.");
                    return View(supplierDto);
                }
                string SuppliersImage = DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;
                string uploadsFloder = Path.Combine(_environment.WebRootPath, "images/Suppliers");
                if (!Directory.Exists(uploadsFloder))
                {
                    Directory.CreateDirectory(uploadsFloder);
                }
                string filePath = Path.Combine(uploadsFloder, SuppliersImage);
                using(var stream = System.IO.File.Create(filePath))
                {
                    await supplierDto.ImageSupplier.CopyToAsync(stream);
                }
                Image = SuppliersImage;
            }
            var suppliers = new Supplier
            {
                SupplierName = supplierDto.SupplierName,
                Email = supplierDto.Email,
                Phone = supplierDto.Phone,
                Address = supplierDto.Address,
                ImageSupplier = Image,
                Street = supplierDto.Street,
                City = supplierDto.City,
                Country = supplierDto.Country,
                ContactPerson = supplierDto.ContactPerson,
                ContractDate = supplierDto.ContractDate,
                Status = supplierDto.Status,
                CreatedAt = DateTime.Now
            };
            try
            {
                _context.Supplier.Add(suppliers);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating suppliers");
                ModelState.AddModelError("", "An error occurred while creating the suppliers. Please try again.");
                return View(supplierDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditSupplier(int id)
        {
            var supplier = await _context.Supplier.FindAsync(id);
            if(supplier == null)
            {
                return NotFound();
            }
            var supplierDto = new SupplierDto
            {
                SupplierName = supplier.SupplierName,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                Street = supplier.Street,
                City = supplier.City,
                Country = supplier.Country,
                ContactPerson = supplier.ContactPerson,
                ContractDate = supplier.ContractDate,
                Status = supplier.Status
            };
            ViewBag.Products = await _context.Product
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.ProductName
            }).ToListAsync();
            ViewData["Id"] = id;
            ViewData["ImageSupplier"] = supplier.ImageSupplier;
            return View(supplierDto);
        }

        public async Task<IActionResult> EditSupplier(int id, SupplierDto supplierDto)
        {
            if(supplierDto == null)
            {
                return NotFound();
            }
            var supplierEdit = await _context.Supplier.FindAsync(id);
            if(supplierDto == null)
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
                ViewData["Id"] = id;
                ViewData["ImageSupplier"] = supplierEdit?.ImageSupplier;
                return View(supplierDto);
            }
            try
            {
                if(supplierDto.ImageSupplier != null)
                {
                    await UpdateImageSuppliers(supplierEdit, supplierDto.ImageSupplier);
                }
                supplierEdit.SupplierName = supplierDto.SupplierName;
                supplierEdit.Email = supplierDto.Email;
                supplierEdit.Phone = supplierDto.Phone;
                supplierEdit.Address = supplierDto.Address;
                supplierEdit.Street = supplierDto.Street;
                supplierEdit.City = supplierDto.City;
                supplierEdit.Country = supplierDto.Country;
                supplierEdit.ContractDate = supplierDto.ContractDate;
                supplierEdit.Status = supplierDto.Status;
                supplierEdit.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Suppliers");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error editing suppliers {id}", id);
                ModelState.AddModelError("", "An error occurred while editing the suppliers. Please try again.");
                ViewBag.Products = await _context.Product
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    }).ToListAsync();
                ViewData["Id"] = id;
                ViewData["ImageSupplier"] = supplierEdit?.ImageSupplier;
                return View(supplierDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSuppliers(int id)
        {
            var suppliers = await _context.Supplier.FirstOrDefaultAsync(sup => sup.Id == id);
            if (suppliers == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(suppliers.ImageSupplier))
            {
                string imagePath = Path.Combine(_environment.WebRootPath, "images/Suppliers", suppliers.ImageSupplier);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _context.Supplier.Remove(suppliers);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Suppliers");
        }

        [HttpGet]
        public async Task<IActionResult> DetailsSuppliers(int id)
        {
            var suppliers = await _context.Supplier
                .Include(p => p.Products)
                    .ThenInclude(p => p.Category)
                .Include(p => p.PurchaseOrders)
                .FirstOrDefaultAsync(sup => sup.Id == id);
            if (suppliers == null)
            {
                return NotFound();
            }

            var totalAmount = suppliers.PurchaseOrders
                .Where(po => po.OrderStatus == Status.Draft)  // Only completed orders
                .Sum(po => po.TotalAmount);

            ViewBag.TotalAmount = totalAmount;
            return View(suppliers);
        }

        public async Task UpdateImageSuppliers(Supplier supplier, IFormFile formFileSuppliers)
        {
            if(!string.IsNullOrEmpty(supplier.ImageSupplier) && supplier.ImageSupplier != "Default.jpg")
            {
                var oldImageSuppliers = Path.Combine(_environment.WebRootPath, "images/Suppliers", supplier.ImageSupplier);
                if (System.IO.File.Exists(oldImageSuppliers))
                {
                    try
                    {
                        System.IO.File.Delete(oldImageSuppliers);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting old supplier image {imagePath}", oldImageSuppliers);
                    }
                    var newImage = $"{DateTime.Now:yyyyyMMddHHss}{Path.GetExtension(formFileSuppliers.FileName)}";
                    var saveImageSupplier = Path.Combine(_environment.WebRootPath, "images/Suppliers", newImage);
                    using (var fileStream = new FileStream(newImage, FileMode.Create))
                    {
                        await formFileSuppliers.CopyToAsync(fileStream);
                    }
                    supplier.ImageSupplier = newImage;
                }
            }
        }
    }
}
