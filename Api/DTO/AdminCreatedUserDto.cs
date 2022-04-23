using System.ComponentModel.DataAnnotations;

namespace Api.DTO
{
    public class AdminCreatedUserDto : UserDto
    {
        [Required(ErrorMessage = "Role is required!")]
        public string Role { get; set; }
    }
}