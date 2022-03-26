using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Model.EnvironmentResolvers;
using Model.Users;

namespace Api.JWTGenerator
{
    public class JWTTokenGenerator : IJWTGenerator
    {
        public string GenerateToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secret = GetSecret();
            var key = new SymmetricSecurityKey(secret);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.Aes256Encryption);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
                Issuer = "Api"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private byte[] GetSecret()
        {
            var keyFile = EnvResolver.ResolveCertFolder() + "secret";
            var filePath = Environment.ExpandEnvironmentVariables(keyFile);

            var secret = File.ReadAllText(filePath);

            return Encoding.UTF8.GetBytes(secret);
        }
    }
}