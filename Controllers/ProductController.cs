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
                    Profits = productDto.Profits,
                    DisccoutPrice = productDto.DisccoutPrice,
                    Weight = productDto.Weight,
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
                Profits = product.Profits,
                DisccoutPrice = product.DisccoutPrice,
                Weight = product.Weight,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                WareHouseId = product.WareHouseId,
            };

            await LoadDropdownData();
            ViewData["Id"] = id;    
            ViewData["ProductImage"] = product.ProductImage != null
                                        ? $"/images/products/{product.ProductImage}"
                                        : null;

            return View(productDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductDto productDto)
        {
            if (productDto == null || id != productDto.Id)
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
                await LoadDropdownData();
                ViewData["Id"] = id;
                ViewData["ProductImage"] = product.ProductImage;
                return View(productDto);
            }

            try
            {
                await UpdateProductPrices(product, productDto);

                if (productDto.ProductImage != null)
                {
                    await UpdateProductImageAsync(product, productDto.ProductImage);
                }

                MapProductDtoToEntity(productDto, product);
                product.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing product Id {ProductId}", id);
                ModelState.AddModelError("", "An error occurred while editing the product. Please try again.");

                await LoadDropdownData();
                ViewData["Id"] = id;
                ViewData["ProductImage"] = product.ProductImage;

                return View(productDto);
            }
        }

        // Helper methods
        private async Task LoadDropdownData()
        {
            ViewBag.Categories = await _context.Category
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Brands = await _context.Brand
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToListAsync();

            ViewBag.Suppliers = await _context.Supplier
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.SupplierName })
                .ToListAsync();

            ViewBag.Warehouses = await _context.WareHouse
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.WareHouseName })
                .ToListAsync();
        }

        private async Task UpdateProductPrices(Product product, ProductDto productDto)
        {
            if (productDto.Profits != product.Profits)
            {
                var lastSupplierPrice = await _context.OrderItemSuppliers
                    .Where(ois => ois.ProductId == product.Id)
                    .OrderByDescending(ois => ois.PurchaseOrder!.OrderDate)
                    .Select(ois => ois.UnitPrice)
                    .FirstOrDefaultAsync();

                if (lastSupplierPrice > 0)
                {
                    decimal profitPercent = productDto.Profits ?? 0;
                    decimal discountPercent = productDto.DisccoutPrice ?? 0;

                    // Selling Price = Cost Price × (1 + Profit % / 100)
                    decimal sellingPrice = lastSupplierPrice * (1 + (profitPercent / 100));

                    // Final Price = Selling Price × (1 − Discount % / 100)
                    decimal finalPrice = sellingPrice * (1 - (discountPercent / 100));

                    product.Price = sellingPrice;
                    product.DisccoutPrice = finalPrice;
                }
            }
        }


        private void MapProductDtoToEntity(ProductDto source, Product target)
        {
            target.ProductName = source.ProductName;
            target.ProductDescription = source.ProductDescription;
            target.Profits = source.Profits;
            target.DisccoutPrice = source.DisccoutPrice;
            target.Weight = source.Weight;
            target.IsFeatured = source.IsFeatured;
            target.IsActive = source.IsActive;
            target.CategoryId = source.CategoryId;
            target.BrandId = source.BrandId;
            target.SupplierId = source.SupplierId;
            target.WareHouseId = source.WareHouseId;
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
