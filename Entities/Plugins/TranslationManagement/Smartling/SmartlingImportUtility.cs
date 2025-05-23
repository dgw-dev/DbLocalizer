using Entities.DAL;
using Entities.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingImportUtility : ISmartlingImportUtility
    {
        private readonly ILogger _logger;
        private readonly ISmartlingImportFileProcessor _fileProcessor;
        private readonly IImportDal _importDal1;

        private readonly ICacheManager _cacheManager;
        public bool OperationComplete { get; set; } = false;
        public List<ImportErrorEntry> ErrorList { get; set; }

        public SmartlingImportUtility(
            AppSettings appSettings,
            IFileDataService fileDataService,
            ISmartlingImportFileProcessor fileProcessor,
            ILogger<SmartlingImportUtility> logger,
            IImportDal importDal)
        {
            _fileProcessor = fileProcessor;
            _cacheManager = fileProcessor.CacheManager;
            _logger = logger;
            _importDal1 = importDal;
        }

        public async Task<SmartlingImportSqlFilePackageCollection> Import(SmartlingImportJsonFileCollection collection)
        {
            var importPackageCollection = new SmartlingImportSqlFilePackageCollection();

            try
            {
                Databases databases = _cacheManager.GetCacheValue("Databases") as Databases;

                StringBuilder importString = new StringBuilder();

                foreach (SmartlingImportJsonFile importFile in collection.JsonFiles)
                {                  
                    SmartlingImportSqlFilePackage importSqlPackage = await _fileProcessor.GetCultureContentPackage(importFile);
                    
                    if (databases != null && databases.DatabaseCollection?.Count > 0 && !string.IsNullOrEmpty(importSqlPackage.ImportSqlFile.FileContent)) 
                    {
                        importString.Append(importSqlPackage.ImportSqlFile.FileContent);
                        importString.AppendLine();

                        //save the import to the database
                        int result = await _importDal1.ImportData(databases.DatabaseCollection
                        .FirstOrDefault(d => d.Name == importFile.ImportFileData.GlobalizationMetaData.DatabaseName)
                        .ConnectionStringValue,
                        importString.ToString());

                        if (result == -1) 
                        {
                            _fileProcessor.ErrorList.Add(new ImportErrorEntry("Import failed " + importSqlPackage.ImportSqlFile.FileName,
                                collection.ProcessId,
                                collection.PackageId,
                                new Guid(importFile.ImportFileData.GlobalizationMetaData.FileId),
                                importSqlPackage.ImportSqlFile.FileName,
                                string.Empty));
                        }
                        else
                        {
                            importString.Clear();
                        }
                    }
                    OperationComplete = true;

                    //it may not be neccessary to add the importSqlPackage to the collection
                    //however, leaving it in just in case there is an alternative implementation required 
                    //other than just building the import sql string and executing it
                    importPackageCollection.AddPackage(importSqlPackage);
                }
            }
            catch(Exception ex)
            {
                OperationComplete = false;
                _fileProcessor.ErrorList.Add(new ImportErrorEntry(ex.Message,
                    collection.ProcessId,
                    collection.PackageId,
                    Guid.Empty,
                    string.Empty,
                    ex.StackTrace));
                _logger.LogError(ex, ex.Message);
            }

            ErrorList = _fileProcessor.ErrorList;

            return importPackageCollection;
        }
    }
}
