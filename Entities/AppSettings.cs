namespace Entities
{
    public class AppSettings
    {
        public string ExportEndPoint { get; set; }
        public string ImportEndPoint { get; set; }
        public string UseScheduler { get; set; }
        public string ScheduledExportCron { get; set; }
        public string ScheduledImportPurgeCron { get; set; }
        public string ImportMaxTablesPerFile { get; set; }
        public string ExportMaxTablesPerFile { get; set; }
        public string MaxRowsPerFile { get; set; }
        public string ExportLookbackInDays { get; set; }
        public string MaximumRequestSize { get; set; }
        public string DataToPurgeInDays { get; set; }
        public Cors Cors { get; set; }
    }

    public class Cors
    {
        public string[] CorsOrigins { get; set; }
    }
}
