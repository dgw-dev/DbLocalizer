using Entities.DAL;
using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingExportUtility : ExportUtilityBase, IExportUtility
    {
        protected ISmartlingExportFileProcessor _fileProcessor = null;
        private readonly ISmartlingConfiguration _smartlingConfiguration;
        public Dictionary<string, SmartlingExportFile> ExportPackage = new Dictionary<string, SmartlingExportFile>();
        public List<SmartlingExportFile> ExportFiles { get; set; }
        public SmartlingJobDetails JobDetails { get; set; }
        

        public SmartlingExportUtility(
            ISmartlingExportFileProcessor fileProcessor,
            IFileDataService fileDataService,
            IExportDal exportDal,
            AppSettings appSettings,
            IConfiguration config,
            ISmartlingConfiguration _smartlingConfig,
            ILogger logger = null,
            Guid processId = default(Guid))
            : base(fileDataService, exportDal, appSettings, config, logger, processId)
        {
            CacheManager = fileProcessor.CacheManager;
            _fileProcessor = fileProcessor;
            _fileProcessor.ProcessId = processId;
            _smartlingConfiguration = _smartlingConfig;
            JobDetails = new SmartlingJobDetails(_smartlingConfiguration.DbLocalizerImportEndPoint);
        }
    

        protected override async Task ProcessExport(CancellationToken ct, List<string> culturesOverride = null, Database db = null)
        {
            if (IsCancelled(ct))
            {
                return;
            }

            string message;
            SmartlingAppData dtmData = null;

            try
            {
                string packageId = Guid.NewGuid().ToString();

                dtmData = _fileProcessor.ProcessExportData<SmartlingAppData>(ExportLookbackInDays, ExportMaxTablesPerFile, MaxRowsPerFile, ct, db);

                if (dtmData.FileData != null && dtmData.FileData.Tables.Count > 0)
                {
                    dtmData.DatabaseName = db.Name;
                    dtmData.FileData.GlobalizationMetaData.PackageId = packageId;
                    dtmData.FileData.GlobalizationMetaData.ExportType = _fileProcessor.ExportType;
                    dtmData.FileData.GlobalizationMetaData.DatabaseName = db.Name;
                    dtmData.FileData.GlobalizationMetaData.Locale = db.SourceLocale;
                    dtmData.FileData.GlobalizationMetaData.FileId = Guid.NewGuid().ToString();

                    await BuildPackageAsync(dtmData, packageId, ct);

                }
                else
                {
                    message = _fileProcessor.ExportType + ": No export data";
                    LogMessage(message);
                    OperationComplete = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                LogMessage(message, true);
                _fileProcessor.ErrorList.Add(new ExportErrorEntry() { ProcessId = ProcessId, Message = ex.Message, Stacktrace = ex.StackTrace });
            }
        }

        public async Task BuildPackageAsync(SmartlingAppData appData, string packageId, CancellationToken ct)
        {
            string message = string.Empty;

            if (appData.FileData.GlobalizationMetaData != null && appData.FileData.Tables != null)
            {
                AddDtmPackage(ref ExportPackage, appData.FileData, packageId);

                //do we have a package to send 
                if (ExportPackage.Count > 0)
                {
                    ITranslationManager tmsPlugin = new SmartlingPlugin(_fileProcessor, _smartlingConfiguration, _logger, CacheManager);
                    OperationComplete = await tmsPlugin.TMSOperations<SmartlingExportFile>(ExportPackage, appData?.FileData?.GlobalizationMetaData, packageId, ct, null, Cultures, ProcessId);

                    //if export failed
                    if (!OperationComplete)
                    {
                        message = _fileProcessor.ExportType + ": Export Failed";
                        LogMessage(message, true);
                    }
                }
                else
                {
                    message = _fileProcessor.ExportType + ": No packages to export";
                    LogMessage(message);
                }
            }
        }

        public Dictionary<string, SmartlingExportFile> BuildDtmFileDataCollection(SmartlingFileData dtmCultureData, int exportMaxTablesPerFile, int maxRowsPerFile)
        {
            //A dictionary to hold all sub files per file
            Dictionary<string, SmartlingExportFile> subFiles = new Dictionary<string, SmartlingExportFile>();

            //A dictionary to hold all sub data per file
            Dictionary<string, SmartlingFileData> subData = new Dictionary<string, SmartlingFileData>();

            //first we need to divide the data based on the exportMaxTablesPerFile value and then the maxRowsPerFile value
            IEnumerable<IEnumerable<Table>> chunkTables = dtmCultureData?.Tables?.AsEnumerable().ToChunks(exportMaxTablesPerFile)
                  .Select(rows => rows.ToList());

            int tableListCount = 0;
            if ((bool)chunkTables?.Any())
            {
                foreach (IEnumerable<Table> tableList in chunkTables)
                {
                    SmartlingFileData subFile = new SmartlingFileData();
                    subFile.smartling = dtmCultureData.smartling;
                    subFile.GlobalizationMetaData = dtmCultureData.GlobalizationMetaData;
                    subFile.Tables = new List<Table>();
                    foreach (Table table in tableList)
                    {
                        IEnumerable<IEnumerable<Row>> chunkRows = table.Rows?.AsEnumerable().ToChunks(maxRowsPerFile)
                              .Select(rows => rows.ToHashSet());

                        if (chunkRows != null)
                        {
                            //create a new file for each chunk of rows
                            foreach (IEnumerable<Row> chunk in chunkRows)
                            {

                                Table subTable = new Table()
                                {
                                    PrimaryKeyName = table.PrimaryKeyName,
                                    BaseTable = table.BaseTable,
                                    FullBaseTableName = table.FullBaseTableName,
                                    LocalizedTable = table.LocalizedTable,
                                    FullLocalizedTableName = table.FullLocalizedTableName,
                                    TableSchema = table.TableSchema,
                                    SystemColumns = table.SystemColumns,
                                    SourceLanguageColumns = table.SourceLanguageColumns,
                                    TranslatedLanguageColumns = table.TranslatedLanguageColumns,

                                    Rows = chunk as HashSet<Row>,
                                };
                                subFile.Tables.Add(subTable);
                            }
                        }
                    }
                    subData.Add(subFile.GlobalizationMetaData.PackageId + "_" + dtmCultureData.GlobalizationMetaData.Locale + "_" + tableListCount.ToString(), subFile);
                    tableListCount++;
                }

                //now we need to create ExportFile objects for each sub data object
                if (subData?.Count > 0)
                {
                    foreach (KeyValuePair<string, SmartlingFileData> fileCollection in subData)
                    {
                        if (fileCollection.Key == subData.Last().Key) { dtmCultureData.GlobalizationMetaData.LastFile = true; }
                        subFiles.Add(fileCollection.Key, AddFile(fileCollection.Key, fileCollection.Value, dtmCultureData.GlobalizationMetaData));
                    }
                }
            }

            return subFiles;
        }

        public virtual void AddDtmPackage(ref Dictionary<string, SmartlingExportFile> dtmPackage, SmartlingFileData fileData, string packageId)
        {
            try
            {
                Dictionary<string, SmartlingExportFile> subFiles = BuildDtmFileDataCollection(fileData, ExportMaxTablesPerFile, MaxRowsPerFile);

                if (subFiles?.Count > 0)
                {
                    foreach (KeyValuePair<string, SmartlingExportFile> subFile in subFiles)
                    {
                        dtmPackage.Add(subFile.Key, subFile.Value);
                    }
                }
                else
                {
                    dtmPackage.Add(packageId, new SmartlingExportFile(fileData, fileData.GlobalizationMetaData));
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message, true);
                _fileProcessor.ErrorList.Add(new ExportErrorEntry(ProcessId, null, ex.Message, ex.StackTrace));
                Abort();
            }
        }

        public static SmartlingExportFile AddFile(string fileName, SmartlingFileData jobData, MetaData metaData)
        {
            SmartlingExportFile exportFile = new SmartlingExportFile(jobData, metaData);
            exportFile.FileName = fileName + ".json";
            return exportFile;
        }
    }
}
