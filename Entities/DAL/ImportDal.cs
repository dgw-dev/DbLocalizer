using Entities.Interfaces;
using System.Threading.Tasks;

namespace Entities.DAL
{
    public class ImportDal : IImportDal
    {
        private readonly IDataProvider _dataProvider;
        public ImportDal(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public async Task<int> ImportData(string connectionString, string sql)
        {
            return await _dataProvider.ExecuteNonQueryAsync(connectionString, sql);
        }
    }
}
