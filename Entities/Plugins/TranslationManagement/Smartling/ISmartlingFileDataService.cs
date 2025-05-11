using System;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingFileDataService
    {
        Task<SmartlingImportJsonFileCollection> GetTranslatedFileForLocaleAsync(string locale, string fileUri, Guid processId);
        Task<SmartlingImportJsonFileCollection> GetTranslatedFilesForAllLocalesAsync(string fileUri, Guid processId);
    }
}