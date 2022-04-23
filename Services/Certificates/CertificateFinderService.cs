using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Model.EnvironmentResolvers;

namespace Services.Certificates
{
    public class CertificateFinderService
    {
        public X509Certificate2 FindCertificate(string serialNumber)
        {
            var dirPath = Environment.ExpandEnvironmentVariables(EnvResolver.ResolveCertFolder());
            var files = Directory.GetFiles(dirPath);

            foreach (var cert in files)
            {
                X509Certificate2 certificate = null;
                if (cert.EndsWith("apiCert.pfx"))
                {
                    certificate = new X509Certificate2(cert, EnvResolver.ResolveAdminPass());
                }
                else
                {
                    certificate = new X509Certificate2(X509Certificate.CreateFromCertFile(cert));   
                }
                if (certificate.SerialNumber == serialNumber)
                    return certificate;
            }
            
            return null;
        }

        public string? FindCertificateName(string serialNumber)
        {
            var dirPath = Environment.ExpandEnvironmentVariables(EnvResolver.ResolveCertFolder());
            var files = Directory.GetFiles(dirPath);
            foreach (var cert in files)
            {
                X509Certificate2 certificate = null;
                if (cert.EndsWith("apiCert.pfx"))
                {
                    certificate = new X509Certificate2(cert, EnvResolver.ResolveAdminPass());
                }
                else
                {
                    certificate = new X509Certificate2(X509Certificate.CreateFromCertFile(cert));   
                }

                if (certificate.SerialNumber == serialNumber)
                    return cert;
            }

            return null;
        }
    }
}