using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingImportUtility
    {
        Task<SmartlingImportSqlFilePackageCollection> Import(SmartlingImportJsonFileCollection collection);
    }
}
