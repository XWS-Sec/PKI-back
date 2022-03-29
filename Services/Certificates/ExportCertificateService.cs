using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model.Certificates;
using Model.Certificates.Repository;
using Model.Constants;
using Model.EnvironmentResolvers;
using Model.Users;
using Model.Users.Repository;

namespace Services.Certificates
{
    public class ExportCertificateService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly UserManager<User> _userManager;
        private readonly CertificateFinderService _certificateFinderService;

        private User user;
        private Certificate certificate;

        public ExportCertificateService(IUserRepository userRepository, ICertificateRepository certificateRepository,UserManager<User> userManager,
            CertificateFinderService certificateFinderService)
        {
            _userRepository = userRepository;
            _certificateRepository = certificateRepository;
            _userManager = userManager;
            _certificateFinderService = certificateFinderService;
        }

        public async Task<string> Export(string username, string serialNumber)
        {
            await ValidateAndFind(username, serialNumber).ConfigureAwait(false);

            var certPath = _certificateFinderService.FindCertificateName(serialNumber);
            if (certPath is null)
            {
                throw new Exception($"Certificate with serial number {serialNumber} not found in storage but found in db.");
            }

            return certPath;
        }

        private async Task ValidateAndFind(string username, string serialNumber)
        {
            user = _userRepository.GetAll()
                .Include(x => x.Certificates)
                .FirstOrDefault(x => x.UserName == username);

            if (user is null)
            {
                throw new Exception($"User with username {username}");
            }

            if (await _userManager.IsInRoleAsync(user, Constants.Admin))
            {
                user.Certificates = _certificateRepository.GetAll();
            }

            certificate = user.Certificates
                .FirstOrDefault(x => x.SerialNumber == serialNumber);

            if (certificate == null)
            {
                throw new Exception(
                    $"User with username {username} doesn't own a certificate with serial number {serialNumber}");
            }
        }
    }
}