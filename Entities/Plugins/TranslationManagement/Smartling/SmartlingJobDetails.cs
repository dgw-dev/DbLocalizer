using System.Collections.Generic;

namespace Entities.Plugins.TranslationManagement.Smartling;

public class SmartlingJobDetails
{
    public SmartlingJob Job { get; set; } = new SmartlingJob();
    public SmartlingJobBatch JobBatch { get; set; } = new SmartlingJobBatch();

    public SmartlingJobDetails(string callbackUri)
    {
        Job.callbackMethod = "GET";
        Job.callbackUrl = callbackUri;
        JobBatch.authorize = true;
    }
}

public class SmartlingJob
{
    public string jobName { get; set; }
    public string jobNumber { get; set; }
    public string dueDate { get; set; }
    public string description { get; set; }
    public string callbackUrl { get; set; }
    public string callbackMethod { get; set; }
    public List<string> targetLocaleId { get; set; }

    public SmartlingJob()
    {
        targetLocaleId = new List<string>();
    }
}

public class SmartlingJobBatch
{
    public bool authorize { get; set; }
    public string translationJobUid { get; set; }
    public List<string> fileUris { get; set; }
    public List<SmartlingLocaleId> localeWorkflows { get; set; }
    public SmartlingJobBatch()
    {
        localeWorkflows = new List<SmartlingLocaleId>();
        fileUris = new List<string>();
    }
    public string batchUId { get; set; }
    public string fileType { get; set; }
}

public class SmartlingLocaleId
{
    public SmartlingLocaleId(ISmartlingConfiguration smartlingConfiguration)
    {
        workflowUid = smartlingConfiguration.WorkflowUid;
    }

    public string targetLocaleId { get; set; }
    public string workflowUid { get; private set; } // = GlobalConfig.GetInstance().AppSettings.SmartlingWorkflowUid;
}
