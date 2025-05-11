using System.Collections.Generic;
using System.Linq;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingImportSqlFilePackageCollection
    {
        public List<SmartlingImportSqlFilePackage> Packages { get; } = new();

        public bool TryGetFirstPackage(out SmartlingImportSqlFilePackage package)
        {
            package = Packages.FirstOrDefault();
            return package is not null;
        }

        public void AddPackage(SmartlingImportSqlFilePackage package)
        {
            Packages.Add(package);
        }
    }
}
