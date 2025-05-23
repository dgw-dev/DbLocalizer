using Entities;
using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DbLocalizer.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IMemoryCache _memoryCache;
        protected TokenStore TokenSore { get; set; }
        protected ICacheManager CacheManager { get; set; }
        protected readonly IConfiguration _config;
        protected readonly ISqlSchemaBuilder _schemaBuilder;

        protected BaseController(IMemoryCache memoryCache, IConfiguration config, ISqlSchemaBuilder sqlSchemaBuilder, ILogger logger, IEncryptionService encryptionService)
        {
            _memoryCache = memoryCache;
            TokenSore = TokenStore.Instance;
            CacheManager = new CacheManager(_memoryCache, config, sqlSchemaBuilder, encryptionService);
            _config = config;
            _schemaBuilder = sqlSchemaBuilder;
            _logger = logger;
        }

        protected void LogMessage(string message, bool isError = false)
        {
            if (isError)
            {
                _logger.LogError(message);
            }
            else
            {
                _logger.LogWarning(message);
            }
        }
    }
}