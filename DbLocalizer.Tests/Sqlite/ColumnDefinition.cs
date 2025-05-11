
namespace DbLocalizer.Tests.Sqlite
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public string Constraint { get; set; }
    }
}
