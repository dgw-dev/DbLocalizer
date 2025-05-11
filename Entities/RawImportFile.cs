using Microsoft.AspNetCore.Http;

namespace Entities
{
    public class RawImportFile
    {
        public IFormFile File { get; set; }
        public string Locale { get; set; }
    }
}
