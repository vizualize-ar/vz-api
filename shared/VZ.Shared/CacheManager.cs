using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace VZ.Shared
{
    public static class CacheManager
    {
        //private static ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();
        private static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static object Get(string key)
        {            
            if (_cache.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public static TResult Get<TResult>(string key)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return (TResult)value;
            }
            return default;
        }

        public static void Set(string key, object value, TimeSpan? expiresInMinutes = null)
        {
            if (expiresInMinutes == null || expiresInMinutes.HasValue == false)
            {
                expiresInMinutes = TimeSpan.FromMinutes(2);
            }
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = expiresInMinutes.Value
            };
            _cache.Set(key, value, options);
        }

        public static void Remove(string key)
        {
            //if (_cache.ContainsKey(key))
            //{
            //    _cache.TryRemove(key, out var obj);
            //}
            _cache.Remove(key);
        }
    }
}
