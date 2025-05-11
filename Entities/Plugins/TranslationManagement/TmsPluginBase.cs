using Entities.Interfaces;
using Entities.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement
{
    public class TmsPluginBase
    {
        protected readonly ICacheManager _cacheManager;
        public TmsPluginBase(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        protected static async Task<bool> IsOutputContentValid(MultipartFormDataContent outputContent)
        {
            if (outputContent != null)
            {
                string result = await outputContent.ReadAsStringAsync();
                return !string.IsNullOrEmpty(result);
            }

            return false;
        }

        protected StreamContent CreateFileContent(string fileContent, string fileName, string contentType, Encoding encoding)
        {
            MemoryStream fileStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(fileStream, encoding);
            writer.Write(fileContent);
            writer.Flush();
            fileStream.Position = 0;

            StreamContent streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = fileName,
            };
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return streamContent;
        }

        protected static bool IsCancelled(CancellationToken ct, Guid processId)
        {
            if (ct.IsCancellationRequested || !TokenStore.Store.TryGetValue(processId, out CancellationTokenSource _))
            {
                return true;
            }

            return false;
        }
    }
}
