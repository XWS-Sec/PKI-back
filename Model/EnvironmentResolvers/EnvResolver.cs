using System;
using System.Text;

namespace Model.EnvironmentResolvers
{
    public static class EnvResolver
    {
        public static string GetConnectionString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("host=").Append(ResolveHost()).Append(";");
            stringBuilder.Append("port=").Append(ResolvePort()).Append(";");
            stringBuilder.Append("database=").Append(ResolveDatabase()).Append(";");
            stringBuilder.Append("username=").Append(ResolveUsername()).Append(";");
            stringBuilder.Append("password=").Append(ResolvePassword());

            return stringBuilder.ToString();
        }

        public static string ResolveHost()
        {
            var host = Environment.GetEnvironmentVariable("XWS_PKI_HOST");
            return string.IsNullOrEmpty(host)
                ? "localhost"
                : host;
        }

        public static string ResolvePort()
        {
            var port = Environment.GetEnvironmentVariable("XWS_PKI_PORT");
            return string.IsNullOrEmpty(port)
                ? "5432"
                : port;
        }

        public static string ResolveDatabase()
        {
            var database = Environment.GetEnvironmentVariable("XWS_PKI_DATABASE");
            return string.IsNullOrEmpty(database)
                ? "XWSPKI"
                : database;
        }

        public static string ResolveUsername()
        {
            var username = Environment.GetEnvironmentVariable("XWS_PKI_USERNAME");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("Username for database is mandatory!");

            return username;
        }

        public static string ResolvePassword()
        {
            var password = Environment.GetEnvironmentVariable("XWS_PKI_PASSWORD");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("Password for database is mandatory!");

            return password;
        }

        public static string ResolveAdminPass()
        {
            var password = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("Admin password needs to be set for database");

            return password;
        }

        public static string ResolveAdminUser()
        {
            var username = Environment.GetEnvironmentVariable("XWS_PKI_ADMINUSER");

            return string.IsNullOrEmpty(username)
                ? "admin"
                : username;
        }

        public static string ResolveCertFolder()
        {
            var rootFolder = Environment.GetEnvironmentVariable("XWS_PKI_ROOT_CERT_FOLDER");
            return string.IsNullOrEmpty(rootFolder)
                ? @"%USERPROFILE%\.xws-cert\"
                : rootFolder;
        }
    }
}