using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaShop.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? CustomerId { get; set; }
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }

        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }

        public string? EmailVerificationToken { get; set; }
        public bool EmailVerified { get; set; } = false;

        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<Order> Order { get; set; } = new List<Order>();
    }
    public enum CustomerStatus
    {
        Active,
        Inactive,
        Banned
    }

    public class CustomerDto
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? Password { get; set; }

        public CustomerStatus? Status { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }

        public IFormFile? ProfilePicture { get; set; }
    }
}
