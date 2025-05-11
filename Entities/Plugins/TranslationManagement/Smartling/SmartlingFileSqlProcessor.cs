using System.Collections.Generic;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingSqlProcessor : ISmartlingSqlProcessor
    {
        public string BuildSqlString(List<Table> tables, string locale)
        {
            var sqlDocumentFormatter = new SmartlingSqlDocumentFormatter(locale);

            foreach (Table table in tables)
            {
                sqlDocumentFormatter.AddTableToImport(table);
            }

            return sqlDocumentFormatter.ToSqlString();
        }
    }
}
