using Entities.DAL;
using Entities.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace Entities.BL
{
    public abstract class ExportBase
    {
        protected ILogger _logger;
        public Guid ProcessId { get; set; }

        protected readonly IExportDal _exportDal;
        public ExportTable ExportTable { get; set; }
        public DataSet Data { get; set; }
        public List<string> BaseTableChunk { get; set; }
        public string ExportLookbackInDays { get; set; }
        public string ExportType { get; set; }

        protected ExportBase(ILogger logger, Guid processId, IExportDal exportDal, string exportType)
        {
            _logger = logger;
            ProcessId = processId;
            _exportDal = exportDal;
            ExportType = exportType;
        }
    }
}
