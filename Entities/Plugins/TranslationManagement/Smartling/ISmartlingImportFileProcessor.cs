using Entities.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingImportFileProcessor
    {
        List<ImportErrorEntry> ErrorList { get; set; }
        ICacheManager CacheManager { get; set; }
        Task<SmartlingImportSqlFilePackage> GetCultureContentPackage(SmartlingImportJsonFile importFile);
    }
}
