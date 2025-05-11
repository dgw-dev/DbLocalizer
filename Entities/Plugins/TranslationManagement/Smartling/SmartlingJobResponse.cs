using System.Collections.Generic;
using System.Net;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingJobResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public jobResponse response { get; set; }
    }
    public class jobResponse
    {
        public string code { get; set; }
        public jobResponseData data { get; set; }
    }

    public class jobResponseData
    {
        public string callbackMethod { get; set; }
        public string callbackUrl { get; set; }
        public string createdByUserId { get; set; }
        public string createdDate { get; set; }
        public string description { get; set; }
        public string dueDate { get; set; }
        public string firstCompletedDate { get; set; }
        public string firstAuthorizedDate { get; set; }
        public string jobName { get; set; }
        public string jobNumber { get; set; }
        public string jobStatus { get; set; }
        public string lastCompletedDate { get; set; }
        public string lastAuthorizedDate { get; set; }
        public string modifiedByUserUid { get; set; }
        public string modifiedDate { get; set; }
        public List<string> targetLocaleIds { get; set; }
        public List<jobResponseCustomFields> customFields { get; set; }
        public string translationJobUid { get; set; }
        public string referenceNumber { get; set; }
        public jobResponseIssues issues { get; set; }

    }
    public class jobResponseCustomFields
    {
        public string fieldUid { get; set; }
        public string fieldName { get; set; }
        public string fieldValue { get; set; }
    }

    public class jobResponseIssues
    {
        public int sourceIssuesCount { get; set; }
        public int transaltionIssuesCount { get; set; }
    }
}
