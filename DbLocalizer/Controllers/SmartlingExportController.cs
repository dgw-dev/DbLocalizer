using Entities;
using Entities.BL;
using Entities.DAL;
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
using System.Threading;
using System.Threading.Tasks;

namespace DbLocalizer.Controllers
{
    [EnableCors("DbLocalizerCorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class SmartlingExportController : BaseController
    {
        private readonly IBackgroundWorkerQueue _backgroundWorkerQueue;
        private readonly AppSettings _appSettings;
        private readonly ILongRunningService _longRunningService;
        private readonly IExportDal _exportDal;
        private readonly IFileDataService _fileDataService;

        public SmartlingExportController(
            IBackgroundWorkerQueue backgroundWorkerQueue,
            ILongRunningService longRunningService,
            AppSettings appSettings,
            ILogger<SmartlingExportController> logger,
            IExportDal exportDal,
            IFileDataService fileDataService,
            IMemoryCache memoryCache,
            IConfiguration config,
            ISqlSchemaBuilder sqlSchemaBuilder)
            : base(memoryCache, config, sqlSchemaBuilder, logger)
        {
            _backgroundWorkerQueue = backgroundWorkerQueue;
            _appSettings = appSettings;
            _longRunningService = longRunningService;
            _exportDal = exportDal;
            _fileDataService = fileDataService;
        }

        /// <summary>
        /// End Point to start the export process
        /// </summary>
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
        ///}
        /// </para>
        /// </remarks>
        /// <response code="200">Returns a Json string</response>
        [Authorize]
        [HttpPost("Export")]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public IActionResult Export()
        {
            if (CacheManager.GetCacheValue("Databases") == null || CacheManager.GetCacheValue("Cultures") == null)
            {
                return BadRequest("The required database and/or culture data is null");
            }

            Guid processId = Guid.NewGuid();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            _longRunningService.Queue.QueueBackgroundWorkItem(token => StartWork(processId, tokenSource.Token));

            return Ok(GetReturnValue(processId));
        }

        private async Task<string> StartWork(Guid processId, CancellationToken ct)
        {
            CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);

            if (!TokenStore.Store.TryGetValue(processId, out CancellationTokenSource _))
            {
                TokenStore.Store.Add(processId, tokenSource);
            }

            string result = string.Empty;

            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    ISmartlingConfiguration smartlingConfig = new SmartlingConfiguration(_config);
                    ISmartlingExportFileProcessor _fileProcessor = new SmartlingExportFileProcessor(_logger, _exportDal, "Default", CacheManager, _schemaBuilder, smartlingConfig);
                    IExportUtility util = new SmartlingExportUtility(
                        _fileProcessor,
                        _fileDataService,
                        _exportDal,
                        _appSettings,
                        _config,
                        smartlingConfig,
                        _logger,
                        processId);

                    result = await util.Export(ct);
                }
                catch (Exception ex)
                {
                    LogMessage(ex.Message, true);
                }
            }, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return result;
        }

        /// <summary>
        /// End Point to cancel the export process
        /// </summary>
        /// <remarks>
        /// <para>Returns a Json string</para>
        /// <para>Example</para>
        /// <para>
        /// {
        ///"OperationComplete": false,
        ///"LogEntries": [],
        ///"ImportErrors": null,
        ///"IsJobRunning": false,
        ///"ProcessId": "07b6de42-5514-445b-931a-06cd4fded060",
        /// "IsCancelled": true,
        ///}
        /// </para>
        /// </remarks>
        /// <response code="200">Returns a Json string</response>
        [Authorize]
        [HttpDelete]
        [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
        [ProducesResponseType<IActionResult>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelExport(string id)
        {
            string result = string.Empty;
            string returnValue = string.Empty;

            ReturnData returnData = null;

            if (string.IsNullOrEmpty(id))
            {
                returnData = new ReturnData()
                {
                    LogEntries = null,
                    IsJobRunning = false,
                    ProcessId = Guid.Empty,
                };

                returnValue = JsonUtility.SerializeData<ReturnData>(returnData);

                return BadRequest(returnValue);
            }

            Guid processId = new Guid(id);

            if (TokenStore.Store.TryGetValue(new Guid(id), out CancellationTokenSource source))
            {
                await source.CancelAsync();

                try
                {
                    result = await _longRunningService.CancelAsync(source.Token);
                }
                catch (Exception ex)
                {
                    LogMessage(ex.Message, true);
                }
                finally
                {
                    TokenStore.Store.Remove(processId);
                }
            }

            List<LogEntry> logEntries = new List<LogEntry>();
            if (!string.IsNullOrEmpty(result))
            {
                logEntries.Add(new LogEntry() { LogLevel = "Information", Message = "Cancelled: " + result, ProcessId = processId });
            }

            returnData = new ReturnData()
            {
                LogEntries = logEntries,
                IsJobRunning = false,
                ProcessId = processId,
                OperationComplete = true,
                IsCancelled = true,
            };

            returnValue = JsonUtility.SerializeData<ReturnData>(returnData);

            return Ok(returnValue);
        }

        private string GetReturnValue(Guid processId)
        {
            ReturnData returnData = null;

            List<LogEntry> logEntries = new List<LogEntry>();
            logEntries.Add(new LogEntry() { LogLevel = "Information", Message = "Export process started", ProcessId = processId });

            returnData = new ReturnData()
            {
                LogEntries = logEntries,
                IsJobRunning = _backgroundWorkerQueue.IsRunning,
                ProcessId = processId,
            };

            return JsonUtility.SerializeData<ReturnData>(returnData);
        }
    }
}
