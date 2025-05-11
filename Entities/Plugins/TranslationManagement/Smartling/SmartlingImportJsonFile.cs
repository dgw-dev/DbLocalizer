using System;
using System.Collections.Generic;
using System.Linq;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingImportJsonFile
    {
        public SmartlingFileData ImportFileData { get; init; }
        public string FileName { get; init; }
        public string SmartlingLocale { get; init; }
        public Guid ProcessId { get; init; }
        public string Locale => ImportFileData.GlobalizationMetaData.Locale;

        public Guid PackageId
        {
            get
            {
                Guid result = Guid.Empty;

                if (ImportFileData?.GlobalizationMetaData?.PackageId is not null)
                {
                    result = new Guid(ImportFileData.GlobalizationMetaData.PackageId);
                }

                return result;
            }
        }

        public Guid FileId
        {
            get
            {
                Guid result = Guid.Empty;

                if (ImportFileData?.GlobalizationMetaData?.FileId is not null)
                {
                    result = new Guid(ImportFileData.GlobalizationMetaData.FileId);
                }

                return result;
            }
        }

        public SmartlingImportJsonFile(SmartlingFileData incomingFile, string fileName, string locale, Guid processId)
        {
            ImportFileData = incomingFile;
            FileName = fileName;
            SmartlingLocale = locale;
            ProcessId = processId;

            // Update metadata locale
            incomingFile.GlobalizationMetaData.Locale = locale;

        }

        public SmartlingImportJsonFile(SmartlingFileData incomingFile, string fileName, Guid processId)
        {
            ImportFileData = incomingFile;
            FileName = fileName;
            ProcessId = processId;
        }
    }

    public class SmartlingImportJsonFileCollection
    {
        public IReadOnlyList<SmartlingImportJsonFile> JsonFiles { get; init; }
        public Guid ProcessId { get; init; }
        public Guid PackageId { get; init; }

        public SmartlingImportJsonFileCollection(IEnumerable<SmartlingImportJsonFile> jsonFiles)
        {
            JsonFiles = jsonFiles.OrderBy(json => json.SmartlingLocale).ToList();
            ProcessId = JsonFiles.FirstOrDefault()?.ProcessId ?? Guid.Empty;
            PackageId = JsonFiles.FirstOrDefault()?.PackageId ?? Guid.Empty;
        }

        public SmartlingImportJsonFileCollection(SmartlingImportJsonFile jsonFile) : this([jsonFile])
        {
            // just one file in the collection
        }
    }
}
