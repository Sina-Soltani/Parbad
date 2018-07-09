namespace Parbad.Infrastructure.Caching
{
    internal static class CacheManagerExtensions
    {
        public static T GetAs<T>(this ICacheManager cacheManager, string key)
        {
            return (T)cacheManager.Get(key);
        }
    }
}