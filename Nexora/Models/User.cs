using System;
using System.ComponentModel.DataAnnotations;

namespace Nexora.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}