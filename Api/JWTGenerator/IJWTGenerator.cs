using System.Collections.Generic;
using Model.Users;

namespace Api.JWTGenerator
{
    public interface IJWTGenerator
    {
        string GenerateToken(User user, IList<string> roles);
    }
}