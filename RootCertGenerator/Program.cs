﻿using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Model.EnvironmentResolvers;

namespace RootCertGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var dir = EnvResolver.ResolveCertFolder();
            var dirPath = Environment.ExpandEnvironmentVariables(dir);

            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            var certDir = dirPath + "apiCert.pfx";
            var certPath = Environment.ExpandEnvironmentVariables(certDir);

            if (File.Exists(certPath))
            {
                Console.WriteLine("Certificate already exists!");
                return;
            }

            var rsaKey = RSA.Create(2048);

            var subject = "CN=localhost";

            var certificateRequest =
                new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(
                true,
                false,
                0,
                true));

            certificateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.KeyCertSign,
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

            var expireAt = DateTime.Now.AddYears(10);

            var certificate = certificateRequest.CreateSelfSigned(DateTime.Now, expireAt);

            var exportableCertificate = new X509Certificate2(
                    certificate.Export(X509ContentType.Cert),
                    (string)null,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet)
                .CopyWithPrivateKey(rsaKey);

            exportableCertificate.FriendlyName = "ApiCertificate";

            var password = new SecureString();
            foreach (var @char in EnvResolver.ResolveAdminPass()) password.AppendChar(@char);

            File.WriteAllBytes(
                certPath,
                exportableCertificate.Export(
                    X509ContentType.Pfx,
                    password));

            Console.WriteLine("Certificate created successfully! Path : " + certPath);
        }
    }
}