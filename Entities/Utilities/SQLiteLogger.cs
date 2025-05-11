using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;

namespace Entities.Utilities
{
    public class SQLiteLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _connectionString;
        public SQLiteLogger(string categoryName, string connectionString)
        {
            _categoryName = categoryName;
            _connectionString = connectionString;
        }
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (logLevel == LogLevel.Error || logLevel == LogLevel.Warning)
            {
                var logMessage = formatter(state, exception);
                var logLevelString = logLevel.ToString();
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                INSERT INTO Logs (LogLevel, Category, Message, CreatedAt)
                VALUES ($logLevel, $category, $message, $createdAt)";
                    command.Parameters.AddWithValue("$logLevel", logLevelString);
                    command.Parameters.AddWithValue("$category", _categoryName);
                    command.Parameters.AddWithValue("$message", logMessage);
                    command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
