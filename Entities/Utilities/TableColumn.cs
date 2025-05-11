namespace Entities.Utilities
{
    public class TableColumn
    {
        public string ColumnName { get; set; }
        public string ColumnDataType { get; set; }
        public int ColumnLength { get; set; }
        public bool IsSourceColumn { get; set; }

        public TableColumn(string columnName = default(string), string columnDataType = default(string), int columnLength = 0, bool isSourceColumn = true)
        {
            ColumnName = columnName;
            ColumnDataType = columnDataType;
            ColumnLength = columnLength; //if value is -1 this equates to nvarchar(max)
            IsSourceColumn = isSourceColumn;
        }
    }
}
