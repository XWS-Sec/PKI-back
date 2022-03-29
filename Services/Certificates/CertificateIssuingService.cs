using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Certificates;
using Model.Certificates.Repository;
using Model.Constants;
using Model.EnvironmentResolvers;
using Model.Users;
using Model.Users.Repository;

namespace Services.Certificates
{
    public class CertificateIssuingService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICertificateRepository _certificateRepository;
        private readonly UserManager<User> _userManager;
        private readonly CertificateFinderService _certificateFinderService;

        private bool isAuthority;
        private RSA rsaKey;
        private User issuer;
        private User isuee;

        public CertificateIssuingService(IUserRepository userRepository, ICertificateRepository certificateRepository, UserManager<User> userManager,
            CertificateFinderService certificateFinderService)
        {
            _userRepository = userRepository;
            _certificateRepository = certificateRepository;
            _userManager = userManager;
            _certificateFinderService = certificateFinderService;
        }

        public async Task<Certificate> CreateCertificate(string issuerUserName, string userOwner, string keyUsageFlags, 
            DateTime validFrom, DateTime validTo, string subject, string? issuerSerialNumber)
        {
            ValidateAndSetUsers(issuerUserName, userOwner, subject);

            var flags = ParseFlags(keyUsageFlags);

            if (await _userManager.IsInRoleAsync(isuee, Constants.User) && isAuthority)
            {
                await _userManager.RemoveFromRoleAsync(isuee, Constants.User);
                await _userManager.AddToRoleAsync(isuee, Constants.Intermediate);
            }

            X509Certificate2 issuerCert = null;
            if (issuerSerialNumber != null)
            {
                issuerCert = ValidateAndSetIssuer(issuerUserName, validFrom, validTo, issuerSerialNumber);
            }
            else if (!await _userManager.IsInRoleAsync(issuer, Constants.Admin))
            {
                throw new Exception($"Intermediate users cannot issue new self signed certificates!");
            }

            var generatedCertificate = GenerateCertificate(validFrom, validTo, subject, flags, issuerCert);

            return ExportGeneratedCertificate(generatedCertificate);
        }

        private X509Certificate2 GenerateCertificate(DateTime validFrom, DateTime validTo, string subject,
            X509KeyUsageFlags flags, X509Certificate2 issuerCert)
        {
            subject = $"CN={subject}";
            rsaKey = RSA.Create(2048);

            var certificateRequest =
                new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(
                isAuthority,
                false,
                0,
                true));

            certificateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(flags, false));

            certificateRequest.CertificateExtensions.Add(
                new X509Extension(
                    new AsnEncodedData(
                        "Subject Alternative Name",
                        new byte[] { 48, 11, 130, 9, 108, 111, 99, 97, 108, 104, 111, 115, 116 }
                    ),
                    false
                )
            );

            var generatedCertificate = issuerCert == null
                ? certificateRequest.CreateSelfSigned(validFrom, validTo)
                : certificateRequest.Create(issuerCert, validFrom, validTo,
                    Guid.NewGuid().ToByteArray());
            return generatedCertificate;
        }

        private X509Certificate2 ValidateAndSetIssuer(string issuerUserName, DateTime validFrom, DateTime validTo,
            string? issuerSerialNumber)
        {
            X509Certificate2 issuerCert;
            var foundIssuerInDb = issuer.Certificates
                .FirstOrDefault(x => x.SerialNumber == issuerSerialNumber);
            if (foundIssuerInDb == null)
            {
                throw new Exception(
                    $"Certificate with serial number {issuerSerialNumber} not linked with the user {issuerUserName}");
            }

            if (foundIssuerInDb.Status == CertificateStatus.Inactive)
            {
                throw new Exception(
                    $"Certificate with serial number {issuerSerialNumber} is no longer valid and thus cannot be used for signing new certificates");
            }

            issuerCert = _certificateFinderService.FindCertificate(issuerSerialNumber);
            if (issuerCert == null)
            {
                throw new Exception($"Certificate with serial number {issuerSerialNumber} not found on file system");
            }

            if (issuerCert.NotBefore > validFrom ||
                issuerCert.NotAfter < validTo)
            {
                throw new Exception(
                    $"Issuer certificate validity span is {issuerCert.NotBefore} - {issuerCert.NotAfter} and your certificate is trying to set the span to {validFrom} - {validTo}");
            }

            return issuerCert;
        }

        private void ValidateAndSetUsers(string issuerUserName, string userOwner, string subject)
        {
            issuer = _userRepository.GetAll()
                .Include(x => x.Certificates)
                .FirstOrDefault(x => x.UserName == issuerUserName);

            isuee = _userRepository.GetAll()
                .Include(x => x.Certificates)
                .FirstOrDefault(x => x.UserName == userOwner);

            if (issuer is null)
            {
                throw new Exception($"User with username {issuerUserName} not found!");
            }

            if (isuee is null)
            {
                throw new Exception($"User with username {userOwner} not found!");
            }

            if (_certificateRepository.GetAll()
                    .FirstOrDefault(x => x.Subject == subject) != null)
            {
                throw new Exception($"User with username {userOwner} already has a certificate for subject {subject}");
            }
        }

        private X509KeyUsageFlags ParseFlags(string flags)
        {
            var flagArray = flags.Split(",");
            var retVal = X509KeyUsageFlags.None;

            var possibleElements = Enum.GetValues<X509KeyUsageFlags>();
            var addedFlags = new List<X509KeyUsageFlags>();
            
            foreach (var flag in flagArray)
            {
                if (int.TryParse(flag, out int order))
                {
                    var currFlag = possibleElements[order];
                    if (addedFlags.Contains(currFlag))
                    {
                        throw new Exception($"Flag already added! [{flag}]");
                    }

                    retVal = retVal | currFlag;
                    addedFlags.Add(currFlag);

                    if (currFlag == X509KeyUsageFlags.KeyCertSign)
                        isAuthority = true;
                }
                else
                {
                    throw new Exception($"Unknown flag : {flag}");
                }
            }
            
            return retVal;
        }

        private Certificate ExportGeneratedCertificate(X509Certificate2 generatedCertificate)
        {
            var certForDatabase = new Certificate()
            {
                Issuer = generatedCertificate.Issuer.Substring(3),
                Status = CertificateStatus.Active,
                Subject = generatedCertificate.Subject.Substring(3),
                SerialNumber = generatedCertificate.SerialNumber,
                SignatureAlgorithm = generatedCertificate.SignatureAlgorithm.FriendlyName,
                ValidFrom = generatedCertificate.NotBefore,
                ValidTo = generatedCertificate.NotAfter,
                UserId = isuee.Id
            };
            _certificateRepository.Add(certForDatabase);

            var certCount = _userRepository.GetAll()
                .Include(x => x.Certificates)
                .First(x => x.UserName == isuee.UserName)
                .Certificates.Count();
            
            var certFile = Environment.ExpandEnvironmentVariables(EnvResolver.ResolveCertFolder()) + isuee.UserName + "-" + certCount + "-certificate.pfx";
            
            var exportableCertificate = new X509Certificate2(
                    generatedCertificate.Export(X509ContentType.Cert),
                    (string)null,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet)
                .CopyWithPrivateKey(rsaKey);

            exportableCertificate.FriendlyName = certForDatabase.Subject;
            
            File.WriteAllBytes(certFile,
                exportableCertificate.Export(X509ContentType.Pfx));
            
            return certForDatabase;
        }
    }
}