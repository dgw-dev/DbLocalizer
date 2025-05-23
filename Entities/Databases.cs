using Entities.Configuration;
using Entities.DAL;
using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Data;

namespace Entities
{
    public class Databases : IDatabases
    {
        private readonly IConfiguration _config;
        private readonly ISqlSchemaBuilder _sqlSchemaBuilder;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IEncryptionService _encryptionService;
        public List<Database> DatabaseCollection { get; set; } = new List<Database>();

        public Databases(IConfiguration config, ISqlSchemaBuilder sqlSchemaBuilder, IEncryptionService encryptionService)
        {
            _config = config;
            _sqlSchemaBuilder = sqlSchemaBuilder;
            _encryptionService = encryptionService;
            BuildDatabases();
        }

        private void BuildDatabases()
        {
            var dbConfig = _config.Get<DatabaseConfig>();
            foreach (var dbConfigItem in dbConfig.DatabaseCollection)
            {
                Database db = new Database(_sqlSchemaBuilder)
                {
                    Name = dbConfigItem.Name,
                    ConnectionStringName = dbConfigItem.ConnectionStringName,
                    SourceLocale = dbConfigItem.SourceLocale,
                    LocalizedTableSuffix = dbConfigItem.LocalizedTableSuffix,
                    TableExclusions = dbConfigItem.TableExclusions,
                    ExportDataTypes = dbConfigItem.ExportDataTypes,
                    ColumnExclusions = dbConfigItem.ColumnExclusions,
                    TableLevelExclusions = dbConfigItem.TableLevelExclusions,
                };

                string connectionString = _config.GetConnectionString(db.ConnectionStringName);
                DbCredentialsManager dbCredentialsManager = new DbCredentialsManager();
                DbCredentials dbCredentials = dbCredentialsManager.GetDbCredentials(db.ConnectionStringName);
                if (dbCredentials != null
                    && !string.IsNullOrEmpty(dbCredentials.ServerName)
                    && !string.IsNullOrEmpty(dbCredentials.DatabaseName)
                    && !string.IsNullOrEmpty(dbCredentials.Username) 
                    && !string.IsNullOrEmpty(dbCredentials.Password) 
                    && !string.IsNullOrEmpty(connectionString))
                {
                    connectionString = string.Format(connectionString, dbCredentials.ServerName, dbCredentials.DatabaseName, dbCredentials.Username, dbCredentials.Password);
                    db.ConnectionStringValue = _encryptionService.Encrypt(connectionString);
                    db.CanConnect = DatabaseCanConnect(db.ConnectionStringValue);
                    _sqlSchemaBuilder.BuildDatabaseSchemaFiles(db);
                }
                else
                {
                    db.Tables = _sqlSchemaBuilder.GetTablesFromFiles(db);
                }
                db.HasSchemaObjects = _sqlSchemaBuilder.CheckSchemaObjectsExist(db.Name);
                DatabaseCollection.Add(db);
            }
        }

        private bool DatabaseCanConnect(string connectionString)
        {
            return SqlUtility.CheckConnection(_encryptionService.Decrypt(connectionString));
        }

    }

    public class Database
    {
        private readonly ISqlSchemaBuilder _sqlSchemaBuilder;

        public Database(ISqlSchemaBuilder sqlSchemaBuilder)
        {
            _sqlSchemaBuilder = sqlSchemaBuilder;
        }
        public string Name { get; set; }
        public string ConnectionStringName { get; set; }
        public string SourceLocale { get; set; }
        public string LocalizedTableSuffix { get; set; }
        public string ConnectionStringValue { get; set; }
        public bool HasSchemaObjects { get; set; } = false;
        public bool CanConnect { get; set; } = false;
        public List<string> Tables { get; set; } = new List<string>();
        public List<string> TableExclusions { get; set; } = new List<string>();
        public List<string> ExportDataTypes { get; set; } = new List<string>();
        public List<string> ColumnExclusions { get; set; } = new List<string>();
        public TableLevelExclusions TableLevelExclusions { get; set; } = new TableLevelExclusions();
        public DataSet ExportTables { get; set; } = new DataSet();
        public Dictionary<string, string> TableSchema { get; set; } = new Dictionary<string, string>();

        public List<ExportColumn> GetTableColumns(string tableName)
        {
            List<ExportColumn> columns = new List<ExportColumn>();
            if (TableSchema.Count == 0)
            {
                columns = _sqlSchemaBuilder.GetColumnsFromFileForTable(tableName, this);
            }
            if (TableSchema.ContainsKey(tableName))
            {
                string json = TableSchema[tableName];
                columns = _sqlSchemaBuilder.GetColumns(json, ExportDataTypes, ColumnExclusions, TableLevelExclusions, tableName);
            }
            return columns;
        }
    }
}
