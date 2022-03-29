using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model.Certificates;
using Model.Certificates.Repository;
using Model.Constants;
using Model.Users;
using Model.Users.Repository;

namespace Services.Certificates
{
    public class CertificateRevocationService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        private User _user;
        private Certificate _certificate;

        public CertificateRevocationService(ICertificateRepository certificateRepository, UserManager<User> userManager, IUserRepository userRepository)
        {
            _certificateRepository = certificateRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task Revoke(string username, string serialNumber)
        {
            await ValidateAndSet(username, serialNumber);

            RevokeFor(_certificate);
        }

        private void RevokeFor(Certificate certificate)
        {
            var childCertificates = _certificateRepository.GetAll()
                .Where(x => x.Issuer == certificate.Subject && x.Status == CertificateStatus.Active);

            foreach (var cert in childCertificates.ToList())
            {
                RevokeFor(cert);
            }

            certificate.Status = CertificateStatus.Inactive;
            _certificateRepository.Update(certificate);
        }
        
        private async Task ValidateAndSet(string username, string serialNumber)
        {
            _user = _userRepository.GetAll()
                .Include(x => x.Certificates)
                .AsNoTracking()
                .FirstOrDefault(x => x.UserName == username);

            if (_user == null)
            {
                throw new Exception($"User with username {username} is not found");
            }

            if (await _userManager.IsInRoleAsync(_user, Constants.Admin))
            {
                _user.Certificates = _certificateRepository.GetAll();
            }

            _certificate = _user.Certificates.FirstOrDefault(x => x.SerialNumber == serialNumber);

            if (_certificate == null)
            {
                throw new Exception($"Certificate with serial number {serialNumber} is not found in context of current user");
            }

            if (_certificate.Status == CertificateStatus.Inactive)
            {
                throw new Exception($"Certificate with serial number {serialNumber} is already revoked");
            }
        }
    }
}