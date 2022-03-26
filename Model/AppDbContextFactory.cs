using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Model.Certificates;
using Model.EnvironmentResolvers;
using Model.Users;

namespace Model
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(EnvResolver.GetConnectionString());
            
            return new AppDbContext(optionsBuilder.Options);
        }

        public static void ConfigureOptions(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(EnvResolver.GetConnectionString());
            using (var context = new AppDbContext((DbContextOptions<AppDbContext>) options.Options))
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }

                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore, null, null, null, null);

                var rolesToCheck = new string[] { "User", "Intermediate", "Admin" };
                foreach (var role in rolesToCheck)
                {
                    if (!context.Roles.Any(r => r.Name == role))
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }

                var hasher = new PasswordHasher<User>();
                var userStore = new UserStore<User>(context);
                var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);

                var adminUserToCheck = EnvResolver.ResolveAdminUser();
                if (!context.Users.Any(u => u.UserName == adminUserToCheck))
                {
                    var adminPass = EnvResolver.ResolveAdminPass();
                    var admin = new User();
                    admin.Name = "Api admin";
                    admin.Surname = "admin";
                    admin.UserName = adminUserToCheck;
                    admin.PasswordHash = hasher.HashPassword(admin, adminPass);

                    userStore.CreateAsync(admin).Wait();
                    userManager.AddToRoleAsync(admin, "Admin").Wait();
                }

                if (!context.Certificates.Any())
                {
                    var certPath = EnvResolver.ResolveCertFolder();
                    var pfxPath = Environment.ExpandEnvironmentVariables(certPath) + "apiCert.pfx";
                    var certPass = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");

                    var certificate = new X509Certificate2(
                        pfxPath,
                        certPass);

                    var admin = context.Users.First(x => x.UserName == adminUserToCheck);

                    context.Certificates.Add(new Certificate()
                    {
                        Issuer = certificate.Issuer,
                        Status = CertificateStatus.Active,
                        Subject = certificate.Subject,
                        SerialNumber = certificate.SerialNumber,
                        SignatureAlgorithm = certificate.SignatureAlgorithm.FriendlyName,
                        UserId = admin.Id,
                        ValidFrom = certificate.NotBefore,
                        ValidTo = certificate.NotAfter
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}