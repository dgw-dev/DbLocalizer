using System.Collections.Generic;

namespace Entities.Utilities
{
    public class ExportTable
    {
        public string FullTableName { get; set; }
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public string CultureCode { get; set; }
        public List<TableColumn> Columns { get; set; }
        public List<ExportTableRow> Rows { get; set; }
        public string TableSchema { get; set; }
        public string FullLocalizedTableName { get; set; }

        public ExportTable() { }

        public ExportTable(
                string fullTableName = default(string),
                string tableName = default(string),
                string primaryKey = default(string),
                List<TableColumn> columns = null,
                List<ExportTableRow> rows = null,
                string tableSchema = default,
                string fullLocalizedTableName = default)
        {
            FullTableName = fullTableName;
            TableName = tableName;
            PrimaryKey = primaryKey;
            if (columns == null)
            {
                Columns = new List<TableColumn>();
            }
            else
            {
                Columns = columns;
                int numberOfSourceColumns = Columns.Count;
                //add translation columns to match source columns
                for (int i = 0; i < numberOfSourceColumns; i++)
                {
                    Columns.Add(new TableColumn()
                    {
                        ColumnName = Columns[i].ColumnName,
                        ColumnDataType = Columns[i].ColumnDataType,
                        ColumnLength = Columns[i].ColumnLength,
                        IsSourceColumn = false
                    });
                }
            }

            if (rows == null)
            {
                Rows = new List<ExportTableRow>();
            }
            else
            {
                Rows = rows;
            }

            TableSchema = tableSchema;
            FullLocalizedTableName = fullLocalizedTableName;
        }
    }
}
