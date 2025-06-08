using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlphaShop.Controllers
{
    public class BrandController : Controller
    {
        private readonly ILogger<BrandController> _logger;
        private readonly AlphaShopContext _context;
        private readonly IWebHostEnvironment _environment;
        public BrandController(ILogger<BrandController> logger, AlphaShopContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }
        
        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brand
                .ToListAsync();
            return View(brands);
        }

        [HttpGet]
        public IActionResult CreateBrand()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBrand(BrandDto brandDto)
        {
            if (brandDto == null)
            {
                ModelState.AddModelError("", "Invalid brand data.");
                return View(brandDto);
            }

            if (!ModelState.IsValid)
            {
                return View(brandDto);
            }

            string brandLogo = null;

            if (brandDto.LogoUrl != null && brandDto.LogoUrl.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".ico", ".svg" };
                var fileExtension = Path.GetExtension(brandDto.LogoUrl.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Invalid file type. Only .jpg, .jpeg, .png, .ico, and .svg are allowed.");
                    return View(brandDto);
                }

                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images/Branding");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await brandDto.LogoUrl.CopyToAsync(fileStream);
                }

                brandLogo = fileName;
            }

            var brand = new Brand
            {
                Name = brandDto.Name,
                Description = brandDto.Description,
                LogoUrl = brandLogo,
                WebsiteUrl = brandDto.WebsiteUrl,
                SocialMediaUrl = brandDto.SocialMediaUrl,
                Country = brandDto.Country,
                IsActive = brandDto.IsActive,
                CreatedAt = DateTime.UtcNow,
            };

            try
            {
                _context.Brand.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Brand");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Creating Brand: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the brand. Please try again.");
                return View(brandDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditBrand(int id)
        {
            var brand = await _context.Brand.FindAsync(id);
            if(brand == null)
            {
                return NotFound();
            }
            var brandDto = new BrandDto
            {
                Name = brand.Name,
                Description = brand.Description,
                WebsiteUrl = brand.WebsiteUrl,
                SocialMediaUrl = brand.SocialMediaUrl,
                Country = brand.Country,
                IsActive = brand.IsActive
            };
            ViewData["Id"] = id;
            ViewData["LogoUrl"] = brand.LogoUrl ?? "Default.jpg";
            return View(brandDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBrand(int id, BrandDto brandDto)
        {
            if(brandDto == null)
            {
                return NotFound();
            }
            var brand = await _context.Brand.FindAsync(id);
            if(brand == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Id"] = id;
                ViewData["LogoUrl"] = brand.LogoUrl ?? "Default.jpg";
                return View(brandDto);
            }
            try
            {
                if(brandDto.LogoUrl != null)
                {
                    await UpdateBranding(brand, brandDto.LogoUrl);
                }
                brand.Name = brandDto.Name;
                brand.Description = brandDto.Description;
                brand.WebsiteUrl = brandDto.WebsiteUrl;
                brand.SocialMediaUrl = brandDto.SocialMediaUrl;
                brand.Country = brandDto.Country;
                brand.IsActive = brandDto.IsActive;
                brand.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Brand");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erorr editing branding Id {brandingId}", id);
                ModelState.AddModelError("", "An erorr occurred while saving. Please try again.");
                ViewData["Id"] = id;
                ViewData["LogoUrl"] = brand.LogoUrl ?? "Default.jpg";
                return View(brandDto);
            }
        }
        private async Task UpdateBranding(Brand brand, IFormFile formFile)
        {
            if(!string.IsNullOrEmpty(brand.LogoUrl) && brand.LogoUrl != "Default.jpg")
            {
                var oldLogo = Path.Combine(_environment.WebRootPath, "images/Braning", brand.LogoUrl);
                if (System.IO.File.Exists(oldLogo))
                {
                    try
                    {
                        System.IO.File.Delete(oldLogo);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning(ex, "Could not delete old icon file {FileName}", brand.LogoUrl);
                    }
                }
            }
            var newLogo = $"{DateTime.Now:yyyyMMddHHss}{Path.GetExtension(formFile.FileName)}";
            var savePath = Path.Combine(_environment.WebRootPath, "images/Branding", newLogo);
            using(var stream = new FileStream(savePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
            brand.LogoUrl = newLogo;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var branding = await _context.Brand.FirstOrDefaultAsync(br => br.Id == id);
            if(branding == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(branding.LogoUrl))
            {
                string logo = Path.Combine(_environment.WebRootPath, "images/Branding", branding.LogoUrl);
                if (System.IO.File.Exists(logo))
                {
                    System.IO.File.Delete(logo);
                }
            }
            _context.Remove(branding);
            _context.SaveChanges();
            return RedirectToAction("Index", "Brand");
        }

        [HttpGet]
        public IActionResult DetailBrand(int id)
        {
            var BrandingDetail = _context.Brand.FirstOrDefault(b => b.Id == id);
            if(BrandingDetail == null)
            {
                return NotFound();
            }
            return View(BrandingDetail);
        }
    }
}
