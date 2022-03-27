using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Model.EnvironmentResolvers;

namespace RootCertGenerator
{
    public class GenerateIntermediateCA
    {
        public GenerateIntermediateCA()
        {
            var certPath = EnvResolver.ResolveCertFolder();
            var pfxPath = Environment.ExpandEnvironmentVariables(certPath) + "apiCert.pfx";
            var certPass = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");

            var newPath = Environment.ExpandEnvironmentVariables(certPath) + "intermediate.pfx";
            
            var certificate = new X509Certificate2(
                pfxPath,
                certPass);

            var rsaKey = RSA.Create(2048);

            var subject = "CN=intermediate";

            var certificateRequest =
                new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(
                true,
                false,
                0,
                true));
            
            certificateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature |
                X509KeyUsageFlags.KeyEncipherment |
                X509KeyUsageFlags.KeyCertSign |
                X509KeyUsageFlags.CrlSign,
                false));
            
            certificateRequest.CertificateExtensions.Add(
                new X509Extension(
                    new AsnEncodedData(
                        "Subject Alternative Name",
                        new byte[] { 48, 11, 130, 9, 108, 111, 99, 97, 108, 104, 111, 115, 116 }
                    ),
                    false
                )
            );
            
            var expireAt = certificate.NotAfter.AddDays(-1);

            var interCert =
                certificateRequest.Create(certificate, DateTime.Now, expireAt, Guid.NewGuid().ToByteArray());
            
            var exportableCertificate = new X509Certificate2(
                    interCert.Export(X509ContentType.Cert),
                    (string)null,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet)
                .CopyWithPrivateKey(rsaKey);

            exportableCertificate.FriendlyName = subject.Substring(3);

            var password = new SecureString();
            foreach (var @char in EnvResolver.ResolveAdminPass()) password.AppendChar(@char);

            File.WriteAllBytes(
                newPath,
                exportableCertificate.Export(
                    X509ContentType.Pfx,
                    password));

            Console.WriteLine("Certificate created successfully! Path : " + newPath);
        }
    }
}