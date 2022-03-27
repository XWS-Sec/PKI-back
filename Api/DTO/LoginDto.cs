using System.ComponentModel.DataAnnotations;

namespace Api.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is necessary")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is mandatory")]
        public string Password { get; set; }
    }
}