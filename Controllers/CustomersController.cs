using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaShop.Data;
using AlphaShop.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using AlphaShop.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AlphaShop.Controllers
{
    public class CustomersController : Controller
    {
        private readonly AlphaShopContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CustomersController> _logger;
        private readonly IEmailService _emailService;

        public CustomersController(AlphaShopContext context, IWebHostEnvironment environment, ILogger<CustomersController> logger,IEmailService emailService)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customer.ToListAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(CustomerDto customerDto)
        {
            if (customerDto == null)
            {
                ModelState.AddModelError("", "Customer data is required.");
                return View(customerDto);
            }

            if (!ModelState.IsValid)
            {
                return View(customerDto);
            }

            if (await _context.Customer.AnyAsync(c => c.Email == customerDto.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(customerDto);
            }

            string? newFileName = null;
            if (customerDto.ProfilePicture != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" }; // Added validation
                var fileExtension = Path.GetExtension(customerDto.ProfilePicture.FileName).ToLower();


                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProfilePicture", "Only image files are allowed.");
                    return View(customerDto);
                }

                newFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + fileExtension;
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await customerDto.ProfilePicture.CopyToAsync(stream);
                }
            }
            string randomCustomerId = GenerateRandomString(10);

            var customer = new Customer
            {
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                DateOfBirth = customerDto.DateOfBirth,
                Email = customerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(customerDto.Password),
                Phone = customerDto.Phone,
                Address = customerDto.Address,
                City = customerDto.City,
                Country = customerDto.Country,
                State = customerDto.State,
                Status = CustomerStatus.Active,
                RegisterDate = DateTime.Now,
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerified = false,
                ProfilePicture = newFileName,
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Customer.Add(customer);
                await _context.SaveChangesAsync();

                await SendVerificationEmail(customer.Email, customer.EmailVerificationToken);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                ModelState.AddModelError("", "An error occurred while creating the customer. Please try again.");
                return View(customerDto);
            }
        }
        private async Task SendVerificationEmail(string email, string token)
        {
            if (_emailService == null)
            {
                _logger.LogError("Email service is not available");
                return;
            }

            var verificationLink = Url.Action(
                "VerifyEmail",
                "Account",
                new { email = email, token = token },
                protocol: HttpContext.Request.Scheme);

            var emailBody = $"Please verify your email by clicking this link: <a href='{verificationLink}'>Verify Email</a>";

            try
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Verify your email address",
                    emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to {Email}", email);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                DateOfBirth = customer.DateOfBirth,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                State = customer.State
            };
            ViewData["Id"] = id;
            ViewData["ProfilePicture"] = customer.ProfilePicture ?? "default.jpg"; // Fixed typo
            return View(customerDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(int id, CustomerDto customerDto)
        {
            // First check if customer exists
            var customerEdit = await _context.Customer.FindAsync(id);
            if (customerEdit == null)
            {
                return NotFound();
            }

            if (customerDto == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["Id"] = id;
                ViewData["ProfilePicture"] = customerEdit.ProfilePicture ?? "default.jpg";
                return View(customerDto);
            }

            try
            {
                // Handle profile picture update
                if (customerDto!.ProfilePicture != null) // Changed to ProfilePictureFile
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(customerEdit.ProfilePicture) &&
                        customerEdit.ProfilePicture != "default.jpg")
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, "images", customerEdit.ProfilePicture);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    var newfile = $"{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(customerDto!.ProfilePicture.FileName)}";
                    var imageFullPath = Path.Combine(_environment.WebRootPath, "images", newfile);

                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await customerDto!.ProfilePicture.CopyToAsync(stream);
                    }

                    customerEdit.ProfilePicture = newfile;
                }

                customerEdit.FirstName = customerDto.FirstName;
                customerEdit.LastName = customerDto.LastName;
                customerEdit.DateOfBirth = customerDto.DateOfBirth;
                customerEdit.Email = customerDto.Email;
                customerEdit.Phone = customerDto.Phone;
                customerEdit.Address = customerDto.Address;
                customerEdit.City = customerDto.City;
                customerEdit.Country = customerDto.Country;
                customerEdit.State = customerDto.State;
                customerEdit.Status = customerDto.Status ?? CustomerStatus.Active;

                customerEdit.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(Request.Form["Password"]))
                {
                    customerEdit.Password = Request.Form["Password"];
                }

                if (!string.IsNullOrEmpty(customerDto.Password))
                {
                    customerEdit.Password = BCrypt.Net.BCrypt.HashPassword(customerDto.Password);
                }


                var changes = await _context.SaveChangesAsync();

                if (changes > 0)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "No changes were saved.");
                    ViewData["Id"] = id;
                    ViewData["ProfilePicture"] = customerEdit.ProfilePicture ?? "default.jpg";
                    return View(customerDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                ViewData["Id"] = id;
                ViewData["ProfilePicture"] = customerEdit.ProfilePicture ?? "default.jpg";
                return View(customerDto);
            }
        }

        [HttpPost]
        public IActionResult DeteleCustomer(int id)
        {
            var customer = _context.Customer.FirstOrDefault(c => c.Id == id);
            if(customer == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(customer.ProfilePicture))
            {
                string imagePath = Path.Combine(_environment.WebRootPath, "image", customer.ProfilePicture);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _context.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction("Index", "Customers");
        }

        [HttpGet]
        public IActionResult DetailCustomer(int id)
        {
            var customer = _context.Customer.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ----------------------- API -----------------------

        //Get Data By API
        [HttpGet("api/customer")]
        public async Task<IActionResult> GetAllCustomer()
        {
            try
            {
                var customers = await _context.Customer.ToListAsync();
                return Ok(new
                {
                    Sucess = customers.Any(),
                    Data = customers.Any() ? customers : null,
                    Message = customers.Any() ? "Customer found" : "No Customer available"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
        }
    }
}