
using System.Data;
using System.Threading.Tasks;

namespace Entities.Interfaces
{
    public interface IDataProvider
    {
        DataTable GetTables(string connectionString, string tableName, string sql = default);
        Task<DataTable> GetTabeSchema(string connectionString, string tableName);
        Task<int> ExecuteNonQueryAsync(string connectionString, string sql);
    }
}
