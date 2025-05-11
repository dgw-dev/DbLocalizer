using Entities.Configuration;
using Entities.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Utilities
{
    public class SqlSchemaBuilder : ISqlSchemaBuilder
    {
        private readonly IDataProvider _dataProvider;
        public SqlSchemaBuilder(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void BuildDatabaseSchemaFiles(Database database)
        {
            SqlUtility sqlUtil = new SqlUtility(database);
            string sql = sqlUtil.GetBaseTables();
            DataTable tables = Task.Run(() => _dataProvider.GetTables(database.ConnectionStringValue, "Tables", sql)).Result;
            database.ExportTables.Tables.Add(tables);

            if (tables != null)
            {
                string basePath = AppContext.BaseDirectory;
                string pathToObjects = Path.Combine(basePath, "Configuration", "Database", "SqlServer", database.Name, "Tables");

                if (!Directory.Exists(pathToObjects))
                {
                    Directory.CreateDirectory(pathToObjects);
                }
                if (Directory.Exists(pathToObjects))
                {
                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["FullTableName"].ToString();
                        database.Tables.Add(tableName);
                        DataTable tableSchema = _dataProvider.GetTabeSchema(database.ConnectionStringValue, tableName).Result;
                        ConvertDataTableToJsonWithSchema(pathToObjects, tableName, tableSchema, database);
                    }
                    database.HasSchemaObjects = CheckSchemaObjectsExist(database.Name);
                }
            }
        }

        public List<string> GetTablesFromFiles(Database database)
        {
            string path = GetPathToTables(database.Name);
            List<string> tables = new List<string>();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    tables.Add(fileName);
                }
            }
            return tables;
        }

        public List<ExportColumn> GetColumnsFromFileForTable(string tableName, Database database)
        {
            string path = GetPathToTables(database.Name);
            List<ExportColumn> columns = new List<ExportColumn>();

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, $"{tableName}.json", SearchOption.AllDirectories);
                if (files != null && files.Length != 0)
                {
                    foreach (string file in files)
                    {
                        if (tableName.Equals(Path.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase))
                        {
                            string json = File.ReadAllText(file);
                            columns = GetColumns(json, database.ExportDataTypes, database.ColumnExclusions, database.TableLevelExclusions, tableName);
                        }
                    }
                }
            }
            return columns;
        }

        private static void ConvertDataTableToJsonWithSchema(string path, string tableName, DataTable table, Database database)
        {
            var tableInfo = new
            {
                TableName = tableName,
                Columns = new List<object>(),
                Rows = new List<Dictionary<string, object>>()
            };

            // Extract column metadata
            foreach (DataColumn column in table.Columns)
            {
                tableInfo.Columns.Add(new
                {
                    ColumnName = column.ColumnName,
                    DataType = column.DataType.Name,
                    AllowDBNull = column.AllowDBNull,
                    IsUnique = column.Unique,
                    IsPrimaryKey = table.PrimaryKey.Contains(column)
                });
            }

            // Extract row data
            foreach (DataRow row in table.Rows)
            {
                var rowDict = new Dictionary<string, object>();

                foreach (DataColumn column in table.Columns)
                {
                    rowDict[column.ColumnName] = row[column];
                }
                tableInfo.Rows.Add(rowDict);
            }
            string result = JsonConvert.SerializeObject(tableInfo, Formatting.Indented);

            if (!string.IsNullOrEmpty(result))
            {
                // Create or overwrite the file with UTF-8 encoding
                using (StreamWriter writer = new StreamWriter(Path.Combine(path, $"{tableName}.json"), false, Encoding.UTF8))
                {
                    writer.WriteLine(result);
                }
                database.TableSchema.Add(tableName, result);
            }
        }

        public bool CheckSchemaObjectsExist(string databaseName)
        {
            string pathToObjects = GetPathToTables(databaseName);

            if (Directory.Exists(pathToObjects))
            {
                string[] files = Directory.GetFiles(pathToObjects, "*.json", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetPathToTables(string databaseName)
        {
            string basePath = AppContext.BaseDirectory;
            return Path.Combine(basePath, "Configuration", "Database", "SqlServer", databaseName, "Tables");
        }

        public List<ExportColumn> GetColumns(string json, List<string> includeDataTypes, List<string> columnExclusions, TableLevelExclusions tableLevelExclusions, string tableName)
        {
            List<ExportColumn> columns = new List<ExportColumn>();

            var tableInfo = JsonConvert.DeserializeObject<dynamic>(json);

            if (tableInfo == null || tableInfo.Rows == null || tableInfo.Rows.Count == 0)
            {
                return columns;
            }

            foreach (var row in tableInfo.Rows)
            {
                bool isExcluded = false;
                bool isExportable = false;

                //Do we have global overrides for types
                if ((includeDataTypes == null || includeDataTypes.Count == 0))
                {
                    //no overrides, export all data types and all columns
                    isExportable = true;
                }
                else
                {
                    //only export global override types and columns
                    isExportable = includeDataTypes.Any(dt => dt.Equals(row.DataTypeName.ToString(), StringComparison.OrdinalIgnoreCase));
                }

                //Do we have any global column level exclusions
                if (columnExclusions == null || columnExclusions.Count == 0)
                {
                    //no exclusions, export all columns
                    isExcluded = false;
                }
                else
                {
                    //only export columns not in the exclusion list
                    isExcluded = columnExclusions.Any(ex => ex.Equals(row.ColumnName.ToString(), StringComparison.OrdinalIgnoreCase));
                }

                //Do we have any table level exclusions
                DbTable tableLevel = tableLevelExclusions.DbTable?.FirstOrDefault(tn => tn.FullTableName == tableName);

                if (tableLevel != null && tableLevel.ColumnExclusions != null && tableLevel.ColumnExclusions.Count > 0)
                {
                    //check for table level column exclusions
                    isExcluded = tableLevel.ColumnExclusions.Any(ex => ex.Equals(row.ColumnName.ToString(), StringComparison.OrdinalIgnoreCase));
                }

                if (!isExcluded)
                {
                    columns.Add(new ExportColumn
                    {
                        Name = row.ColumnName.ToString(),
                        IsNullable = row.AllowDBNull.ToString(),
                        IsPrimaryKey = row.IsKey,
                        Type = row.DataTypeName.ToString(),
                        Length = row.ColumnSize,
                        IsExportable = isExportable,
                    });
                }
            }

            return columns;
        }
    }
}
