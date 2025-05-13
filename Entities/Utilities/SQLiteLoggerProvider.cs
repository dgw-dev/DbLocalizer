using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Entities.Utilities
{
    public class SQLiteLoggerProvider : ILoggerProvider
    {
        private readonly string _connectionString;
        private readonly ConcurrentDictionary<string, SQLiteLogger> _loggers = new ConcurrentDictionary<string, SQLiteLogger>();
        public SQLiteLoggerProvider(string connectionString)
        {
            _connectionString = connectionString;
            SqliteUtility.CreateDatabase().Wait();
        }
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new SQLiteLogger(name, _connectionString));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            _loggers.Clear();
        }
    }
}
