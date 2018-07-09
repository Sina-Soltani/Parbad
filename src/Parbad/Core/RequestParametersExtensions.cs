namespace Parbad.Core
{
    internal static class RequestParametersExtensions
    {
        public static T GetAs<T>(this IRequestParameters callbackCommandParameter, string key, bool caseSensitive = false)
        {
            var result = callbackCommandParameter.Get(key, caseSensitive);

            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }
    }
}