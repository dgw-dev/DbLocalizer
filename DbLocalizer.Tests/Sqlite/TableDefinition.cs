
using System.Collections.Generic;

namespace DbLocalizer.Tests.Sqlite
{
    public class TableDefinition
    {
        public string TableName { get; set; }
        public List<ColumnDefinition> Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
        public string Constraint { get; set; }
    }
}
