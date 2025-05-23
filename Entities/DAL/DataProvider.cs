using Entities.Interfaces;
using Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.DAL
{
    public class DataProvider : IDataProvider
    {
        private readonly IEncryptionService _encryptionService;
        public DataProvider(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public DataTable GetTables(string connectionString, string tableName, string sql = default)
        {
            DataTable tables = new DataTable(tableName);

            SqlConnection con = new SqlConnection(_encryptionService.Decrypt(connectionString));
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 3600;

            using (con)
            {
                con.Open();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tables);
                }
            }

            return tables;
        }

        public async Task<DataTable> GetTabeSchema(string connectionString, string tableName)
        {
            DataTable tables = new DataTable(tableName);

            SqlConnection con = new SqlConnection(_encryptionService.Decrypt(connectionString));
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + tableName + " WHERE 1 = 0", con);
            cmd.CommandTimeout = 3600;

            using (con)
            {
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.KeyInfo))
                {
                    tables = await reader.GetSchemaTableAsync();
                }
            }

            return tables;
        }

        public async Task<int> ExecuteNonQueryAsync(string connectionString, string sql)
        {
            using var con = new SqlConnection(_encryptionService.Decrypt(connectionString));
            var cmd = new SqlCommand(sql, con);
            int resultCount = 0;

            try
            {
                using (con)
                {
                    await con.OpenAsync();
                    resultCount = await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception) 
            {
                resultCount = -1;
            }

            return resultCount;
        }
    }
}
