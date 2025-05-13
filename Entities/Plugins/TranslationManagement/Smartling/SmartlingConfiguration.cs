using Microsoft.Extensions.Configuration;
using System;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public sealed class SmartlingConfiguration : ISmartlingConfiguration
    {
        public SmartlingEndPoints EndPoints { get; set; }
        public SmartlingAuthorization Authorization { get; set; }
        public string WorkflowUid { get; set; }
        public bool AutoAuthorize { get; set; } = false;
        public Smartling smartling { get; set; }
        public string DbLocalizerExportEndPoint { get; set; }
        public string DbLocalizerImportEndPoint { get; set; }

        public SmartlingConfiguration(IConfiguration config)
        {
            var smartlingSettings = config.Get<SmartlingSettings>();
            EndPoints = smartlingSettings.SmartlingEndPoints;
            SmartlingEndPoints endPointPaths = smartlingSettings.SmartlingEndPoints;
            Authorization = smartlingSettings.SmartlingAuthorization;
            AutoAuthorize = Authorization.AutoAuthorize;
            WorkflowUid = smartlingSettings.SmartlingWorkflowUid;
            smartling = smartlingSettings.smartling;
            DbLocalizerExportEndPoint = smartlingSettings.DbLocalizerExportEndPoint;
            DbLocalizerImportEndPoint = smartlingSettings.DbLocalizerImportEndPoint;

            string _baseUri = endPointPaths.ServiceBaseUri;

            if (!string.IsNullOrEmpty(_baseUri) && EndPoints != null)
            {
                EndPoints.ExportAPI = _baseUri + endPointPaths.ExportAPI;
                EndPoints.ImportAPI = _baseUri + endPointPaths.ImportAPI;
                EndPoints.JobAPI = _baseUri + endPointPaths.JobAPI;
                EndPoints.JobBatchAPI = _baseUri + endPointPaths.JobBatchAPI;
                EndPoints.AuthenticateAPI = _baseUri + endPointPaths.AuthenticateAPI;
                EndPoints.DownloadFile = _baseUri + endPointPaths.DownloadFile;
                EndPoints.FileStatus = _baseUri + endPointPaths.FileStatus;
            }
            else
            {
                throw new ArgumentException("ServiceBaseUri is not set in appsettings.json");
            }
        }
        
    }
}
