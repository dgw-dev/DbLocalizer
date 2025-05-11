using System.Net.Http;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingDataService
    {
        Task<T> Authenticate<T, U>(U data);
        Task<T> CreateJob<T, U>(string endPoint, string authToken, U jobData);
        Task<T> CreateJobBatch<T, U>(string endPoint, string authToken, U jobBatchData);
        Task<T> GetSmartlingObjectDetails<T>(string endPoint, string authToken);
        Task<SmartlingFileData> GetTranslatedFile(string endPoint, string authToken, string localeId, string fileUri);
        Task<ResponseErrorEntry> PostFilesAsync(MultipartFormDataContent payload, string endPoint, string batchUid, string authToken, string packageId = default);
    }
}
