using System.Collections.Generic;
using System.Text;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingSqlDocumentFormatter
    {
        private readonly string _cultureCode;
        private readonly string _cultureCodeSqlVariable = "@CultureCode";
        private readonly List<SmartlingSqlTableFormatter> _tableFormatters = new();

        public SmartlingSqlDocumentFormatter(string cultureCode)
        {
            _cultureCode = cultureCode;
        }

        public void AddTableToImport(Table table)
        {
            var formatter = new SmartlingSqlTableFormatter(table, _cultureCodeSqlVariable);
            _tableFormatters.Add(formatter);
        }

        public string ToSqlString()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine($"DECLARE {_cultureCodeSqlVariable} NVARCHAR(10) = '{_cultureCode}';\n");

            foreach (SmartlingSqlTableFormatter tableFormatter in _tableFormatters)
            {
                sqlBuilder.AppendLine(tableFormatter.ToSqlString());
            }

            return sqlBuilder.ToString();
        }
    }
}
