using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingSqlTableFormatter
    {
        private readonly Table _table;
        private readonly string _cultureCodeSqlVariable;
        private readonly List<SmartlingSqlSourceTranslatedColumnPair> _orderedColumnPairs;
        private string LangTempTable => $"{_table.BaseTable}LangTemp";
        private static string IndentText(string text) => "    " + text;

        public SmartlingSqlTableFormatter(Table table, string cultureCodeSqlVariable)
        {
            _table = table;
            _cultureCodeSqlVariable = cultureCodeSqlVariable;
            _orderedColumnPairs = CreateOrderedSourceTranslatedColumnPairs(table);
        }

        private sealed class SqlColumnSelectCollection : SqlColumnCollectionBase
        {
            public string SelectSQL => string.Join(", ", ColumnData.Select(pair => pair.Item2));

            public void AddColumnSelect(string columnName, string selectExpression)
            {
                ColumnData.Add((columnName, selectExpression));
            }
        }

        private static List<SmartlingSqlSourceTranslatedColumnPair> CreateOrderedSourceTranslatedColumnPairs(Table table)
        {
            var columns = new List<SmartlingSqlSourceTranslatedColumnPair>();

            foreach (SourceLanguageColumn source in table.SourceLanguageColumns)
            {
                TranslatedLanguageColumn translated = table.FindTranslatedFromSourceColumn(source);
                columns.Add(new SmartlingSqlSourceTranslatedColumnPair(source, translated));
            }

            return columns.OrderBy(column => column.Source.ColumnName).ToList();
        }

        public string ToSqlString()
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("-- Import table: " + _table.FullLocalizedTableName);

            sqlBuilder.AppendLine(CreateTempTable());
            sqlBuilder.AppendLine(FillTempTables());
            sqlBuilder.AppendLine(UpdateOrInsertCultureRecords());

            return sqlBuilder.ToString();
        }

        private string CreateTempTable()
        {
            var langTempDefinitions = GetLangTempColumnDefinitions();


            return $"""
            CREATE TABLE #{LangTempTable}
                ( {langTempDefinitions.ColumnSchemaSQL} )
            """;
        }

        private string FillTempTables()
        {
            var sqlBuilder = new StringBuilder();

            string langTempInsertColumns = GetLangTempColumnDefinitions().ColumnListSQL;

            // Populate INSERT VALUES to the language temp table - note the max number of arguments to INSERT VALUES (...) is 1000
            foreach (SmartlingSqlRowFormatter[] rowBuilderChunk in GetUniqueRowFormatters().Chunk(1000))
            {
                string insertIntoValues = $"INSERT INTO #{LangTempTable}({langTempInsertColumns}) VALUES";
                var lastValidRowBuilder = rowBuilderChunk.LastOrDefault(row => row.IsRowValid());

                if (rowBuilderChunk.Any(row => row.IsRowValid()))
                {
                    sqlBuilder.AppendLine(insertIntoValues);
                }
                else
                {
                    sqlBuilder.AppendLine("-- " + insertIntoValues); // All rows are invalid, comment out INSERT INTO
                }

                foreach (SmartlingSqlRowFormatter rowBuilder in rowBuilderChunk)
                {
                    bool isLastValidRow = rowBuilder == lastValidRowBuilder;
                    string insertRowSql = rowBuilder.InsertRowValueSql;

                    if (rowBuilder.IsRowInvalid(out string invalidReason))
                    {
                        sqlBuilder.AppendLine(IndentText($"-- WARNING: {invalidReason}"));
                        sqlBuilder.AppendLine(IndentText("/*" + insertRowSql + "*/"));
                    }
                    else
                    {
                        sqlBuilder.Append(IndentText(insertRowSql));

                        if (!isLastValidRow)
                        {
                            sqlBuilder.AppendLine(",");
                        }
                    }
                }

                sqlBuilder.AppendLine(";");
                sqlBuilder.AppendLine();
            }

            return sqlBuilder.ToString();
        }

        private string UpdateOrInsertCultureRecords()
        {
            string updateCultureTableSetExpression = string.Join(",\n", GetUpdateCultureTableSetExpressions());

            var selectInsertCollection = GetInsertCultureTableColumnSelects();

            string insertCultureTableSql = $"""
                -- Insert new records into {_table.FullLocalizedTableName}
                INSERT INTO {_table.FullLocalizedTableName}({selectInsertCollection.ColumnListSQL})
                SELECT {selectInsertCollection.SelectSQL}
                FROM #{LangTempTable} t
                LEFT JOIN {_table.FullLocalizedTableName} c ON (t.{_table.PrimaryKeyName} = c.{_table.PrimaryKeyName} AND c.CultureCode = {_cultureCodeSqlVariable}) 
                WHERE c.{_table.PrimaryKeyName} IS NULL
                """;

            string updateWhereClause = "WHERE " + string.Join("OR\n ", GetUpdateWhereClause());

            string updateCultureTableSql = $"""
                -- Update existing records in {_table.FullLocalizedTableName}
                UPDATE c
                SET 
                {updateCultureTableSetExpression}
                FROM {_table.FullLocalizedTableName} c
                INNER JOIN #{LangTempTable} t ON (c.{_table.PrimaryKeyName} = t.{_table.PrimaryKeyName} AND c.CultureCode = {_cultureCodeSqlVariable}) 
                {updateWhereClause}
                """;

            string dropTempTableSQL = $"""
                -- Drop #{LangTempTable} temporary table
                IF OBJECT_ID('tempdb..#{LangTempTable}') IS NOT NULL DROP TABLE #{LangTempTable};
                """;

            var sqlBuilder = new StringBuilder();

            sqlBuilder.AppendLine(insertCultureTableSql);
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine(updateCultureTableSql);
            sqlBuilder.AppendLine();
            sqlBuilder.AppendLine(dropTempTableSQL);

            return sqlBuilder.ToString();
        }

        private List<string> GetUpdateCultureTableSetExpressions()
        {
            var setExpressions = new List<string>();

            foreach (var pair in _orderedColumnPairs)
            {
                setExpressions.Add($"c.{pair.Source.ColumnName} = t.{pair.LangTempCulture}");
            }

            return setExpressions;
        }

        private List<string> GetUpdateWhereClause()
        {
            var whereClause = new List<string>();

            foreach (var pair in _orderedColumnPairs)
            {
                whereClause.Add($"ISNULL(c.{pair.Source.ColumnName}, '') <> ISNULL(t.{pair.LangTempCulture}, '') ");
            }

            return whereClause;
        }

        private SqlColumnSelectCollection GetInsertCultureTableColumnSelects()
        {
            var collection = new SqlColumnSelectCollection();

            collection.AddColumnSelect(_table.PrimaryKeyName, $"t.{_table.PrimaryKeyName}");
            collection.AddColumnSelect("CultureCode", _cultureCodeSqlVariable);

            foreach (var pair in _orderedColumnPairs)
            {
                collection.AddColumnSelect(pair.Translated.ColumnName, $"t.{pair.LangTempCulture}");
            }

            return collection;
        }

        private SqlColumnDefinitionCollection GetLangTempColumnDefinitions()
        {
            var collection = new SqlColumnDefinitionCollection();
            collection.AddColumnSchema(_table.PrimaryKeyName, "INT");

            foreach (var pair in _orderedColumnPairs)
            {
                string maxLength = pair.Source.MaxLength.ToString();
                if (pair.Source.MaxLength == 0 || pair.Source.MaxLength > 1024)
                {
                    maxLength = "max";
                }

                collection.AddColumnSchema(pair.LangTempCulture, $"NVARCHAR({maxLength})");
            }

            return collection;
        }

        private List<SmartlingSqlRowFormatter> GetUniqueRowFormatters()
        {
            return _table.Rows
                .Select(row => new SmartlingSqlRowFormatter(row, _orderedColumnPairs))
                .DistinctBy(row => row.InsertRowValueSql)
                .ToList();
        }

        private sealed class SqlColumnDefinitionCollection : SqlColumnCollectionBase
        {
            public string ColumnSchemaSQL => string.Join(", ", ColumnData.Select(pair => $"{pair.Item1} {pair.Item2}"));

            public void AddColumnSchema(string columnName, string sqlTypeExpression)
            {
                ColumnData.Add((columnName, sqlTypeExpression));
            }
        }

        private abstract class SqlColumnCollectionBase
        {
            protected readonly List<(string, string)> ColumnData = new();
            public string ColumnListSQL => string.Join(", ", ColumnData.Select(pair => pair.Item1));
        }
    }
}
