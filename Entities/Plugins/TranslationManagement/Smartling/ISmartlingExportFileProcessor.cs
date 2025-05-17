using Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingExportFileProcessor
    {
        Guid ProcessId { get; set; }
        List<ExportErrorEntry> ErrorList { get; set; }
        string ExportType { get; set; }
        ICacheManager CacheManager { get; set; }
        Dictionary<string, List<DataSet>> DatabaseUpdatesForApp { get; set; }
        T ProcessExportData<T>(string exportLookbackInDays, int exportMaxTablesPerFile, int maxRowsPerTable, CancellationToken ct, Database db);
        MultipartFormDataContent GetCultureContent(SmartlingExportFile culturePackage, List<SmartlingLocaleId> localeIds, string callbackUrl = default(string));
    }
}
