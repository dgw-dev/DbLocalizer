
using System;
using System.Collections.Generic;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingSqlRowFormatter
    {
        private readonly Row _row;
        private readonly List<SmartlingSqlSourceTranslatedColumnPair> _orderedColumns;

        private string _rowValueSqlCached = null;
        public string InsertRowValueSql => _rowValueSqlCached ??= GetInsertRowValueSql();

        public SmartlingSqlRowFormatter(Row row, List<SmartlingSqlSourceTranslatedColumnPair> orderedColumns)
        {
            _row = row;
            _orderedColumns = orderedColumns;
        }

        private Source FindSourceFromTranslatedColumn(TranslatedLanguageColumn column)
        {
            return _row.Source.Find(source => string.Equals(source.Column, column.ColumnName, StringComparison.OrdinalIgnoreCase));
        }

        private static string EscapeSqlQuote(string text)
        {
            return text?.Replace("'", "''") ?? "";
        }

        public bool IsRowValid()
        {
            return !IsRowInvalid(out string _);
        }

        public bool IsRowInvalid(out string invalidReason)
        {
            invalidReason = null;

            if (IsRowExceedAnyColumnMaxLengths())
            {
                invalidReason = "Translation length exceeds exported column MaxLength";
            }

            return invalidReason is not null;
        }

        private bool IsRowExceedAnyColumnMaxLengths()
        {
            bool exceedMaxLengthDetected = false;

            foreach (var pair in _orderedColumns)
            {
                var sourceText = FindSourceFromTranslatedColumn(pair.Translated).Text as string ?? ""; // text from TMS
                exceedMaxLengthDetected |= sourceText.Length > pair.Translated.MaxLength;
            }

            return exceedMaxLengthDetected;
        }

        private string GetInsertRowValueSql()
        {
            var insertValues = new List<string>();
            insertValues.Add(_row.PrimaryKeyValue);

            foreach (var pair in _orderedColumns)
            {
                var source = FindSourceFromTranslatedColumn(pair.Translated);
                insertValues.Add($"N'{EscapeSqlQuote(source.Text as string)}'");
            }

            string insertValueSql = string.Join(", ", insertValues);

            return $"({insertValueSql})";
        }
    }
}
