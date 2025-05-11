using Entities;
using Entities.BL;
using Entities.Interfaces;
using Entities.Plugins.TranslationManagement.Smartling;
using Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DbLocalizer.Controllers
{
    [EnableCors("DbLocalizerCorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class SmartlingImportController : BaseController
    {
        private readonly IBackgroundWorkerQueue _backgroundWorkerQueue;
        private readonly ISmartlingImportUtility _importUtility;
        private readonly ISmartlingFileDataService _smartlingFileDataService;

        public SmartlingImportController(
            IBackgroundWorkerQueue backgroundWorkerQueue,
            ILogger<SmartlingImportController> logger,
            ISmartlingFileDataService smartlingFileDataService,
            ISmartlingImportUtility importUtility,
            IMemoryCache memoryCache,
            IConfiguration config,
            ISqlSchemaBuilder sqlSchemaBuilder
        ) : base(memoryCache, config, sqlSchemaBuilder, logger)
        {
            _backgroundWorkerQueue = backgroundWorkerQueue;
            _importUtility = importUtility;
            _smartlingFileDataService = smartlingFileDataService;
        }

        /// <summary>
        /// Callback end point for Smartling import files sent from Smartling
        /// </summary>
        /// <param name="fileUri"></param>
        /// <param name="publishedStatus"></param>
        /// <param name="locale"></param>
        /// <remarks>
        /// <para>Returns a Json string</para>
        /// <para>Example</para>
        /// <para>
        /// {
        ///"OperationComplete": false,
        ///"LogEntries": [],
        ///"ImportErrors": null,
        ///"IsJobRunning": true,
        ///"ProcessId": "07b6de42-5514-445b-931a-06cd4fded060",
        /// "IsCancelled": false,
        /// }
        /// </para>
        /// </remarks>
        /// <response code="200">Returns a Json string</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromQuery] string fileUri, [FromQuery] string publishedStatus, [FromQuery] string locale)
        {
            if (CacheManager.GetCacheValue("Databases") == null || CacheManager.GetCacheValue("Cultures") == null)
            {
                return BadRequest("The required database and/or culture data is null");
            }

            if (string.IsNullOrEmpty(publishedStatus) || !string.Equals(publishedStatus, "published", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("The requested file is not in a published state");
            }

            Guid processId = Guid.NewGuid();

            ReturnData returnData = new ReturnData()
            {
                NumberOfProcessErrors = 0,
                LogEntries = new List<LogEntry>(),
                IsJobRunning = false,
                ProcessId = processId
            };

            try
            {
                if (string.IsNullOrEmpty(fileUri))
                {
                    throw new ArgumentException("fileUri is null or empty");
                }

                if (string.IsNullOrEmpty(locale))
                {
                    throw new ArgumentException("locale is null or empty");
                }

                // Download import json from smartling
                var importJson = await _smartlingFileDataService.GetTranslatedFileForLocaleAsync(locale, fileUri, processId);

                // Run the import
                //do something with the importPackage
                SmartlingImportSqlFilePackageCollection importPackage = await _importUtility.Import(importJson);

                returnData.IsJobRunning = _backgroundWorkerQueue.IsRunning;
                returnData.OperationComplete = true;

                return Ok(JsonUtility.SerializeData<ReturnData>(returnData));
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message, true);
                returnData.LogEntries.Add(new LogEntry() { Message = ex.ToString() });
                return BadRequest(JsonUtility.SerializeData<ReturnData>(returnData));
            }
        }

        /// <summary>
        /// Manual upload end point for Smartling import files
        /// </summary>
        /// <param name="package"></param>
        /// <remarks>
        /// <para>Accepts a RawImportFile object</para>
        /// <para>Example</para>
        /// <para>
        /// {
        ///(IFormFile)File,
        ///(string)Locale,
        /// }
        /// </para>
        /// <para>Returns a Json string</para>
        /// <para>Example</para>
        /// <para>
        /// {
        ///"OperationComplete": false,
        ///"LogEntries": [],
        ///"ImportErrors": null,
        ///"IsJobRunning": true,
        ///"ProcessId": "07b6de42-5514-445b-931a-06cd4fded060",
        /// "IsCancelled": false,
        /// }
        /// </para>
        /// </remarks>
        /// <response code="200">Returns a Json string</response>
        [Authorize]
        [HttpPost("UploadImportFiles")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadImportFiles(RawImportFile package)
        {
            if (CacheManager.GetCacheValue("Databases") == null || CacheManager.GetCacheValue("Cultures") == null)
            {
                return BadRequest("The required database and/or culture data is null");
            }

            if (package == null || package.File == null)
            {
                return BadRequest("No file data was received");
            }

            if (package == null || string.IsNullOrEmpty(package.Locale))
            {
                return BadRequest("No locale data was received");
            }

            Guid processId = Guid.NewGuid();

            ReturnData returnData = new ReturnData()
            {
                NumberOfProcessErrors = 0,
                LogEntries = new List<LogEntry>(),
                IsJobRunning = false,
                ProcessId = processId
            };

            try
            {
                List<SmartlingImportJsonFile> importJsonFiles = new List<SmartlingImportJsonFile>();
                using (var reader = new StreamReader(package.File.OpenReadStream()))
                {
                    string content = await reader.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(content))
                    {
                        SmartlingFileData smartlingFileData = JsonUtility.DeserializeData<SmartlingFileData>(content);
                        smartlingFileData.GlobalizationMetaData.Locale = package.Locale;
                        importJsonFiles.Add(new SmartlingImportJsonFile(smartlingFileData, package.File.Name, processId));
                    }
                }

                // Validate the import JSON
                SmartlingImportJsonFileCollection importJsonFileCollection = new SmartlingImportJsonFileCollection(importJsonFiles);

                // Run the import
                //do something with the importPackage
                SmartlingImportSqlFilePackageCollection importPackage = await _importUtility.Import(importJsonFileCollection);

                returnData.IsJobRunning = _backgroundWorkerQueue.IsRunning;
                returnData.OperationComplete = true;

                return Ok(JsonUtility.SerializeData<ReturnData>(returnData));
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message, true);
                returnData.LogEntries.Add(new LogEntry() { Message = ex.ToString() });
                return BadRequest(JsonUtility.SerializeData<ReturnData>(returnData));
            }
        }
    }
}
