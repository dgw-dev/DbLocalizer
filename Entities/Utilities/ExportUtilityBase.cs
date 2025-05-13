using Entities.DAL;
using Entities.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Entities.Utilities
{
    public class ExportUtilityBase
    {
        protected ILogger _logger = null;
        protected readonly IConfiguration _config;
        protected readonly IExportDal _exportDal;
        protected int ExportMaxTablesPerFile { get; set; }
        protected int MaxRowsPerFile { get; set; }
        protected decimal MaxRequestSize { get; set; }
        protected bool IsDevelopmentEnvironment { get; set; }
        public Guid ProcessId { get; set; }
        public MultipartFormDataContent OutputContent { get; set; }
        protected Cultures Cultures { get; set; }
        protected string ExportLookbackInDays { get; set; }
        protected bool OperationComplete { get; set; } = false;
        protected ICacheManager CacheManager { get; set; }


        public ExportUtilityBase(
            IFileDataService fileDataService,
            IExportDal exportDal,
            AppSettings appSettings,
            IConfiguration config,
            ILogger logger = null,
            Guid processId = default(Guid))
        {
            _logger = logger;
            ExportMaxTablesPerFile = Convert.ToInt32(appSettings.ExportMaxTablesPerFile);
            MaxRowsPerFile = Convert.ToInt32(appSettings.MaxRowsPerFile);
            ExportLookbackInDays = appSettings.ExportLookbackInDays;
            MaxRequestSize = Convert.ToDecimal(appSettings.MaximumRequestSize) / 1024 / 1024;
            ProcessId = processId;
            _exportDal = exportDal;
            _config = config;
            Cultures = new Cultures(_config);
        }

        public virtual async Task<string> Export(CancellationToken ct = default)
        {
            if (IsCancelled(ct))
            {
                return default;
            }

            await Task.Factory.StartNew(async () =>
            {
                Databases databases = CacheManager.GetCacheValue("Databases") as Databases;
                foreach (Database db in databases.DatabaseCollection)
                {
                    if (db.CanConnect)
                    {
                        await ProcessExport(ct, null, db);
                    }
                }

            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return string.Empty;
        }

        protected virtual async Task ProcessExport(CancellationToken ct, List<string> culturesOverride = null, Database db = null)
        {
            if (IsCancelled(ct))
            {
                return;
            }
            await Task.CompletedTask;
        }

        protected bool IsCancelled(CancellationToken ct)
        {
            if (ct.IsCancellationRequested || !TokenStore.Store.TryGetValue(ProcessId, out CancellationTokenSource _))
            {
                return true;
            }

            return false;
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

        protected void Abort()
        {
            if (TokenStore.Store.TryGetValue(ProcessId, out CancellationTokenSource source))
            {
                source.Cancel();
                TokenStore.Store.Remove(ProcessId);
            }
        }
    }
}
