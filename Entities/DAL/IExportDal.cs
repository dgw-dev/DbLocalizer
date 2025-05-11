using System.Data;

namespace Entities.DAL
{
    public interface IExportDal
    {
        DataTable GetTableDataToExport(string tableName, Database db);
    }
}