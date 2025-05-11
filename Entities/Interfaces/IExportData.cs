using Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Data;

namespace Entities.Interfaces
{
    public interface IExportData
    {
        Guid ProcessId { get; set; }
        ExportTable ExportTable { get; set; }
        DataSet Data { get; set; }
        List<string> BaseTableChunk { get; set; }
        string ExportLookbackInDays { get; set; }
        string ExportType { get; set; }
    }
}
