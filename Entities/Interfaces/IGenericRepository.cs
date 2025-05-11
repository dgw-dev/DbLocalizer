
using System.Net.Http;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface IGenericRepository
    {
        Task<T> GetItemAsync<T>(string id, string uri, string authToken);
        Task<T> GetAsync<T>(string uri, string authToken);
        Task<HttpResponseMessage> GetFileAsync<HttpResponseMessage>(string uri, string authToken);
        Task<bool> PostAsyncBool<T>(string uri, T data, string authToken);
        Task<T> PostFilesAsync<T>(string uri, MultipartFormDataContent content, string authToken);
        Task<bool> PutAsync<T>(string uri, T data, string authToken);
        Task DeleteAsync(string uri, string authToken);
        Task<HttpResponseMessage> PostAsync<T>(string uri, T data, string authToken);
        Task<HttpResponseMessage> PostAsync<T, U>(string uri, U data, string authToken);
    }
}
