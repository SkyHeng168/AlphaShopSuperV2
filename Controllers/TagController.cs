using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlphaShop.Controllers
{
    public class TagController : Controller
    {
        private readonly AlphaShopContext _context;
        public TagController(AlphaShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tags = _context.Tag
                .Include(t => t.ProductTags)
                .ToList();
            return View(tags);
        }

        [HttpGet]
        public IActionResult createTag()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> createTag(TagDto tagDto)
        {
            if(tagDto == null)
            {
                ModelState.AddModelError("", "Invalid Tag Data");
                return View(tagDto);
            }
            if (!ModelState.IsValid)
            {
                return View(tagDto);
            }
            var tag = new Tag
            {
                Name = tagDto.Name,
                Slug = tagDto.Slug
            };
            try
            {
                _context.Tag.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Tag");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the category. Please try again.");
                return View(tagDto);
            }
        }

        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tag.FirstOrDefaultAsync(t => t.Id == id);
            if(tag == null)
            {
                return NotFound();
            }
            _context.Remove(tag);
            _context.SaveChanges();
            return RedirectToAction("Index", "Tag");
        }

        [HttpGet]
        public async Task<IActionResult> EditTag(int id)
        {
            var tag = await _context.Tag.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            var tagDto = new TagDto
            {
                Name = tag.Name,
                Slug = tag.Slug
            };

            return View(tagDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTag(int id, TagDto tagDto)
        {
            if (tagDto == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Tag Data.");
                return View(tagDto);
            }

            var existingTag = await _context.Tag.FindAsync(id);
            if (existingTag == null)
            {
                return NotFound();
            }

            existingTag.Name = tagDto.Name;
            existingTag.Slug = tagDto.Slug;

            try
            {
                _context.Tag.Update(existingTag);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Tag");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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
                ModelState.AddModelError("", $"An error occurred while updating the tag. Error: {ex.Message}");
                return View(tagDto);
            }
        }

        private bool TagExists(int id)
        {
            return _context.Tag.Any(e => e.Id == id);
        }
    }
}
