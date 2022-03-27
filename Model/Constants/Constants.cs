namespace Model.Constants
{
    public static class Constants
    {
        public static string Admin = "Admin";
        public static string Intermediate = "Intermediate";
        public static string User = "User";

        public static string[] GetRoles()
        {
            return new[] { Admin, Intermediate, User };
        }
    }
}