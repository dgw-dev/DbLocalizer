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
            //important, new instance otherwise we end up concatenating the uri over an over
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
        //private static SmartlingConfiguration _instance = null;
        //private static readonly object oLock = new object();

        //private string _baseUri;
        //public required string workflowUid { get; private set; }
        //public required SmartlingEndPoints EndPoints { get; set; }
        //public required SmartlingAuthorization Authorization { get; set; }
        //private static SmartlingConfiguration Initialize(AppSettings appSettings)
        //{
        //    SmartlingConfiguration config = new SmartlingConfiguration();
        //    //important, new instance otherwise we end up concatenating the uri over an over
        //    config.EndPoints = new SmartlingEndPoints();
        //    SmartlingEndPoints endPointPaths = appSettings.SmartlingEndPoints;
        //    config.Authorization = appSettings.SmartlingAuthorization;
        //    config.workflowUid = appSettings.SmartlingWorkflowUid;
        //    config._baseUri = appSettings.TranslationServiceBaseUri;

        //    if (!string.IsNullOrEmpty(config._baseUri) && config.EndPoints != null)
        //    {
        //        config.EndPoints.ExportAPI = config._baseUri + endPointPaths.ExportAPI;
        //        config.EndPoints.ImportAPI = config._baseUri + endPointPaths.ImportAPI;
        //        config.EndPoints.JobAPI = config._baseUri + endPointPaths.JobAPI;
        //        config.EndPoints.JobBatchAPI = config._baseUri + endPointPaths.JobBatchAPI;
        //        config.EndPoints.AuthenticateAPI = config._baseUri + endPointPaths.AuthenticateAPI;
        //        config.EndPoints.DownloadFile = config._baseUri + endPointPaths.DownloadFile;
        //        config.EndPoints.FileStatus = config._baseUri + endPointPaths.FileStatus;
        //    }
        //    else
        //    {
        //        throw new Exception("TranslationServiceBaseUri is not set in appsettings.json");
        //    }

        //    return config;
        //}

        //public static SmartlingConfiguration InitializeInstance(AppSettings appSettings)
        //{
        //    lock (oLock)
        //    {
        //        if (_instance == null)
        //        {
        //            _instance = Initialize(appSettings);
        //        }
        //        return _instance;
        //    }
        //}

        //public static SmartlingConfiguration GetInstance()
        //{
        //    return _instance;
        //}

        //public SmartlingConfiguration Get()
        //{
        //    return _instance;
        //}
    }
}
