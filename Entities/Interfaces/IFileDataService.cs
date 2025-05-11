using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface IFileDataService
    {
        Task<IFormFile> GetSelectedFileAsync(string fileName, string endPoint, string authToken);
        Task<List<IFormFile>> GetTeamFilesAsync(string teamName, string endPoint, string authToken);
        Task<bool> PostFileAsync(IFormFile file, string endPoint, string authToken);
        Task<ResponseErrorEntry> PostFilesAsync(MultipartFormDataContent payload, string endPoint, string batchUid, string authToken);
        Task<ResponseErrorEntry> PostFilesAsync(MultipartFormDataContent payload, string endPoint, string authToken);
        Task<bool> PutFileAsync(IFormFile file, string endPoint, string authToken);
        Task DeleteFileAsync(string fileName, string endPoint, string authToken);

    }
}
