using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace DbLocalizer.Tests.Sqlite
{
    public class SqliteObjectBuilder
    {
        private readonly string _connectionString;

        public SqliteObjectBuilder(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a table in the database
        /// </summary>
        /// <param name="tableData"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CreateTable(TableDefinition tableDefinition)
        {
            if (string.IsNullOrEmpty(tableDefinition.TableName) || tableDefinition.Columns == null || tableDefinition.Columns.Count == 0)
            {
                throw new ArgumentException("Invalid table name or columns");
            }

            if (!TableExists(tableDefinition.TableName))
            {
                string createTableSql = BuildCreateTableSql(tableDefinition);
                using (var con = new SqliteConnection(_connectionString))
                {
                    con.Open();
                    using var command = new SqliteCommand(createTableSql, con);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string BuildCreateTableSql(TableDefinition tableDefinition)
        {
            var columnDefinitions = new List<string>();
            foreach (var column in tableDefinition.Columns)
            {
                string columnDefinition = $"{column.Name} {column.DataType}";
                if (column.IsPrimaryKey)
                {
                    columnDefinition += " PRIMARY KEY";
                }
                if (!column.IsNullable)
                {
                    columnDefinition += " NOT NULL";
                }
                columnDefinitions.Add(columnDefinition);
            }
            string columnsSql = string.Join(", ", columnDefinitions);
            return $"CREATE TABLE {tableDefinition.TableName} ({columnsSql}); {tableDefinition.Constraint};";
        }

        public bool TableExists(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            }

            string sql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";

            using (var con = new SqliteConnection(_connectionString))
            {
                con.Open();
                using (var command = new SqliteCommand(sql, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts rows into the table
        /// </summary>
        /// <param name="tableDefinition"></param>
        /// <exception cref="ArgumentException"></exception>
        public void InsertRows(TableDefinition tableDefinition)
        {
            if (tableDefinition == null || string.IsNullOrEmpty(tableDefinition.TableName) || tableDefinition.Columns == null || tableDefinition.Columns.Count == 0)
            {
                throw new ArgumentException("Invalid table definition");
            }

            if (tableDefinition.Rows == null || tableDefinition.Rows.Count == 0)
            {
                throw new ArgumentException("Invalid rows data");
            }

            if (TableExists(tableDefinition.TableName))
            {
                foreach (var row in tableDefinition.Rows)
                {
                    string insertSql = BuildInsertSql(tableDefinition, row);
                    using (var con = new SqliteConnection(_connectionString))
                    {
                        con.Open();
                        using var command = new SqliteCommand(insertSql, con);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private string BuildInsertSql(TableDefinition tableDefinition, Dictionary<string, object> columnValues)
        {
            var columns = new List<string>();
            var values = new List<string>();

            foreach (var column in tableDefinition.Columns)
            {
                if (columnValues.ContainsKey(column.Name))
                {
                    columns.Add(column.Name);
                    values.Add(FormatValue(columnValues[column.Name], column.DataType));
                }
            }

            string columnsSql = string.Join(", ", columns);
            string valuesSql = string.Join(", ", values);
            return $"INSERT OR IGNORE INTO {tableDefinition.TableName} ({columnsSql}) VALUES ({valuesSql});";
        }

        private static string FormatValue(object value, string dataType)
        {
            if (value == null)
            {
                return "NULL";
            }

            switch (dataType.ToUpper())
            {
                case "TEXT":
                case "VARCHAR":
                case "CHAR":
                    return $"'{value.ToString().Replace("'", "''")}'";
                case "INTEGER":
                case "NUMERIC":
                case "REAL":
                    return value.ToString();
                default:
                    throw new ArgumentException($"Unsupported data type: {dataType}");
            }
        }
    }
}
