using System.Net;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingJobBatchResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public jobBatchResponse response { get; set; }
    }
    public class jobBatchResponse
    {
        public string code { get; set; }
        public jobBatchResponseData data { get; set; }

    }
    public class jobBatchResponseData
    {
        public string batchUid { get; set; }
    }
}