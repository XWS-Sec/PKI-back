using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model.Certificates;
using Model.Certificates.Repository;
using Model.Constants;
using Model.Users;
using Model.Users.Repository;

namespace Services.Certificates
{
    public class GetCertificateService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;

        public GetCertificateService(ICertificateRepository certificateRepository, IUserRepository userRepository, UserManager<User> userManager)
        {
            _certificateRepository = certificateRepository;
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<User> GetUserCertificates(string username)
        {
            var user = _userRepository.GetAll()
                .Include(x => x.Certificates)
                .FirstOrDefault(x => x.UserName == username);

            if (await _userManager.IsInRoleAsync(user, Constants.Admin))
            {
                user.Certificates = _certificateRepository.GetAll();
            }

            return user;
        }
    }
}