namespace Demo.Common.Utils;

public enum RoleEnum
{
    ADMIN,
    COMPANY_ADMIN,
    SELLER
}

public enum StatusEnum
{
    ACTIVE,
    INACTIVE,
    LOCK,
    REMOVED
}

public enum BusinessArea
{
    FOOD,
    INDUSTRY,
    ENTERTAINMENT
}

public enum GenderEnum
{
    MALE,
    FEMALE
}

public class SignInEnum
{
    public const string SIGN_UID = "SIGN_UID";

    public const string USERNAME = "USERNAME";
    public const string USERNAME_FIELD = "username";
    public const string PASSWORD = "PASSWORD";
}

public class AuthorizationEnum
{
    public const string AdminManager = "admin_scheme";//for admin and company admin
    public const string AccountManager = "user_scheme";
}