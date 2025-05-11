using Entities.Utilities;
using System;
using System.Text;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingExportFile
    {
        public SmartlingFileData ExportData { get; set; }
        public string JsonOutput { get; set; }
        public decimal ProjectedFileSizeInMegabytes { get; set; }
        public MetaData MetaData { get; set; }
        public string FileName { get; set; }
        public string DatabaseName { get; set; }

        public SmartlingExportFile()
        {

        }

        public SmartlingExportFile(SmartlingFileData dataToExport, MetaData metaData)
        {
            ExportData = dataToExport;
            MetaData = metaData;

            if (ExportData != null && MetaData != null)
            {
                ExportData.GlobalizationMetaData = metaData;
                SerializeJson();
            }
        }

        public void SerializeJson()
        {
            JsonOutput = JsonUtility.SerializeData<SmartlingFileData>(ExportData);
            ProjectedFileSizeInMegabytes = Math.Round(((decimal)Encoding.UTF8.GetByteCount(JsonOutput) / 1024 / 1024), 3);
        }
    }
}
