using System.Threading.Tasks;
using Api.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model.Constants;
using Model.Users;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RegisterController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(UserDto newUser)
        {
            var userToCreate = _mapper.Map<User>(newUser);
            var result = await _userManager.CreateAsync(userToCreate, newUser.Password);

            if (!result.Succeeded)
            {
                foreach (var identityError in result.Errors)
                    ModelState.AddModelError(identityError.Code, identityError.Description);

                return BadRequest(ModelState);
            }

            await _userManager.AddToRoleAsync(userToCreate, Constants.User);

            return Ok("Successfully registered");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/api/register/create")]
        public async Task<IActionResult> CreateUser(AdminCreatedUserDto newUser)
        {
            var matchedRole = false;
            foreach (var role in Constants.GetRoles())
                if (role == newUser.Role)
                {
                    matchedRole = true;
                    break;
                }

            if (!matchedRole)
            {
                ModelState.AddModelError(nameof(newUser.Role), $"Nonexistants role {newUser.Role}");
                return BadRequest(ModelState);
            }

            if (newUser.Role == Constants.Admin)
            {
                ModelState.AddModelError(nameof(newUser.Role), $"Cannot create users in role - {newUser.Role}");
                return BadRequest(ModelState);
            }

            var userToCreate = _mapper.Map<User>(newUser);
            var result = await _userManager.CreateAsync(userToCreate, newUser.Password);

            if (!result.Succeeded)
            {
                foreach (var identityError in result.Errors)
                    ModelState.AddModelError(identityError.Code, identityError.Description);

                return BadRequest(ModelState);
            }

            await _userManager.AddToRoleAsync(userToCreate, newUser.Role);

            return Ok($"Successfully registered user : {userToCreate.UserName}");
        }
    }
}