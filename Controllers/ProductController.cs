using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AlphaShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly AlphaShopContext _context;
        private readonly IWebHostEnvironment _environment;
        public ProductController(ILogger<ProductController> logger, AlphaShopContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Category
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Brands = await _context.Brand
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToListAsync();

            var products = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Category.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();

            ViewBag.Brands = await _context.Brand.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToListAsync();

            ViewBag.Suppliers = await _context.Supplier.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.SupplierName
            }).ToListAsync();
            ViewBag.Warehouses = await _context.WareHouse.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.WareHouseName
            }).ToListAsync();
            return View(new ProductDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            if (productDto == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Category.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

                ViewBag.Brands = await _context.Brand.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToListAsync();
                ViewBag.Suppliers = await _context.Supplier.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SupplierName
                }).ToListAsync();
                ViewBag.Warehouses = await _context.WareHouse.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.WareHouseName
                }).ToListAsync();
                return View(productDto);
            }

            try
            {
                var product = new Product
                {
                    ProductName = productDto.ProductName,
                    ProductDescription = productDto.ProductDescription,
                    BasePrice = productDto.BasePrice,
                    DisccoutPrice = productDto.DisccoutPrice,
                    Weight = productDto.Weight,
                    Price = productDto.BasePrice - (productDto.DisccoutPrice ?? 0),
                    IsFeatured = productDto.IsFeatured,
                    IsActive = productDto.IsActive,
                    SKU = await GenerateUniqueSkuAsync(),
                    CategoryId = productDto.CategoryId,
                    BrandId = productDto.BrandId,
                    SupplierId = productDto.SupplierId,
                    WareHouseId = productDto.WareHouseId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ProductImage = "Default.jpg"
                };

                if (productDto.ProductImage != null)
                {
                    var newImageFileName = $"{DateTime.Now:yyyyyMMddHHss}{Path.GetExtension(productDto.ProductImage.FileName)}";
                    var saveImage = Path.Combine(_environment.WebRootPath, "images/Products", newImageFileName);

                    using (var fileStream = new FileStream(saveImage, FileMode.Create))
                    {
                        await productDto.ProductImage.CopyToAsync(fileStream);
                    }

                    product.ProductImage = newImageFileName;
                }

                _context.Product.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "An error occurred while creating the product. Please try again.");

                ViewBag.Categories = await _context.Category.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();

                ViewBag.Brands = await _context.Brand.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToListAsync();
                ViewBag.Suppliers = await _context.Supplier.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SupplierName
                }).ToListAsync();
                ViewBag.Warehouses = await _context.WareHouse.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.WareHouseName
                }).ToListAsync();
                return View(productDto);
            }
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Product.FirstOrDefaultAsync(pr => pr.Id == id);
            if(product == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(product.ProductImage))
            {
                string Img = Path.Combine(_environment.WebRootPath, "images/Products", product.ProductImage);
                if (System.IO.File.Exists(Img))
                {
                    System.IO.File.Delete(Img);
                }
            }
            _context.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index", "Product");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var productDto = new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                BasePrice = product.BasePrice,
                DisccoutPrice = product.DisccoutPrice,
                Weight = product.Weight,
                Price = product.Price,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                WareHouseId = product.WareHouseId,
            };
            ViewBag.Categories = await _context.Category.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();
            ViewBag.Brands = await _context.Brand.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToListAsync();
            ViewBag.Suppliers = await _context.Supplier.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.SupplierName
            }).ToListAsync();
            ViewBag.Warehouses = await _context.WareHouse.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.WareHouseName
            }).ToListAsync();
            ViewData["Id"] = id;
            ViewData["ProductImage"] = product.ProductImage;
            return View(productDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id,ProductDto productDto)
        {
            if (productDto == null)
            {
                return NotFound();
            }
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Category.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
                ViewBag.Brands = await _context.Brand.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToListAsync();
                ViewBag.Suppliers = await _context.Supplier.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SupplierName
                }).ToListAsync();
                ViewBag.Warehouses = await _context.WareHouse.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.WareHouseName
                }).ToListAsync();
                ViewData["Id"] = id;
                ViewData["ProductImage"] = product.ProductImage;
                return View(productDto);
            }
            try
            {
                if(productDto.ProductImage != null)
                {
                    await UpdateProductImageAsync(product, productDto.ProductImage);
                }
                product.ProductName = productDto.ProductName;
                product.ProductDescription = productDto.ProductDescription;
                product.BasePrice = productDto.BasePrice;
                product.DisccoutPrice = productDto.DisccoutPrice;
                product.Weight = productDto.Weight;
                product.Price = productDto.BasePrice - (productDto.DisccoutPrice ?? 0);
                product.IsFeatured = productDto.IsFeatured;
                product.IsActive = productDto.IsActive;
                product.CategoryId = productDto.CategoryId;
                product.BrandId = productDto.BrandId;
                product.SupplierId = productDto.SupplierId;
                product.WareHouseId = productDto.WareHouseId;
                product.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Product");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error editing product Id {productId}", id);
                ModelState.AddModelError("", "An error occurred while editing the product. Please try again.");
                ViewBag.Categories = await _context.Category.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
                ViewBag.Brands = await _context.Brand.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToListAsync();
                ViewBag.Suppliers = await _context.Supplier.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SupplierName
                }).ToListAsync();
                ViewBag.Warehouses = await _context.WareHouse.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.WareHouseName
                }).ToListAsync();
                ViewData["Id"] = id;
                ViewData["ProductImage"] = product.ProductImage;
                return View(productDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewProduct(int id)
        {
            var productDetail = await _context.Product
                .Include(p => p.WareHouse) // Include the Warehouse data
                .Include(p => p.Category)  // Include Category if needed
                .Include(p => p.Brand)     // Include Brand if needed
                .Include(p => p.Supplier)  // Include Supplier if needed
                .Include(p => p.InventoryLogs)
                .FirstOrDefaultAsync(pro => pro.Id == id);

            if (productDetail == null)
            {
                return NotFound();
            }
            return View(productDetail);
        }

        private async Task<String> GenerateUniqueSkuAsync()
        {
            string sku;
            bool isUnique;
            int attempts = 0;
            const int maxAttempts = 5;
            do
            {
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomNumber = new Random().Next(100, 999);
                sku = $"SKU-{timestamp}-{randomNumber}";
                isUnique = !await _context.Product.AnyAsync(p => p.SKU == sku);
                attempts++;
                if (attempts >= maxAttempts)
                {
                    throw new Exception("Unable to generate a unique SKU after multiple attempts.");
                }
            }
            while (!isUnique);
            return sku;
        }

        private async Task UpdateProductImageAsync(Product product, IFormFile productImage)
        {
            if(!string.IsNullOrEmpty(product.ProductImage) && product.ProductImage != "Default.jpg")
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, "images/Products", product.ProductImage);
                if (System.IO.File.Exists(oldImagePath))
                {
                    try
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting old product image {imagePath}", oldImagePath);
                    }
                }
                var newImageFileName = $"{DateTime.Now:yyyyyMMddHHss}{Path.GetExtension(productImage.FileName)}";
                var saveImage = Path.Combine(_environment.WebRootPath, "images/Products", newImageFileName);
                using (var fileStream = new FileStream(saveImage, FileMode.Create))
                {
                    await productImage.CopyToAsync(fileStream);
                }
                product.ProductImage = newImageFileName;
            }
        }
    }
}
