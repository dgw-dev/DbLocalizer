namespace Entities.Plugins.TranslationManagement.Smartling
{
    public interface ISmartlingConfiguration
    {
        SmartlingEndPoints EndPoints { get; set; }
        SmartlingAuthorization Authorization { get; set; }
        string WorkflowUid { get; set; }
        bool AutoAuthorize { get; set; }
        Smartling smartling { get; set; }
        string DbLocalizerExportEndPoint { get; set; }
        string DbLocalizerImportEndPoint { get; set; }
    }
}
