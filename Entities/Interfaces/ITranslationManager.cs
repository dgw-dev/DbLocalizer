using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface ITranslationManager
    {
        Task<bool> TMSOperations<T>(Dictionary<string, T> dtmPackage, MetaData globalMetaData, string packageId, CancellationToken ct, List<string> culturesOverride, Cultures cultures, Guid processId);
        Task<bool> PostDataToTMS(MultipartFormDataContent outputContent, MetaData globalMetaData, string packageId, string authToken, CancellationToken ct);
    }
}
