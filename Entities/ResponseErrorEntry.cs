namespace Entities
{
    public class ResponseErrorEntry
    {
        public string PackageId { get; set; }
        public string ReasonPhrase { get; set; }
        public string StatusCode { get; set; }

        public ResponseErrorEntry() { }
    }
}
