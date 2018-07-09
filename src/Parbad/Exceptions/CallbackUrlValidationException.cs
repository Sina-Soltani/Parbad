using System;

namespace Parbad.Exceptions
{
    public class CallbackUrlValidationException : Exception
    {
        public CallbackUrlValidationException(string url) : base($"The format of Callback URL {url} is not valid. A valid URL would be like: http://www.site.com/foo/bar")
        {
        }
    }
}