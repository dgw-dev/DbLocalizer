using System.Collections.Generic;
namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingJobBatchQuery
    {
        public SmartlingJobBatchQueryResponse response { get; set; }
    }

    public class SmartlingJobBatchQueryResponse
    {
        public string code { get; set; }
        public SmartlingJobBatchQueryData data { get; set; }
    }

    public class SmartlingJobBatchQueryData
    {
        public string authorized { get; set; }
        public List<SmartlingJobBatchQueryFiles> files { get; set; }
        public string generalErrors { get; set; }
        public string projectId { get; set; }
        public string status { get; set; }
        public string translationJobUid { get; set; }
        public string updatedDate { get; set; }

    }

    public class SmartlingJobBatchQueryFiles
    {
        public string errors { get; set; }
        public string fileUri { get; set; }
        public string status { get; set; }
        public List<SmartlingJobBatchQueryFileLocales> targetLocales { get; set; }
        public string updatedDate { get; set; }
    }

    public class SmartlingJobBatchQueryFileLocales
    {
        public string localeId { get; set; }
        public int stringsAdded { get; set; }
    }
}
