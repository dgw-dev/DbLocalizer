using Entities;
using Entities.BL;
using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Threading.Tasks;

namespace DbLocalizer.Controllers
{
    [EnableCors("DbLocalizerCorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class CommonServiceController : BaseController
    {
        public CommonServiceController(IMemoryCache memoryCache,
            IConfiguration config,
            ISqlSchemaBuilder sqlSchemaBuilder,
            ILogger<CommonServiceController> logger,
            IEncryptionService encryptionService) : base(memoryCache, config, sqlSchemaBuilder, logger, encryptionService)
        {
        }

        [Authorize]
        [HttpGet("CheckConnectivity")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public IActionResult CheckConnectivity()
        {
            ReturnData result = new ReturnData();

            if (CacheManager != null)
            {
                Databases databases = CacheManager.GetCacheValue("Databases") as Databases;
                if (databases?.DatabaseCollection?.Count > 0)
                {
                    foreach (var db in databases.DatabaseCollection)
                    {
                        if (db.CanConnect)
                        {
                            result.LogEntries.Add(new LogEntry { LogLevel = "Information", Message = db.Name + " could connect successfully" });
                        }
                        else if (db.ConnectionStringValue == null)
                        {
                            result.LogEntries.Add(new LogEntry { LogLevel = "Error", Message = db.Name + " connectionStringValue is null" });
                        }
                        else
                        {
                            result.LogEntries.Add(new LogEntry { LogLevel = "Error", Message = db.Name + " could not connect" });
                        }
                    }
                }
            }
            string returnValue = JsonConvert.SerializeObject(result, Formatting.Indented);
            return Ok(returnValue);
        }

        [Authorize]
        [HttpGet("GetLog")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLog()
        {
            ReturnData<DataTable> result = new ReturnData<DataTable>();
            result.Data = await SqliteUtility.GetData("logs");
            string returnValue = JsonConvert.SerializeObject(result, Formatting.Indented);
            return Ok(returnValue);
        }

        [Authorize]
        [HttpGet("TruncateLog")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TruncateLog()
        {
            bool result = await SqliteUtility.TruncateData("logs");
            var message = new
            {
                ResultValue = result
            };
            string returnValue = JsonConvert.SerializeObject(message, Formatting.Indented);
            return Ok(returnValue);
        }
    }
}
