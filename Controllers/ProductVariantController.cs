using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class ProductVariantController : Controller
    {
        private readonly ILogger<ProductVariantController> _logger;
        private readonly AlphaShopContext _context;

        public ProductVariantController(ILogger<ProductVariantController> logger, AlphaShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productVaraints = await _context.ProductVaraint
                .Include(p => p.Product)
                .ToListAsync();
            return View(productVaraints);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProductVariant()
        {
            await PopulateProductsViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProductVariant(ProductVariantDto productVariantDto)
        {
            await PopulateProductsViewBag();

            if (productVariantDto == null)
            {
                ModelState.AddModelError("", "Invalid product variant data.");
                return View(productVariantDto);
            }

            if (productVariantDto.ProductId <= 0)
            {
                ModelState.AddModelError("ProductId", "Please select a valid product");
            }

            if (!ModelState.IsValid)
            {
                return View(productVariantDto);
            }

            var skuSuffix = await GenerateUniqueSkuAsync();
            if (string.IsNullOrEmpty(skuSuffix))
            {
                ModelState.AddModelError("", "Could not generate SKU suffix");
                return View(productVariantDto);
            }

            var productVariant = new ProductVariant
            {
                attributeName = productVariantDto.attributeName,
                attributeValue = productVariantDto.attributeValue,
                price_modifier = productVariantDto.price_modifier,
                ProductId = productVariantDto.ProductId,
                sku_suffix = skuSuffix,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            try
            {
                _context.ProductVaraint.Add(productVariant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product variant created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving product variant");
                ModelState.AddModelError("", "A database error occurred. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product variant");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View(productVariantDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditProductVariant(int id)
        {
            var pv = await _context.ProductVaraint.FindAsync(id);
            if (pv == null)
            {
                return NotFound();
            }

            var productvariantDto = new ProductVariantDto
            {
                Id = pv.Id,  // Add this
                attributeName = pv.attributeName,
                attributeValue = pv.attributeValue,
                price_modifier = pv.price_modifier ?? 0,
                ProductId = pv.ProductId
            };
            ViewData["Id"] = id;
            ViewBag.Products = new SelectList(_context.Product, "Id", "ProductName", pv.ProductId);
            return View(productvariantDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductVariant(int Id, ProductVariantDto productVariantDto)
        {
            if (productVariantDto == null)
            {
                return NotFound();
            }

            var pv = await _context.ProductVaraint.FindAsync(Id);
           
            if (!ModelState.IsValid)
            {
                ViewData["Id"] = Id;
                ViewBag.Products = new SelectList(_context.Product, "Id", "ProductName", productVariantDto.ProductId);
                return View(productVariantDto);
            }

            try
            {
                pv.attributeName = productVariantDto.attributeName;
                pv.attributeValue = productVariantDto.attributeValue;
                pv.price_modifier = productVariantDto.price_modifier;
                pv.ProductId = productVariantDto.ProductId;

                _context.Update(pv);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductVariantExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
                ViewData["Id"] = Id;
                ViewBag.Products = new SelectList(_context.Product, "Id", "ProductName", productVariantDto.ProductId);
                return View(productVariantDto);
            }
        }
        private bool ProductVariantExists(int id)
        {
            return _context.ProductVaraint.Any(e => e.Id == id);
        }
        public async Task<IActionResult> DeleteProductVariant(int id)
        {
            var pv = await _context.ProductVaraint.FirstOrDefaultAsync(p => p.Id == id);
            if (pv == null)
            {
                return NotFound();
            }
            _context.Remove(pv);
            _context.SaveChanges();
            return RedirectToAction("Index", "ProductVariant");
        }

        private async Task PopulateProductsViewBag()
        {
            ViewBag.Products = await _context.Product
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();
        }

        private async Task<string> GenerateUniqueSkuAsync()
        {
            string sku;
            int attempts = 0;
            const int maxAttempts = 100;

            using var rng = RandomNumberGenerator.Create();

            do
            {
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var randomBytes = new byte[4];
                rng.GetBytes(randomBytes);
                var randomSuffix = BitConverter.ToUInt32(randomBytes, 0) % 9000 + 1000;
                sku = $"{timestamp}-{randomSuffix}";

                if (++attempts >= maxAttempts)
                {
                    throw new Exception("Unable to generate a unique SKU after multiple attempts.");
                }
            }
            while (await _context.ProductVaraint.AnyAsync(p => p.sku_suffix == sku));

            return sku;
        }
    }
}
