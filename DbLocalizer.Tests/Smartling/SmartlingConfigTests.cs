using Entities.Plugins.TranslationManagement.Smartling;
using NUnit.Framework;

namespace DbLocalizer.Tests.Smartling
{
    public class SmartlingConfigTests : TestBase
    {
        [Test, Order(1)]
        public void Smartling_Config_Populated_Successfully()
        {
            ISmartlingConfiguration smartlingConfiguration = new SmartlingConfiguration(_configuration);
            Assert.IsNotNull(smartlingConfiguration);
            Assert.IsNotNull(smartlingConfiguration.EndPoints);
            Assert.IsNotNull(smartlingConfiguration.EndPoints.ServiceBaseUri);
            Assert.AreEqual(smartlingConfiguration.EndPoints.ExportAPI, smartlingConfiguration.EndPoints.ServiceBaseUri + "/files-api/v2/projects/<PROJECTID>/file");
            Assert.AreEqual(smartlingConfiguration.EndPoints.ImportAPI, smartlingConfiguration.EndPoints.ServiceBaseUri + "/v1/file/get");
            Assert.AreEqual(smartlingConfiguration.EndPoints.JobAPI, smartlingConfiguration.EndPoints.ServiceBaseUri + "/jobs-api/v3/projects/<PROJECTID>/jobs");
            Assert.AreEqual(smartlingConfiguration.EndPoints.JobBatchAPI, smartlingConfiguration.EndPoints.ServiceBaseUri + "/job-batches-api/v2/projects/<PROJECTID>/batches");
            Assert.AreEqual(smartlingConfiguration.EndPoints.AuthenticateAPI, smartlingConfiguration.EndPoints.ServiceBaseUri + "/auth-api/v2/authenticate");
            Assert.AreEqual(smartlingConfiguration.EndPoints.DownloadFile, smartlingConfiguration.EndPoints.ServiceBaseUri + "/files-api/v2/projects/<PROJECTID>/locales");
            Assert.AreEqual(smartlingConfiguration.Authorization.userIdentifier, "<USERID>");
            Assert.AreEqual(smartlingConfiguration.Authorization.userSecret, "<USERSECRET>");
            Assert.AreEqual(smartlingConfiguration.WorkflowUid, "<WORKFLOWID>");
            Assert.AreEqual(bool.Parse(smartlingConfiguration.smartling.entity_escaping), true);
            Assert.AreEqual(bool.Parse(smartlingConfiguration.smartling.variants_enabled), true);
            Assert.AreEqual(smartlingConfiguration.smartling.translate_paths[0].path, "*/Source/Text");
            Assert.Greater(smartlingConfiguration.smartling.translate_paths.Count, 0);
            Assert.AreEqual(smartlingConfiguration.smartling.translate_paths[0].character_limit, "*/Source/MaxLength");
        }

        [Test, Order(3)]
        public void Smartling_JobDetails()
        {
            ISmartlingConfiguration smartlingConfiguration = new SmartlingConfiguration(_configuration);
            SmartlingJobDetails tmsJobDetails = new SmartlingJobDetails(smartlingConfiguration.DbLocalizerImportEndPoint);

            Assert.IsNotNull(tmsJobDetails);
            Assert.IsNotNull(tmsJobDetails.Job);
            Assert.AreEqual(tmsJobDetails.Job.callbackMethod, "GET");
            Assert.AreEqual(tmsJobDetails.Job.callbackUrl, "https://localhost:44310/api/SmartlingImport");
            Assert.IsNotNull(tmsJobDetails.JobBatch);
            Assert.AreEqual(tmsJobDetails.JobBatch.authorize, true);
        }
    }
}
