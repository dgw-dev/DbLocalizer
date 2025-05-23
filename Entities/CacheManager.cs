using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace Entities
{
    public class CacheManager : ICacheManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _config;
        private readonly ISqlSchemaBuilder _sqlSchemaBuilder;
        private readonly IEncryptionService _encryptionService;

        public CacheManager(IMemoryCache memoryCache, IConfiguration config, ISqlSchemaBuilder sqlSchemaBuilder, IEncryptionService encryptionService)
        {
            _memoryCache = memoryCache;
            _config = config;
            _sqlSchemaBuilder = sqlSchemaBuilder;
            _encryptionService = encryptionService;
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
                Databases databases = new Databases(_config, _sqlSchemaBuilder, _encryptionService);
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
