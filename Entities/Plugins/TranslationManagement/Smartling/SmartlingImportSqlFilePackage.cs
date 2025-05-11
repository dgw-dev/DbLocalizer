using System;
using System.Text;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingImportSqlFilePackage
    {
        public class ImportSqlFileContent
        {
            public string FileName { get; init; }
            public string FileContent { get; init; }
            public Encoding Encoding { get; init; }
        }

        public MetaData MetaData => ImportJsonFile.ImportFileData.GlobalizationMetaData;
        public Guid ProcessId => ImportJsonFile.ProcessId;
        public Guid PackageId => Guid.Parse(MetaData.PackageId);

        public SmartlingImportJsonFile ImportJsonFile { get; }

        public ImportSqlFileContent ImportSqlFile { get; }

        public SmartlingImportSqlFilePackage(SmartlingImportJsonFile importJsonFile, string sqlFileContent)
        {
            if (importJsonFile.ImportFileData.GlobalizationMetaData is null)
            {
                throw new ArgumentException("Import JSON file GlobalizationMetaData cannot be null");
            }

            ImportJsonFile = importJsonFile;

            string fileName = ImportJsonFile.FileId + "_" + ImportJsonFile.Locale + ".sql";

            ImportSqlFile = new ImportSqlFileContent
            {
                FileName = fileName,
                FileContent = sqlFileContent,
                Encoding = new UTF8Encoding(true)
            };
        }
    }
}
