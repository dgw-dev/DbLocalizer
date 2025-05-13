using Entities.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingImportUtility : ISmartlingImportUtility
    {
        private readonly ILogger _logger;
        private readonly ISmartlingImportFileProcessor _fileProcessor;
        protected bool OperationComplete { get; set; } = false;

        public SmartlingImportUtility(
            AppSettings appSettings,
            IFileDataService fileDataService,
            ISmartlingImportFileProcessor fileProcessor,
            ILogger<SmartlingImportUtility> logger)
        {
            _fileProcessor = fileProcessor;
            _logger = logger;
        }

        public async Task<SmartlingImportSqlFilePackageCollection> Import(SmartlingImportJsonFileCollection collection)
        {
            var importPackageCollection = new SmartlingImportSqlFilePackageCollection();

            OperationComplete = true;
            foreach (SmartlingImportJsonFile importFile in collection.JsonFiles)
            {
                try
                {
                    SmartlingImportSqlFilePackage importSqlPackage = await _fileProcessor.GetCultureContentPackage(importFile);
                    importPackageCollection.AddPackage(importSqlPackage);
                }
                catch (Exception ex)
                {
                    OperationComplete = false;
                    _fileProcessor.ErrorList.Add(new ImportErrorEntry(ex.Message,
                        importFile.ProcessId,
                        importFile.PackageId,
                        importFile.FileId,
                        importFile.FileName,
                        ex.StackTrace));

                    _logger.LogError(ex, ex.Message);
                }
            }

            return importPackageCollection;
        }
    }
}
