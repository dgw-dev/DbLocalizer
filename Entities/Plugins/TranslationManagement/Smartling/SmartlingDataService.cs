using Entities.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingDataService : ISmartlingDataService
    {
        private readonly IGenericPluginInData _genericRepository;
        private readonly ISmartlingConfiguration _smartlingConfiguration;

        public SmartlingDataService(IGenericPluginInData genericRepository, ISmartlingConfiguration config)
        {
            _genericRepository = genericRepository;
            _smartlingConfiguration = config;
        }

        public async Task<T> Authenticate<T, U>(U data)
        {
            T result = default(T);

            HttpResponseMessage httpResult = null;

            UriBuilder builder = new UriBuilder()
            {
                Path = _smartlingConfiguration.EndPoints.AuthenticateAPI
            };

            httpResult = await _genericRepository.PostAsync<T, U>(builder.Path.ToString(), data);

            if (httpResult != null)
            {
                string responseContent = await httpResult.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseContent))
                {
                    result = JsonConvert.DeserializeObject<T>(responseContent);
                }
            }

            return result;
        }

        public async Task<T> CreateJob<T, U>(string endPoint, string authToken, U jobData)
        {
            T result = default(T);
            HttpResponseMessage httpResult = null;

            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint
            };

            httpResult = await _genericRepository.PostAsync<T, U>(builder.Path.ToString(), jobData, authToken);

            if (httpResult != null)
            {
                string responseContent = await httpResult.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseContent))
                {
                    result = JsonConvert.DeserializeObject<T>(responseContent);
                }
            }

            return result;
        }

        public async Task<T> CreateJobBatch<T, U>(string endPoint, string authToken, U jobBatchData)
        {
            T result = default(T);
            HttpResponseMessage httpResult = null;

            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint
            };

            httpResult = await _genericRepository.PostAsync<T, U>(builder.Path.ToString(), jobBatchData, authToken);

            if (httpResult != null)
            {
                string responseContent = await httpResult.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseContent))
                {
                    result = JsonConvert.DeserializeObject<T>(responseContent);
                }
            }

            return result;
        }

        public async Task<T> GetSmartlingObjectDetails<T>(string endPoint, string authToken)
        {
            return await _genericRepository.GetAsync<T>(endPoint, authToken);
        }


        public async Task<SmartlingFileData> GetTranslatedFile(string endPoint, string authToken, string localeId, string fileUri)
        {
            SmartlingFileData result = null;
            result = await Task.Run(() => _genericRepository.GetAsync<SmartlingFileData>(endPoint + "/" + localeId + "/file" + "?fileUri=" + fileUri, authToken));

            return result;
        }

        public async Task<ResponseErrorEntry> PostFilesAsync(MultipartFormDataContent payload, string endPoint, string batchUid, string authToken, string packageId = default(string))
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint + "/" + batchUid + "/file"
            };

            ResponseErrorEntry result = null;
            HttpResponseMessage httpResult = await _genericRepository.PostFilesAsync<HttpResponseMessage>(builder.Path, payload, authToken, packageId);

            if (httpResult != null && !httpResult.IsSuccessStatusCode)
            {
                result = new ResponseErrorEntry()
                {
                    PackageId = packageId,
                    ReasonPhrase = httpResult.ReasonPhrase,
                    StatusCode = httpResult.StatusCode.ToString()
                };
            }

            return result;
        }
    }
}
