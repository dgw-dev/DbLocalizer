using System;

namespace Entities.BL
{
    public class LogEntry
    {
        public int LogTableId { get; set; }
        public string Message { get; set; }
        public Guid ProcessId { get; set; }
        public Guid PackageId { get; set; }
        public DateTime LastModifiedTimestamp { get; set; }
        public string LogLevel { get; set; }
        public bool CancelOperation { get; set; }

        public LogEntry() { }
        public LogEntry(int logTableId, string message, Guid processId, Guid packageId, DateTime lastModifiedTimestamp, string logLevel, bool cancelOperation = false)
        {
            LogTableId = logTableId;
            Message = message;
            ProcessId = processId;
            PackageId = packageId;
            LastModifiedTimestamp = lastModifiedTimestamp;
            LogLevel = logLevel;
            CancelOperation = cancelOperation;
        }
    }
}
