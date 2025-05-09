namespace ClbTinhoc.Web.Models
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";

        public static bool IsValidRole(string role)
        {
            return role == Admin || role == User;
        }
    }
}