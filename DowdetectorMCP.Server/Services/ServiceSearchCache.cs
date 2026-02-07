using DowndetectorMCP.API.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DowdetectorMCP.Server.Services
{
    public interface IServiceSearchCache
    {
        bool TryGetValue(string serviceName, string country, out SearchServiceResult? result);
        void Set(string serviceName, string country, SearchServiceResult result);
        void Clear();
    }

    public class ServiceSearchCache : IServiceSearchCache
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration;

        public ServiceSearchCache(IMemoryCache memoryCache, TimeSpan? cacheDuration = null)
        {
            _cache = memoryCache;
            _cacheDuration = cacheDuration ?? TimeSpan.FromHours(24);
        }

        public bool TryGetValue(string serviceName, string country, out SearchServiceResult? result)
        {
            var key = GetCacheKey(serviceName, country);
            return _cache.TryGetValue(key, out result);
        }

        public void Set(string serviceName, string country, SearchServiceResult result)
        {
            var key = GetCacheKey(serviceName, country);
            _cache.Set(key, result, _cacheDuration);
        }

        public void Clear()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
        }

        private static string GetCacheKey(string serviceName, string country)
        {
            return $"{country.ToLowerInvariant()}:{serviceName.ToLowerInvariant()}";
        }
    }
}