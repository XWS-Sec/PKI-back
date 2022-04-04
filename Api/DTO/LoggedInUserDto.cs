using System;

namespace Api.DTO
{
    public class LoggedInUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
    }
}