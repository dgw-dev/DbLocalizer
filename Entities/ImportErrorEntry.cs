using System;

namespace Entities
{
    public class ImportErrorEntry
    {
        public string Message { get; set; }
        public Guid ProcessId { get; set; }
        public Guid PackageId { get; set; }
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string Stacktrace { get; set; }
        public ErrorType ErrorType { get; set; }

        public ImportErrorEntry(string message = default(string), Guid processId = default(Guid), Guid packageId = default(Guid), Guid fileId = default(Guid), string fileName = default(string), string stackTrace = default(string), ErrorType errorType = ErrorType.Import)
        {
            Message = message;
            ProcessId = processId;
            PackageId = packageId;
            FileId = fileId;
            FileName = fileName;
            Stacktrace = stackTrace;
            ErrorType = errorType;
        }
    }
}
