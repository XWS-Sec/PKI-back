using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Model.EnvironmentResolvers;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var certPath = EnvResolver.ResolveCertFolder();
                    var pfxPath = Environment.ExpandEnvironmentVariables(certPath) + "apiCert.pfx";
                    var certPass = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");

                    var certificate = new X509Certificate2(
                        pfxPath,
                        certPass);

                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(System.Net.IPAddress.Loopback, 44321, listenOptions =>
                        {
                            var connectionOptions = new HttpsConnectionAdapterOptions();
                            connectionOptions.ServerCertificate = certificate;

                            listenOptions.UseHttps();
                        });
                    });
                    
                    webBuilder.UseStartup<Startup>();
                });
        }
            
    }
}