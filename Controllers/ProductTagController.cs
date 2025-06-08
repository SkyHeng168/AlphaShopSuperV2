using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace AlphaShop.Controllers
{
    public class ProductTagController : Controller
    {
        private readonly AlphaShopContext _context;
        public ProductTagController(AlphaShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var productTags = _context.ProductTag
                .Include(pt => pt.Product)
                .Include(pt => pt.Tag)
                .ToList();
            return View(productTags);
        }

        [HttpGet]
        public async Task<IActionResult> createProductTag()
        {
            try
            {
                ViewBag.Tags = await _context.Tag
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                    .ToListAsync();

                ViewBag.Products = await _context.Product
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.ProductName
                    })
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                // Log the error (you should implement proper logging)
                ModelState.AddModelError("", "An error occurred while loading the form");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProductTag(ProductTag productTag)
        {
            try
            {
                if (productTag == null)
                {
                    ModelState.AddModelError("", "Invalid ProductTag Data");
                    return await ReloadFormWithErrors();
                }

                if (!ModelState.IsValid)
                {
                    return await ReloadFormWithErrors(productTag);
                }

                // Set creation and update dates
                productTag.CreateAt = DateTime.UtcNow;

                // Check if the product-tag combination already exists
                bool exists = await _context.ProductTag
                    .AnyAsync(pt => pt.ProductId == productTag.ProductId && pt.TagId == productTag.TagId);

                if (exists)
                {
                    ModelState.AddModelError("", "This product-tag combination already exists");
                    return await ReloadFormWithErrors(productTag);
                }

                _context.ProductTag.Add(productTag);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product tag created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error (you should implement proper logging)
                ModelState.AddModelError("", "An error occurred while saving the product tag");
                return await ReloadFormWithErrors(productTag);
            }
        }

        private async Task<IActionResult> ReloadFormWithErrors(ProductTag productTag = null)
        {
            ViewBag.Tags = await _context.Tag
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            ViewBag.Products = await _context.Product
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();

            return View(productTag);
        }
    }
}
