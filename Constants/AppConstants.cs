namespace WebApi.NetCore.Constants;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
    }

    public static readonly string[] ValidRoles = [Roles.Admin, Roles.Manager, Roles.User];
}
