using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Entities.Utilities
{
    public interface IStoredProcedureHelper
    {
        decimal ExecuteDecimalScalar(SqlParameter[] parameters);
        Task<decimal> ExecuteDecimalScalarAsync(SqlParameter[] parameters);
        int ExecuteNonQuery(SqlParameter[] parameters);
        Task<int> ExecuteNonQueryAsync(SqlParameter[] parameters);
        List<T> ExecuteReaderBindResults<T>(Func<SqlDataReader, T> bindSqlReaderFunction);
        List<T> ExecuteReaderBindResults<T>(SqlParameter[] sqlParameters, Func<SqlDataReader, T> bindSqlReaderFunction);
        Task<List<T>> ExecuteReaderBindResultsAsync<T>(Func<SqlDataReader, T> bindSqlReaderFunction);
        Task<List<T>> ExecuteReaderBindResultsAsync<T>(SqlParameter[] sqlParameters, Func<SqlDataReader, T> bindSqlReaderFunction);
        int ExecuteScalar(SqlParameter[] parameters);
        Task<int> ExecuteScalarAsync(SqlParameter[] parameters);
    }
}