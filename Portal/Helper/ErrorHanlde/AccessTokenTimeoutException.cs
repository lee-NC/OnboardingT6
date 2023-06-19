using System.Globalization;

namespace Demo.Portal.Helper.ErrorHanlde;

public class AccessTokenTimeoutException : Exception
{
    public AccessTokenTimeoutException()
    {
    }

    public AccessTokenTimeoutException(string message) : base(message)
    {
    }

    public AccessTokenTimeoutException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}