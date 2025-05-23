using System.Collections.Generic;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingImportUtility
    {
        List<ImportErrorEntry> ErrorList { get; set; }
        bool OperationComplete { get; set; }
        Task<SmartlingImportSqlFilePackageCollection> Import(SmartlingImportJsonFileCollection collection);
    }
}
