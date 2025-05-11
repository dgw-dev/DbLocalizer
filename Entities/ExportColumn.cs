

namespace Entities
{
    public class ExportColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Length { get; set; }
        public string IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsExportable { get; set; }
    }
}
