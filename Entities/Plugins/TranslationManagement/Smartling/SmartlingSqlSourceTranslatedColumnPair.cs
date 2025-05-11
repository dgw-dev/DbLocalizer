using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public record SmartlingSqlSourceTranslatedColumnPair(SourceLanguageColumn Source, TranslatedLanguageColumn Translated)
    {
        public readonly string LangTempBase = $"{Source.ColumnName}Base";
        public readonly string LangTempCulture = $"{Translated.ColumnName}Culture";
    }
}
