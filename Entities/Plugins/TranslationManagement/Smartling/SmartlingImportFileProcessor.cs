using Entities.BL;
using Entities.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingImportFileProcessor : FileProcessorBase, ISmartlingImportFileProcessor
    {
        public new List<ImportErrorEntry> ErrorList { get; set; }



        public SmartlingImportFileProcessor(ILogger<SmartlingImportFileProcessor> logger,
            ICacheManager cacheManager,
            ISqlSchemaBuilder sqlSchemaBuilder) : base(logger, cacheManager, sqlSchemaBuilder)
        {

            ErrorList = new List<ImportErrorEntry>();
        }

        public async Task<SmartlingImportSqlFilePackage> GetCultureContentPackage(SmartlingImportJsonFile importFile)
        {
            try
            {
                // Generate the import SQL
                var sqlFileProcessor = new SmartlingSqlProcessor();
                string importSql = sqlFileProcessor.BuildSqlString(importFile.ImportFileData.Tables, importFile.Locale);

                // Create the import package
                var package = new SmartlingImportSqlFilePackage(importFile, importSql);

                return package;
            }
            catch (Exception ex)
            {
                ErrorList.Add(new ImportErrorEntry(ex.Message, new Guid(importFile.ImportFileData.GlobalizationMetaData.PackageId), ProcessId, new Guid(importFile.ImportFileData.GlobalizationMetaData.FileId), importFile.FileName));
                throw;
            }
        }
    }
}
