using System.Collections.Generic;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingFileSqlRowFormatter
    {
        private readonly Row _row;
        private readonly List<SmartlingSqlSourceTranslatedColumnPair> _orderedColumns;

        private string _rowValueSqlCached = null;
        public string InsertRowValueSql => _rowValueSqlCached ??= GetInsertRowValueSql();

        public SmartlingFileSqlRowFormatter(Row row, List<SmartlingSqlSourceTranslatedColumnPair> orderedColumns)
        {
            _row = row;
            _orderedColumns = orderedColumns;
        }

        private string GetInsertRowValueSql()
        {
            var insertValues = new List<string>();

            foreach (var pair in _orderedColumns)
            {
                //var baseValue = FindBaseValueFromSourceColumn(pair.Source);
                //var source = FindSourceFromTranslatedColumn(pair.Translated);

                //insertValues.Add($"N'{EscapeSqlQuote(baseValue.Text as string)}'");
                //insertValues.Add($"N'{EscapeSqlQuote(source.Text as string)}'");
            }

            string insertValueSql = string.Join(", ", insertValues);

            return $"({insertValueSql})";
        }
    }
}
