
using System.Net.Http;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface IGenericPluginInData
    {
        Task<T> GetItemAsync<T>(string id, string uri, string authToken = default(string));
        Task<T> GetAsync<T>(string uri, string authToken = default(string));
        Task<HttpResponseMessage> GetFileAsync<HttpResponseMessage>(string uri, string authToken);
        Task<bool> PostAsyncBool<T>(string uri, T data);
        Task<T> PostFilesAsync<T>(string uri, MultipartFormDataContent content, string authToken = default(string), string packageId = default(string));
        Task<bool> PutAsync<T>(string uri, T data, string authToken = default(string));
        Task DeleteAsync(string uri, string authToken = default(string));
        Task<HttpResponseMessage> PostAsync<T>(string uri, T data, string authToken = default(string));
        Task<HttpResponseMessage> PostAsync<T, U>(string uri, U data, string authToken = default(string));
    }
}
