
using Entities.Configuration;
using System.Collections.Generic;

namespace Entities.Interfaces
{
    public interface ISqlSchemaBuilder
    {
        void BuildDatabaseSchemaFiles(Database database);
        bool CheckSchemaObjectsExist(string databaseName);
        List<string> GetTablesFromFiles(Database database);
        List<ExportColumn> GetColumnsFromFileForTable(string tableName, Database database);
        List<ExportColumn> GetColumns(string json, List<string> includeDataTypes, List<string> columnExclusions, TableLevelExclusions tableLevelExclusions, string tableName);
    }
}
