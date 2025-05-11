namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingSettings
    {
        public SmartlingEndPoints SmartlingEndPoints { get; set; }
        public SmartlingAuthorization SmartlingAuthorization { get; set; }
        public string SmartlingWorkflowUid { get; set; }
        public Smartling smartling { get; set; }
        public string DbLocalizerExportEndPoint { get; set; }
        public string DbLocalizerImportEndPoint { get; set; }
    }
}
