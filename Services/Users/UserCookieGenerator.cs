using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Model.Users;

namespace Services.Users
{
    public class UserCookieGenerator
    {
        public void Fill(IResponseCookies cookies, User user, string role)
        {
            cookies.Append("username", user.UserName);
            cookies.Append("email", user.Email ?? string.Empty);
            cookies.Append("name", user.Name);
            cookies.Append("surname", user.Surname);
            cookies.Append("role", role);
        }
    }
}