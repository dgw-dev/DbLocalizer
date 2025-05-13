
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Entities.Utilities
{
    public class SqlUtility
    {
        private static string _localizedTableSuffix = string.Empty;
        public string ExportLookbackInDays { get; set; }
        public List<string> ExclusionList { get; set; }
        public DataTable BaseTables { get; set; }


        private static readonly string _sqlGetAllTables = @"
            SELECT DISTINCT
                SC1.TABLE_NAME AS 'TableName',
                MIN(QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME)) AS 'FullTableName',
                {0}
                SC1.TABLE_SCHEMA AS 'TableSchema'
                    FROM INFORMATION_SCHEMA.TABLES AS SC1
                    WHERE
	                    TABLE_TYPE = 'BASE TABLE' " +
                        " {1} " +
                        " {2} " +
            " GROUP BY SC1.TABLE_NAME, SC1.TABLE_SCHEMA";


        public SqlUtility(Database database)
        {
            _localizedTableSuffix = database.LocalizedTableSuffix;

            if (database.TableExclusions != null && database.TableExclusions.Count > 0)
            {
                ExclusionList = database.TableExclusions;
            }
            else
            {
                ExclusionList = new List<string>();
            }
        }

        public SqlUtility(string exportLookbackInDays, List<string> exclusionList = null, List<string> doNotTranslateList = null)
        {
            ExportLookbackInDays = exportLookbackInDays;

            if (exclusionList != null && exclusionList.Count > 0)
            {
                ExclusionList = exclusionList;
            }
            else
            {
                ExclusionList = new List<string>();
            }
        }

        public static bool CheckConnection(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public string GetBaseTables()
        {
            string tablesToExclude = string.Empty;
            string exclusionList = string.Empty;
            string cultureTable = string.Empty;
            string fullCultureTableName = "'' AS 'FullLocalizedTableName,";

            if (ExclusionList != null && ExclusionList.Count > 0)
            {
                for (int i = 0; i < ExclusionList.Count; i++)
                {
                    if (i < ExclusionList.Count - 1)
                    {
                        exclusionList += " '" + ExclusionList[i] + "', ";
                    }
                    else
                    {
                        exclusionList += " '" + ExclusionList[i] + "'";
                    }
                }
                tablesToExclude = " AND SC1.TABLE_NAME NOT IN (" + exclusionList + ")";
            }

            if (!string.IsNullOrEmpty(_localizedTableSuffix))
            {
                fullCultureTableName = "MIN(QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME + '" + _localizedTableSuffix + "')) AS 'FullLocalizedTableName',";
                cultureTable = " AND SC1.TABLE_NAME NOT LIKE '%" + _localizedTableSuffix + "%'";
            }

            return string.Format(_sqlGetAllTables, fullCultureTableName, tablesToExclude, cultureTable);
        }

        public static string SqlGetTableDataToExport(string tableName, Database db)
        {
            List<ExportColumn> columns = db.GetTableColumns(tableName);
            if (columns == null || columns.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            string colList = string.Empty;

            colList = columns.FirstOrDefault(x => x.IsPrimaryKey).Name + ", ";

            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].IsExportable)
                {
                    colList += columns[i].Name + ", ";
                }
            }

            int finalComma = colList.LastIndexOf(",");
            colList = colList.Remove(finalComma, 1);

            sb.AppendLine("SELECT " + colList + " FROM " + tableName);

            return sb.ToString();
        }

        public static string GetColumns(ExportTable table)
        {
            StringBuilder colList = new StringBuilder();

            colList.Append(table.PrimaryKey).Append(", ");

            //build column list
            if (table?.Columns?.Count > 0)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (!table.Columns[i].IsSourceColumn)
                    {
                        colList.Append(table.Columns[i].ColumnName).Append(", ");
                    }
                }
            }

            int finalComma = colList.ToString().LastIndexOf(",");
            colList.Remove(finalComma, 1);

            return colList.ToString();
        }
    }
}
