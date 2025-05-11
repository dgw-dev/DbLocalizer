using Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingFileData
    {
        public Smartling smartling { get; set; }
        public MetaData GlobalizationMetaData { get; set; }

        public List<Table> Tables { get; set; }

        public class SystemColumn
        {
            public string ColumnName { get; set; }
            public int MaxLength { get; set; }
            public string DataType { get; set; }
        }

        public class SourceLanguageColumn
        {
            public string ColumnName { get; set; }
            public int MaxLength { get; set; }
            public string DataType { get; set; }
            public string TranslatedLanguageColumnName { get; set; }
        }

        public class TranslatedLanguageColumn
        {
            public string ColumnName { get; set; }
            public int MaxLength { get; set; }
            public string DataType { get; set; }
            public string SourceLanguageColumnName { get; set; }
        }
        public class Source
        {
            public string BaseTable { get; set; }
            public string LocalizedTable { get; set; }
            public int MaxLength { get; set; }
            public string Column { get; set; }
            public object Text { get; set; }
        }

        public class Row
        {
            public List<Source> Source { get; set; }
            public bool IsUpdate { get; set; }
            public string PrimaryKeyValue { get; set; }

            public override int GetHashCode()
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(JsonUtility.SerializeData(this.Source));
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                Row otherRow = (Row)obj;
                return StringComparer.OrdinalIgnoreCase.Equals(JsonUtility.SerializeData(this.Source), JsonUtility.SerializeData(otherRow.Source));
            }
        }

        public class Table
        {
            public string PrimaryKeyName { get; set; }
            public string BaseTable { get; set; }
            public string FullBaseTableName { get; set; }
            public string LocalizedTable { get; set; }
            public string FullLocalizedTableName { get; set; }
            public string TableSchema { get; set; }
            public List<SystemColumn> SystemColumns { get; set; }
            public List<SourceLanguageColumn> SourceLanguageColumns { get; set; }
            public List<TranslatedLanguageColumn> TranslatedLanguageColumns { get; set; }
            public HashSet<Row> Rows { get; set; }

            public TranslatedLanguageColumn FindTranslatedFromSourceColumn(SourceLanguageColumn source)
            {
                return TranslatedLanguageColumns.FirstOrDefault(translated => string.Equals(translated.SourceLanguageColumnName, source.ColumnName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
