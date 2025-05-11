using System.Collections.Generic;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingSqlProcessor
    {
        string BuildSqlString(List<Table> tables, string locale);
    }
}
