using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Entities.Interfaces
{
    public interface IExportFileProcessor
    {
        Guid ProcessId { get; set; }
        List<ExportErrorEntry> ErrorList { get; set; }
        string ExportType { get; set; }
        ICacheManager CacheManager { get; set; }
        Dictionary<string, List<DataSet>> DatabaseUpdatesForApp { get; set; }
        T ProcessExportData<T>(string exportLookbackInDays, int exportMaxTablesPerFile, int maxRowsPerTable, CancellationToken ct, Database db);
    }
}
