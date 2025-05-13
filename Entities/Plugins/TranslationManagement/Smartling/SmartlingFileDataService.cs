using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{

    public class SmartlingApiFileStatusItem
    {
        public string LocaleId { get; set; }
        public int CompletedStringCount { get; set; }
    }

    public class SmartlingApiFileStatus
    {
        public string FileUri { get; set; }
        public List<SmartlingApiFileStatusItem> Items { get; set; }
    }

    public class SmartlingApiResponseInner<T>
    {
        public string Code { get; set; }
        public T Data { get; set; }
    }

    public class SmartlingApiResponse<T>
    {
        public SmartlingApiResponseInner<T> Response { get; set; }
    }


    public class SmartlingFileDataService : ISmartlingFileDataService
    {
        private readonly ILogger _logger;
        private readonly ISmartlingDataService _fileDataService;
        private readonly IConfiguration _config;
        private readonly ISmartlingConfiguration _smartlingConfiguration;
        private SmartlingAuthResponse _smartlingAuthResponseCached = null;

        public SmartlingFileDataService(ISmartlingDataService fileDataService,
            ILogger<SmartlingFileDataService> logger,
            AppSettings appSettings,
            IConfiguration config)
        {
            _logger = logger;
            _fileDataService = fileDataService;
            _config = config;
            _smartlingConfiguration = new SmartlingConfiguration(_config);
        }

        private async Task<string> GetAuthTokenCached()
        {
            if (_smartlingAuthResponseCached is null)
            {
                var response = await _fileDataService.Authenticate<SmartlingAuthResponse, SmartlingAuthorization>(_smartlingConfiguration.Authorization);
                var inner = response.response;

                if (response is null || inner is null || !string.Equals(inner.code, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Unable to get authorization code from smartling, StatusCode={response.StatusCode}, ReasonPhrase={response.ReasonPhrase}");
                }

                _smartlingAuthResponseCached = response;
            }

            return _smartlingAuthResponseCached.response.data.accessToken;
        }

        private async Task<SmartlingApiResponse<SmartlingApiFileStatus>> GetSmartlingFileStatus(string fileUri)
        {
            var token = await GetAuthTokenCached();
            string endpoint = _smartlingConfiguration.EndPoints.FileStatus + "?fileUri=" + fileUri;
            return await _fileDataService.GetSmartlingObjectDetails<SmartlingApiResponse<SmartlingApiFileStatus>>(endpoint, token);
        }

        private async Task<SmartlingImportJsonFile> GetImportJsonFileAsync(string locale, string fileUri, Guid processId)
        {
            var token = await GetAuthTokenCached();
            string endpoint = _smartlingConfiguration.EndPoints.DownloadFile;
            SmartlingFileData smartlingTranslatedFile = await _fileDataService.GetTranslatedFile(endpoint, token, locale, fileUri);
            return new SmartlingImportJsonFile(smartlingTranslatedFile, fileUri, locale, processId);
        }

        public async Task<SmartlingImportJsonFileCollection> GetTranslatedFileForLocaleAsync(string locale, string fileUri, Guid processId)
        {
            _logger.LogInformation("Getting translated file for {FileUri} and {Locale}", fileUri, locale);
            var importJsonFile = await GetImportJsonFileAsync(locale, fileUri, processId);
            return new SmartlingImportJsonFileCollection(importJsonFile);
        }

        public async Task<SmartlingImportJsonFileCollection> GetTranslatedFilesForAllLocalesAsync(string fileUri, Guid processId)
        {
            var fileStatus = await GetSmartlingFileStatus(fileUri);

            // Detect what smartling locales are have content for this file
            var availableSmartlingLocales = fileStatus.Response.Data.Items
                .Where(item => item.CompletedStringCount > 0)
                .Select(item => item.LocaleId)
                .ToList();

            ConcurrentBag<SmartlingImportJsonFile> results = new();

            await Parallel.ForEachAsync(availableSmartlingLocales, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, async (locale, ct) =>
            {
                results.Add(await GetImportJsonFileAsync(locale, fileUri, processId));
            });

            return new SmartlingImportJsonFileCollection(results);
        }
    }
}
