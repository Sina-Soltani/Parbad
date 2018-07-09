namespace Parbad.Infrastructure.Caching
{
    internal static class CacheManagerFactory
    {
        private static ICacheManager _instance;
        private static readonly object LockObject = new object();

        public static ICacheManager Instance
        {
            get
            {
                lock (LockObject)
                {
                    return _instance ?? (_instance = new MemoryCacheManager());
                }
            }
        }
    }
}