using System.ComponentModel.DataAnnotations;
using MultiVendor.Core.Models;

namespace MultiVendor.Backoffice.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required]
        public string UserRole { get; set; } = "Customer";

        public string CNIC { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserRole == "Vendor")
            {
                if (string.IsNullOrWhiteSpace(CNIC))
                    yield return new ValidationResult("CNIC is required for vendors.", new[] { nameof(CNIC) });

                if (string.IsNullOrWhiteSpace(ContactNumber))
                    yield return new ValidationResult("Contact number is required for vendors.", new[] { nameof(ContactNumber) });

                if (string.IsNullOrWhiteSpace(ShopName))
                    yield return new ValidationResult("Shop name is required for vendors.", new[] { nameof(ShopName) });
            }
        }
    }
}
