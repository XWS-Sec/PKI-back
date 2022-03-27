using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            using (var context = new AppDbContext((DbContextOptions<AppDbContext>)options.Options))
            {
                if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();

                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager =
                    new RoleManager<IdentityRole>(roleStore, null, new UpperInvariantLookupNormalizer(), null, null);

                var rolesToCheck = Constants.Constants.GetRoles();
                foreach (var role in rolesToCheck)
                    if (!context.Roles.Any(r => r.Name == role))
                    {
                        var identityRole = new IdentityRole(role);
                        roleManager.CreateAsync(identityRole).GetAwaiter().GetResult();
                    }

                var hasher = new PasswordHasher<User>();
                var userStore = new UserStore<User>(context);
                var userManager = new UserManager<User>(userStore, null, null, null, null,
                    new UpperInvariantLookupNormalizer(), null, null, null);

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
                    userManager.AddToRoleAsync(admin, Constants.Constants.Admin).Wait();
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

                    context.Certificates.Add(new Certificate
                    {
                        Issuer = certificate.Issuer.Substring(3),
                        Status = CertificateStatus.Active,
                        Subject = certificate.Subject.Substring(3),
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