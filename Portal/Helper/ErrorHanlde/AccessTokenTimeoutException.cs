using System.Globalization;

namespace Demo.Portal.Helper.ErrorHanlde
{
    public class AccessTokenTimeoutException : Exception
    {
        public AccessTokenTimeoutException() : base() { }

        public AccessTokenTimeoutException(string message) : base(message) { }

        public AccessTokenTimeoutException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
