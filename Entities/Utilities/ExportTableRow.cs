using System.Data;

namespace Entities.Utilities
{
    public class ExportTableRow
    {
        public bool IsUpdate { get; set; }
        public DataRow TableRowData { get; set; }
    }
}
