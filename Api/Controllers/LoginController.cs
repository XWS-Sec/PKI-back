using System.Threading.Tasks;
using Api.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model.Users;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public LoginController(UserManager<User> userManager, SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto credentials)
        {
            var user = await _userManager.FindByNameAsync(credentials.Username);
            if (user is null)
            {
                ModelState.AddModelError(nameof(LoginDto.Username),
                    $"Username {credentials.Username} is not registered");
                return BadRequest(ModelState);
            }

            var result =
                await _signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(credentials.Password), "Incorrect password!");
                return BadRequest(ModelState);
            }

            return Ok("Successful login!");
        }

        [HttpPost("/api/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}