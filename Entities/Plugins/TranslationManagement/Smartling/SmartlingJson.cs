using System.Collections.Generic;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingJson
    {
        public required Smartling smartling { get; set; }

        //public SmartlingJson()
        //{
        //    smartling = GlobalConfig.GetInstance().AppSettings.Smartling;
        //}
    }

    public class Smartling
    {
        public string entity_escaping { get; set; }
        public string variants_enabled { get; set; }
        public List<TranslatePath> translate_paths { get; set; }
    }

    public class TranslatePath
    {
        public string path { get; set; }
        public string[] key { get; set; }
        public string character_limit { get; set; }
        public string exclude_path { get; set; }
        public string instruction { get; set; }
    }
}
