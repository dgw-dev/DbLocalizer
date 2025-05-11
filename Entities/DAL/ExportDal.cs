using Entities.Interfaces;
using Entities.Utilities;
using System.Data;

namespace Entities.DAL
{
    public class ExportDal : IExportDal
    {
        private readonly IDataProvider _dataProvider;
        public ExportDal(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public DataTable GetTableDataToExport(string tableName, Database db)
        {
            string sql = SqlUtility.SqlGetTableDataToExport(tableName, db);
            return _dataProvider.GetTables(db.ConnectionStringValue, tableName, sql);
        }
    }
}
