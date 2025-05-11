namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingAuthorization
    {
        public string userIdentifier { get; set; }
        public string userSecret { get; set; }
        public bool AutoAuthorize { get; set; } = false;
    }
}
