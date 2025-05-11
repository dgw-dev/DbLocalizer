using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Entities.Utilities
{
    public class StoredProcedureHelper : IStoredProcedureHelper
    {
        private readonly string _connectionString;
        private readonly string _storedProcedureName;
        public bool TestMode { get; set; } = false;


        public StoredProcedureHelper(string connectionString, string storedProcedureName)
        {
            _connectionString = connectionString;
            _storedProcedureName = storedProcedureName;
        }

        /// <summary>
        /// Performs Sql non-query operations, accepting a SqlParameter array
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);
            int resultCount = 0;

            using (con)
            {
                con.Open();
                resultCount = cmd.ExecuteNonQuery();
            }

            return resultCount;
        }

        /// <summary>
        /// Asynchronously performs Sql non-query operations, accepting a SqlParameter array
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);
            int resultCount = 0;

            using (con)
            {
                await con.OpenAsync();
                resultCount = await cmd.ExecuteNonQueryAsync();
            }

            return resultCount;
        }

        /// <summary>
        /// Binds SqlDataReader result to callback function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindSqlReaderFunction"></param>
        /// <returns></returns>
        public List<T> ExecuteReaderBindResults<T>(Func<SqlDataReader, T> bindSqlReaderFunction)
        {
            return ExecuteReaderBindResults(Array.Empty<SqlParameter>(), bindSqlReaderFunction);
        }

        /// <summary>
        /// Binds SqlDataReader result to callback function, accepting a SqlParameter array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlParameters"></param>
        /// <param name="bindSqlReaderFunction"></param>
        /// <returns></returns>
        public List<T> ExecuteReaderBindResults<T>(SqlParameter[] sqlParameters, Func<SqlDataReader, T> bindSqlReaderFunction)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(sqlParameters);
            var results = new List<T>();

            using (con)
            {
                con.Open();
                using var reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    results.Add(bindSqlReaderFunction(reader));
                }
            }

            return results;
        }

        /// <summary>
        /// Asynchronously binds SqlDataReader result to callback function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindSqlReaderFunction"></param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteReaderBindResultsAsync<T>(Func<SqlDataReader, T> bindSqlReaderFunction)
        {
            return await ExecuteReaderBindResultsAsync(Array.Empty<SqlParameter>(), bindSqlReaderFunction);
        }

        /// <summary>
        /// Asynchronously binds SqlDataReader result to callback function, accepting a SqlParameter array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlParameters"></param>
        /// <param name="bindSqlReaderFunction"></param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteReaderBindResultsAsync<T>(SqlParameter[] sqlParameters, Func<SqlDataReader, T> bindSqlReaderFunction)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(sqlParameters);

            var results = new List<T>();

            using (con)
            {
                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();


                while (await reader.ReadAsync())
                {
                    results.Add(bindSqlReaderFunction(reader));
                }
            }

            return results;
        }

        public int ExecuteScalar(SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);
            int resultCount = 0;

            using (con)
            {
                con.Open();
                resultCount = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return resultCount;
        }

        public async Task<int> ExecuteScalarAsync(SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);
            int resultCount = 0;

            using (con)
            {
                await con.OpenAsync();
                resultCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            return resultCount;
        }

        public decimal ExecuteDecimalScalar(SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);
            decimal resultCount = 0;

            using (con)
            {
                con.Open();
                resultCount = Convert.ToDecimal(cmd.ExecuteScalar());
            }

            return resultCount;
        }

        public async Task<decimal> ExecuteDecimalScalarAsync(SqlParameter[] parameters)
        {
            using var con = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(_storedProcedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);
            decimal resultCount = 0;

            using (con)
            {
                await con.OpenAsync();
                resultCount = Convert.ToDecimal(await cmd.ExecuteScalarAsync());
            }

            return resultCount;
        }
    }
}
