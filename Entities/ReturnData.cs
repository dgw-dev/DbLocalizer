using Entities.BL;
using System;
using System.Collections.Generic;

namespace Entities
{
    public class ReturnData<T> where T : class
    {
        public T Data { get; set; }
        public List<ResponseErrorEntry> ResponseErrors { get; set; } = new List<ResponseErrorEntry>();
    }

    public class ReturnData
    {
        public bool OperationComplete { get; set; }
        public int NumberOfProcessErrors { get; set; }
        public List<LogEntry> LogEntries { get; set; } = new List<LogEntry>();
        public List<ImportErrorEntry> ImportErrors { get; set; } = new List<ImportErrorEntry>();
        public bool IsJobRunning { get; set; }
        public Guid ProcessId { get; set; }
        public bool IsCancelled { get; set; }
    }
}
