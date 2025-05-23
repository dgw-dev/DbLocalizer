using Entities.Configuration;
using Entities.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingPlugin : TmsPluginBase, ITranslationManager
    {
        private readonly SmartlingJobDetails _jobDetails;
        private readonly ISmartlingConfiguration _smartlingConfig;
        private readonly ILogger _logger;

        private readonly IExportFileProcessor _fileProcessor;
        private readonly ISmartlingDataService _fileDataService;

        public SmartlingPlugin(IExportFileProcessor fileProcessor,
            ISmartlingConfiguration smartlingConfig,
            ILogger logger,
            ICacheManager cacheManager) : base(cacheManager)
        {
            _fileProcessor = fileProcessor;
            _smartlingConfig = smartlingConfig;
            _jobDetails = new SmartlingJobDetails(_smartlingConfig.DbLocalizerImportEndPoint);
            _logger = logger;

            IGenericPluginInData genericRepository = new GenericPluginInData();
            _fileDataService = new SmartlingDataService(genericRepository, _smartlingConfig);
        }

        public async Task<bool> TMSOperations<T>(Dictionary<string, T> dtmPackage, MetaData globalMetaData, string packageId, CancellationToken ct, List<string> culturesOverride, Cultures cultures, Guid processId)
        {
            bool result = false;

            if (IsCancelled(ct, processId))
            {
                return false;
            }

            try
            {
                int culturePackageCount = 1;
                int jobCount = 0;
                foreach (KeyValuePair<string, T> exportFile in dtmPackage)
                {
                    SmartlingExportFile smartlingExportFile = exportFile.Value as SmartlingExportFile;

                    if (culturesOverride != null && culturesOverride.Count > 0)
                    {
                        for (int i = 0; i < culturesOverride.Count; i++)
                        {
                            //get this supported culture from string
                            Culture culture = cultures.CultureCollection.FirstOrDefault(s => s.CultureCode == culturesOverride[i]);

                            if (culture != null)
                            {
                                //add smartling locale info for job
                                PopulateJobDetails(culture);
                            }
                        }
                    }
                    else if (cultures.CultureCollection != null && cultures.CultureCollection.Count > 0)
                    {
                        //get all supported cultures
                        foreach (Culture culture in cultures.CultureCollection)
                        {
                            //add smartling locale info for job
                            PopulateJobDetails(culture);
                        }
                    }

                    _jobDetails.Job.jobName = "DTM_" + _fileProcessor.ExportType + "_" + smartlingExportFile.MetaData.Locale + "_" + DateTime.Now.ToString("yyyy-MM-dd h:mm tt");
                    _jobDetails.Job.description = "Package Id: " + packageId;
                    _jobDetails.JobBatch.authorize = _smartlingConfig.AutoAuthorize;

                    //get http content
                    var outputContent = GetCultureContentPackage(smartlingExportFile, _jobDetails.JobBatch.localeWorkflows, _smartlingConfig.DbLocalizerImportEndPoint);

                    ////do we have output content
                    if (outputContent != null && await IsOutputContentValid(outputContent))
                    {
                        //authenticate smartling connection
                        SmartlingAuthResponse smartlingAuthResponse = await _fileDataService.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(_smartlingConfig.Authorization);
                        if (smartlingAuthResponse?.response != null && string.Equals(smartlingAuthResponse.response.code, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                        {
                            //create job
                            SmartlingJobResponse smartlingJobResponse = await _fileDataService.CreateJob<SmartlingJobResponse, SmartlingJob>(_smartlingConfig.EndPoints.JobAPI, smartlingAuthResponse.response.data.accessToken, _jobDetails.Job);

                            if (smartlingJobResponse?.response != null && string.Equals(smartlingJobResponse.response.code, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                            {
                                _jobDetails.JobBatch.translationJobUid = smartlingJobResponse.response.data.translationJobUid;
                                _jobDetails.JobBatch.fileUris.Add(smartlingExportFile.FileName);
                                _jobDetails.JobBatch.fileType = "json";

                                //create jobbatch
                                SmartlingJobBatchResponse smartlingJobBatchResponse = await _fileDataService.CreateJobBatch<SmartlingJobBatchResponse, SmartlingJobBatch>(_smartlingConfig.EndPoints.JobBatchAPI, smartlingAuthResponse.response.data.accessToken, _jobDetails.JobBatch);
                                if (smartlingJobBatchResponse?.response != null && string.Equals(smartlingJobBatchResponse.response.code, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                                {
                                    _jobDetails.JobBatch.batchUId = smartlingJobBatchResponse.response.data.batchUid;
                                    _jobDetails.Job.jobNumber = smartlingJobResponse.response.data.jobNumber;
                                    _jobDetails.Job.dueDate = smartlingJobResponse.response.data.dueDate;
                                    //post the data to Smartling
                                    result = await PostDataToTMS(outputContent, globalMetaData, packageId, smartlingAuthResponse.response.data.accessToken, ct);
                                }
                                else
                                {
                                    string message = "Could not create job batch in Smartling. - StatusCode: " + smartlingJobBatchResponse.response.code + " ReasonPhrase: " + smartlingJobBatchResponse.ReasonPhrase;
                                    _logger.LogError(message);
                                }
                            }
                            else
                            {
                                string message = "Could not create job in Smartling. - StatusCode: " + smartlingJobResponse.response.code + " ReasonPhrase: " + smartlingJobResponse.ReasonPhrase;
                                _logger.LogError(message);
                            }
                        }
                        else
                        {
                            string message = "Could not authenticate Smartling. - StatusCode: " + smartlingAuthResponse?.response?.code ?? "NONE";
                            _logger.LogError(message);
                        }
                    }

                    culturePackageCount++;
                    jobCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }

            return result;
        }

        protected MultipartFormDataContent GetCultureContentPackage(SmartlingExportFile culturePackage, List<SmartlingLocaleId> localeIds, string callbackUrl = default(string))
        {
            MultipartFormDataContent content = null;
            content = new MultipartFormDataContent();
            content.Headers.ContentType.MediaType = "multipart/form-data";

            AddFileContent(content, culturePackage.FileName, culturePackage.JsonOutput, localeIds, callbackUrl);
            return content;
        }

        protected void AddFileContent(MultipartFormDataContent content, string fileName, string sqlFileContent, List<SmartlingLocaleId> localeIds, string callBackUrl)
        {
            //add to outputcontent
            StreamContent streamContent = CreateFileContent(sqlFileContent, fileName, "application/json", new UTF8Encoding(false));
            content.Add(streamContent, "file");
            content.Add(new StringContent(fileName ?? string.Empty), "fileUri");
            content.Add(new StringContent("json" ?? string.Empty), "fileType");
            foreach (SmartlingLocaleId locale in localeIds)
            {
                content.Add(new StringContent(locale.targetLocaleId ?? string.Empty), "localeIdsToAuthorize[]");
            }
            content.Add(new StringContent(callBackUrl ?? string.Empty), "callbackUrl");
        }

        public async Task<bool> PostDataToTMS(MultipartFormDataContent outputContent, MetaData globalMetaData, string packageId, string authToken, CancellationToken ct)
        {
            ResponseErrorEntry error = await _fileDataService.PostFilesAsync(outputContent, _smartlingConfig.EndPoints.JobBatchAPI, _jobDetails.JobBatch.batchUId, authToken, packageId);
            if (error != null && !string.IsNullOrEmpty(error.StatusCode))
            {
                string message = error.StatusCode + ": " + error.ReasonPhrase;
                _logger.LogError(message);
                return false;
            }
            return true;
        }

        private void PopulateJobDetails(Culture culture)
        {
            //add smartling locale info for job
            SmartlingLocaleId localeId = new SmartlingLocaleId(_smartlingConfig);
            localeId.targetLocaleId = culture.SmartlingCultureCode;
            _jobDetails.Job.targetLocaleId.Add(localeId.targetLocaleId);
            _jobDetails.JobBatch.localeWorkflows.Add(localeId);
        }
    }
}
