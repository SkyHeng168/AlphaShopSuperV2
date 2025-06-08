using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AlphaShop.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AlphaShopContext _context;
        private readonly ILogger<CategoryController> _logger;
        private readonly IWebHostEnvironment _environment;
        public CategoryController(AlphaShopContext context, ILogger<CategoryController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment​​ = environment;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Category
                .Include(c => c.SubCategories)
                .ToListAsync();
                
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            ViewBag.ParentCategories = new SelectList(_context.Category, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
        {
            if(categoryDto == null)
            {
                ModelState.AddModelError("", "Invalid category data.");
                return View(categoryDto);
            }
            if (!ModelState.IsValid)
            {
                return View(categoryDto);
            }
            string fileName = null;
            if (categoryDto.IconUrl != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".icon", ".svg"};
                var fileExtension = Path.GetExtension(categoryDto.IconUrl.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Invalid file type. Only .jpg, .jpeg, .png, .icon, and .svg are allowed.");
                    return View(categoryDto);
                }
                fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + fileExtension;
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images/icon");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await categoryDto.IconUrl.CopyToAsync(fileStream);
                }
            }
            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ParentCategoryId = categoryDto.ParentCategoryId,
                IconUrl = fileName,
                CreatedAt = DateTime.UtcNow,
            };
            try
            {
                _context.Category.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Category");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while creating the category. Please try again.");
                return View(categoryDto);
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> EditeCategory(int id)
        {
            var categoryEdit = await _context.Category.FindAsync(id);
            if(categoryEdit == null)
            {
                return NotFound();
            }
            var categoryDto = new CategoryDto
            {
                Name = categoryEdit.Name,
                Description = categoryEdit.Description,
                ParentCategoryId = categoryEdit.ParentCategoryId
            };
            ViewData["Id"] = id;
            ViewData["IconUrl"] = categoryEdit.IconUrl ?? "default.jpg";
            ViewBag.ParentCategories = new SelectList(_context.Category, "Id", "Name");

            return View(categoryDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CategoryDto categoryDto)
        {
            if (categoryDto == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["Id"] = id;
                ViewData["IconUrl"] = category.IconUrl ?? "Default.jpg";
                ViewData["ParentCategories"] = await GetParentCategoriesAsync(id); // Make sure to populate this
                return View(categoryDto);
            }

            try
            {
                // Handle icon update
                if (categoryDto.IconUrl != null)
                {
                    await UpdateCategoryIcon(category, categoryDto.IconUrl);
                }

                // Update other properties
                category.Name = categoryDto.Name;
                category.Description = categoryDto.Description;
                category.ParentCategoryId = categoryDto.ParentCategoryId;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing category ID {CategoryId}", id);
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                ViewData["Id"] = id;
                ViewData["IconUrl"] = category.IconUrl ?? "Default.jpg";
                ViewData["ParentCategories"] = await GetParentCategoriesAsync(id);
                return View(categoryDto);
            }
        }

        private async Task UpdateCategoryIcon(Category category, IFormFile newIconFile)
        {
            // Delete old icon if it exists
            if (!string.IsNullOrEmpty(category.IconUrl) && category.IconUrl != "Default.jpg")
            {
                var oldIconPath = Path.Combine(_environment.WebRootPath, "images/icon", category.IconUrl);
                if (System.IO.File.Exists(oldIconPath))
                {
                    try
                    {
                        System.IO.File.Delete(oldIconPath);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning(ex, "Could not delete old icon file {FileName}", category.IconUrl);
                    }
                }
            }

            // Save new icon
            var newFileName = $"{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(newIconFile.FileName)}";
            var savePath = Path.Combine(_environment.WebRootPath, "images/icon", newFileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await newIconFile.CopyToAsync(stream);
            }

            category.IconUrl = newFileName;
        }

        private async Task<List<SelectListItem>> GetParentCategoriesAsync(int? currentCategoryId)
        {
            return await _context.Category
                .Where(c => c.Id != currentCategoryId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Category.FirstOrDefaultAsync(ct => ct.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(category.IconUrl))
            {
                string icon = Path.Combine(_environment.WebRootPath, "images/icon", category.IconUrl);
                if (System.IO.File.Exists(icon))
                {
                    System.IO.File.Delete(icon);
                }
            }
            _context.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("Index", "Category");
        }

        // GET: CategoryCreate ApiController
        [HttpGet]
        [Route("api/category")]
        public async Task<IActionResult> GetAllProduct()
        {
            try
            {
                var categories = await _context.Category
                    .Include(c => c.SubCategories)
                    .ToListAsync();
                return Ok(new
                {
                    Success = categories.Any(),
                    Data = categories.Any() ? categories : null,
                    Message = categories.Any() ? "Categories found" : "No categories available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
