using Entities.DAL;
using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace Entities.BL
{
    public class DefaultExport : ExportBase, IExportData
    {
        public DefaultExport(
            ILogger exportLogger,
            Guid processId,
            IExportDal exportDal,
            string exportType) : base(exportLogger, processId, exportDal, exportType){}
    }
}
