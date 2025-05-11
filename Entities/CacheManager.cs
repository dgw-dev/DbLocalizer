using Entities.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace Entities
{
    public class CacheManager : ICacheManager
    {
        private IMemoryCache _memoryCache;
        private IConfiguration _config;
        private ISqlSchemaBuilder _sqlSchemaBuilder;

        public CacheManager(IMemoryCache memoryCache, IConfiguration config, ISqlSchemaBuilder sqlSchemaBuilder)
        {
            _memoryCache = memoryCache;
            _config = config;
            _sqlSchemaBuilder = sqlSchemaBuilder;
            BuildCache();
        }

        public void SetCacheValue(string key, object value)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(10));
        }

        public object GetCacheValue(string key)
        {
            return _memoryCache.TryGetValue(key, out var value) ? value : null;
        }

        public void BuildCache()
        {
            if (GetCacheValue("Databases") == null)
            {
                Databases databases = new Databases(_config, _sqlSchemaBuilder);
                if (databases != null || databases.DatabaseCollection != null || databases.DatabaseCollection.Count != 0)
                {
                    SetCacheValue("Databases", databases);
                }
            }
            if (GetCacheValue("Cultures") == null)
            {
                Cultures cultures = new Cultures(_config);
                if (cultures != null || cultures.CultureCollection != null || cultures.CultureCollection.Count != 0)
                {
                    SetCacheValue("Cultures", cultures);
                }
            }
        }
    }
}
