using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;


namespace Entities.Utilities
{
    public static class SqliteUtility
    {
        private static string _dbPath = AppDomain.CurrentDomain.BaseDirectory;
        private const string _connectionString = "Data Source=logs.db";

        public static async Task CreateDatabase()
        {
            // Create a new SQLite database
            FileInfo fileInfo = new FileInfo(_dbPath + @"\logs.db");
            if (!fileInfo.Exists)
            {
                File.Create(_dbPath + @"\logs.db");
            }

            bool tableExists = await DoesTableExist("Logs");

            if (!tableExists)
            {
                await CreateTable("Logs");
            }
        }

        public static async Task CreateTable(string tableName)
        {
            // Create tables in the SQLite database

            // Create a new database connection
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // SQL command to create a new table
                string createTableQuery = @"
                CREATE TABLE Logs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LogLevel TEXT,
                    Category TEXT,
                    Message TEXT,
                    CreatedAt TEXT
                );";

                // Execute the command
                using (SqliteCommand command = new SqliteCommand(createTableQuery, connection))
                {
                    await command.ExecuteScalarAsync();
                }
            }
        }

        public static async Task<DataTable> GetData(string tableName)
        {
            // Get data from the SQLite database
            DataTable dataTable = new DataTable();
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = $"SELECT * FROM {tableName};";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }
            return dataTable;
        }

        public static async Task<bool> TruncateData(string tableName)
        {
            // Get data from the SQLite database
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = $"DELETE FROM {tableName};";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
            }
        }

        public static void DeleteDatabase()
        {
            //Delete the SQLite database
            File.Delete(_dbPath + @"\logs.db");
        }

        private static async Task<bool> DoesTableExist(string tableName)
        {
            // Check if a table exists in the SQLite database

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = $"SELECT 1 FROM {tableName} LIMIT 1;";

                try
                {
                    using (var command = new SqliteCommand(query, connection))
                    {
                        await command.ExecuteScalarAsync();
                        return true;
                    }
                }
                catch (SqliteException)
                {
                    return false;
                }
            }
        }
    }
}
