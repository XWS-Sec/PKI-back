using System.ComponentModel.DataAnnotations;

namespace Api.DTO
{
    public class UserDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessage = "Not a valid email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Not a valid phone number")]
        public string Phone { get; set; }
    }
}