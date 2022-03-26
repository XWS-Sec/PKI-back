using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Model.EnvironmentResolvers;
using Model.Users;

namespace Api.Controllers
{
    [ApiController]
    [Route("/ping")]
    public class PingController : ControllerBase
    {
        private readonly UserManager<User> _userStore;

        public PingController(UserManager<User> userStore)
        {
            _userStore = userStore;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var admin = EnvResolver.ResolveAdminUser();
            var adminUser = _userStore.Users.First(x => x.UserName == admin);

            return Ok($"Greetings from {adminUser.UserName}");
        }
    }
}