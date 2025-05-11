using System.Collections.Generic;

namespace Entities.Configuration
{
    public class DatabaseConfig
    {
        public List<DatabaseConfigItem> DatabaseCollection { get; set; } = new List<DatabaseConfigItem>();
    }

    public class DatabaseConfigItem
    {
        public string Name { get; set; }
        public string ConnectionStringName { get; set; }
        public string SourceLocale { get; set; }
        public string CultureTableSuffix { get; set; }
        public List<string> TableExclusions { get; set; } = new List<string>();
        public List<string> ExportDataTypes { get; set; } = new List<string>();
        public List<string> ColumnExclusions { get; set; } = new List<string>();
        public TableLevelExclusions TableLevelExclusions { get; set; } = new TableLevelExclusions();
    }

    public class TableLevelExclusions
    {
        public List<DbTable> DbTable { get; set; } = new List<DbTable>();
    }

    public class DbTable
    {
        public string FullTableName { get; set; }
        public List<string> ColumnExclusions { get; set; } = new List<string>();
    }
}
