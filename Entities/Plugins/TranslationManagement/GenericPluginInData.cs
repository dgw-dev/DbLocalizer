using Entities.Interfaces;
using Newtonsoft.Json;
using Polly;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement
{
    public class GenericPluginInData : IGenericPluginInData
    {
        public async Task<T> GetItemAsync<T>(string id, string uri, string authToken = default(string))
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);
                string jsonResult = string.Empty;

                var responseMessage = await Policy
                    .Handle<WebException>(ex =>
                    {
                        Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                        return true;
                    })
                    .WaitAndRetryAsync
                    (
                        5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    )
                    .ExecuteAsync(async () => await httpClient.GetAsync(uri));

                if (responseMessage.IsSuccessStatusCode)
                {
                    jsonResult =
                        await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                return default(T);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<T> GetAsync<T>(string uri, string authToken = default(string))
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);
                using (httpClient)
                {
                    string jsonResult = string.Empty;

                    var responseMessage = await Policy
                        .Handle<WebException>(ex =>
                        {
                            Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                            return true;
                        })
                        .WaitAndRetryAsync
                        (
                            5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await httpClient.GetAsync(uri));

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var json = JsonConvert.DeserializeObject<T>(jsonResult);
                        return json;
                    }

                    return default(T);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<bool> PostAsyncBool<T>(string uri, T data)
        {
            try
            {
                HttpClient httpClient = CreateHttpClient();
                using (httpClient)
                {
                    var jsonObj = JsonConvert.SerializeObject(data);

                    var content = new StringContent(jsonObj);
                    content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");

                    var responseMessage = await Policy
                        .Handle<WebException>(ex =>
                        {
                            Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                            return true;
                        })
                        .WaitAndRetryAsync
                        (
                            5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));


                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<T> PostFilesAsync<T>(string uri, MultipartFormDataContent content, string authToken = default(string), string packageId = default(string))
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);
                using (httpClient)
                {
                    var responseMessage = await Policy
                        .Handle<WebException>(ex =>
                        {
                            Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                            return true;
                        })
                        .WaitAndRetryAsync
                        (
                            5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));

                    return (T)(object)responseMessage;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<bool> PutAsync<T>(string uri, T data, string authToken = default(string))
        {
            try
            {
                HttpClient httpClient = CreateHttpClient(authToken);
                using (httpClient)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(data));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var responseMessage = await Policy
                        .Handle<WebException>(ex =>
                        {
                            Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                            return true;
                        })
                        .WaitAndRetryAsync
                        (
                            5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await httpClient.PutAsync(uri, content));

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string uri, string authToken = default(string))
        {
            HttpClient httpClient = CreateHttpClient(authToken);
            using (httpClient)
            {
                await Task.Run(() => (httpClient.DeleteAsync(uri)));
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string uri, T data, string authToken = default(string))
        {
            HttpResponseMessage httpResult = null;
            HttpClient httpClient = null;
            try
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    httpClient = CreateHttpClient();
                }
                else
                {
                    httpClient = CreateHttpClient(authToken);
                }

                using (httpClient)
                {
                    var jsonObj = JsonConvert.SerializeObject(data);

                    var content = new StringContent(jsonObj);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var responseMessage = await Policy
                        .Handle<WebException>(ex =>
                        {
                            Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                            return true;
                        })
                        .WaitAndRetryAsync
                        (
                            5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));

                    httpResult = (HttpResponseMessage)(object)responseMessage;

                    return httpResult;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T, U>(string uri, U data, string authToken = default(string))
        {
            HttpResponseMessage httpResult = null;
            HttpClient httpClient = null;
            try
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    httpClient = CreateHttpClient();
                }
                else
                {
                    httpClient = CreateHttpClient(authToken);
                }

                using (httpClient)
                {
                    var jsonObj = JsonConvert.SerializeObject(data);

                    var content = new StringContent(jsonObj);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var responseMessage = await Policy
                        .Handle<WebException>(ex =>
                        {
                            Debug.WriteLine($"{ex.GetType().Name + " : " + ex.Message}");
                            return true;
                        })
                        .WaitAndRetryAsync
                        (
                            5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        )
                        .ExecuteAsync(async () => await httpClient.PostAsync(uri, content));

                    httpResult = (HttpResponseMessage)(object)responseMessage;

                    return httpResult;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        private static HttpClient CreateHttpClient(string authToken = default(string))
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }
            return httpClient;
        }

        public Task<HttpResponseMessage> GetFileAsync<HttpResponseMessage>(string uri, string authToken)
        {
            throw new NotImplementedException();
        }
    }
}
