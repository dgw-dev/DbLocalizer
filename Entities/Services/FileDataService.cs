using Entities.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class FileDataService : IFileDataService
    {
        private readonly IGenericRepository _genericRepository;

        public bool TestMode { get; set; } = false;

        public FileDataService(IGenericRepository genericRepository, AppSettings appSettings)
        {
            _genericRepository = genericRepository;

        }

        public async Task<IFormFile> GetSelectedFileAsync(string fileName, string endPoint, string authToken)
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint + "/" + fileName
            };

            return await Task.Run(() => _genericRepository.GetAsync<IFormFile>(builder.ToString(), authToken));
        }

        public async Task<List<IFormFile>> GetTeamFilesAsync(string teamName, string endPoint, string authToken)
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint + "/" + teamName
            };

            return await Task.Run(() => _genericRepository.GetAsync<List<IFormFile>>(builder.ToString(), authToken));
        }

        public async Task<bool> PostFileAsync(IFormFile file, string endPoint, string authToken)
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint
            };

            await Task.Run(() => _genericRepository.PostAsyncBool<IFormFile>(builder.ToString(), file, authToken));

            return false;
        }

        public async Task<ResponseErrorEntry> PostFilesAsync(MultipartFormDataContent payload, string endPoint, string batchUid, string authToken)
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint + "/" + batchUid + "/file"
            };

            ResponseErrorEntry result = null;
            HttpResponseMessage httpResult = await _genericRepository.PostFilesAsync<HttpResponseMessage>(builder.Path, payload, authToken);

            if (httpResult != null && !httpResult.IsSuccessStatusCode)
            {
                result = new ResponseErrorEntry()
                {
                    ReasonPhrase = httpResult.ReasonPhrase,
                    StatusCode = httpResult.StatusCode.ToString()
                };
            }

            return result;
        }

        public async Task<ResponseErrorEntry> PostFilesAsync(MultipartFormDataContent payload, string endPoint, string authToken)
        {
            ResponseErrorEntry result = null;
            HttpResponseMessage httpResult = await Task.Run(() => _genericRepository.PostFilesAsync<HttpResponseMessage>(endPoint, payload, authToken));

            if (httpResult != null && !httpResult.IsSuccessStatusCode)
            {
                result = new ResponseErrorEntry()
                {
                    ReasonPhrase = httpResult.ReasonPhrase,
                    StatusCode = httpResult.StatusCode.ToString()
                };

            }

            return result;
        }

        public async Task<bool> PutFileAsync(IFormFile file, string endPoint, string authToken)
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint + "/" + file.FileName
            };

            await Task.Run(() => _genericRepository.PutAsync<IFormFile>(builder.ToString(), file, authToken));

            return false;
        }
        public async Task DeleteFileAsync(string fileName, string endPoint, string authToken)
        {
            UriBuilder builder = new UriBuilder()
            {
                Path = endPoint + "/" + fileName
            };

            await Task.Run(() => _genericRepository.DeleteAsync(builder.ToString(), authToken));
        }
    }
}
