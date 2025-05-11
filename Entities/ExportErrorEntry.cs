using System;

namespace Entities
{
    public class ExportErrorEntry
    {
        public Guid ProcessId { get; set; }
        public MetaData MetaData { get; set; }
        public string Message { get; set; }
        public string Stacktrace { get; set; }
        public ErrorType ErrorType { get; set; }
        public string PackageId { get; set; }
        public string SourceType { get; set; }
        public string ConnectorType { get; set; }
        public string ResourceType { get; set; }
        public string FTPPath { get; set; }

        public ExportErrorEntry()
        {
        }

        public ExportErrorEntry(Guid processId = default, MetaData metaData = null, string message = default(string), string stacktrace = default(string), ErrorType errorType = ErrorType.Export, string ftpPath = default(string))
        {
            MetaData = metaData;
            ProcessId = processId;
            FTPPath = ftpPath;
            Message = message;
            Stacktrace = stacktrace;
            ErrorType = errorType;
        }
    }
}
