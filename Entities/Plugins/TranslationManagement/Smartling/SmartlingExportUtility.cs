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
using Entities;
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

                dtmData = _fileProcessor.ProcessExportData<SmartlingAppData>(ExportLookbackInDays, ExportMaxTablesPerFile, MaxRowsPerTable, ct, db);

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

        public static Dictionary<string, SmartlingExportFile> BuildDtmFileDataCollection(SmartlingFileData dtmCultureData, int exportMaxTablesPerFile, int maxRowsPerTable)
        {
            if (dtmCultureData != null && dtmCultureData.Tables != null && dtmCultureData.Tables.Any())
            {
                //A dictionary to hold all sub files per file
                Dictionary<string, SmartlingExportFile> subFiles = new Dictionary<string, SmartlingExportFile>();

                //A dictionary to hold all row chunks per table file
                Dictionary<string, List<HashSet<Row>>> tabelRowChunks = new Dictionary<string, List<HashSet<Row>>>();

                int tableCount = 0;
                foreach (Table table in dtmCultureData.Tables)
                {
                    //Get rows based on the max number of rows we're allowed per table
                    List<HashSet<Row>> chunkRows = SplitRowList<Row>(table.Rows, maxRowsPerTable);

                    if (chunkRows != null)
                    {
                        foreach (IEnumerable<Row> chunk in chunkRows)
                        {
                            List<HashSet<Row>> rows = new List<HashSet<Row>>() { chunk as HashSet<Row> };

                            if (!tabelRowChunks.TryGetValue(table.FullBaseTableName, out List<HashSet<Row>> _))
                            {
                                tabelRowChunks.Add(table.FullBaseTableName, rows);
                            }
                            else
                            {
                                tabelRowChunks[table.FullBaseTableName].AddRange(rows);
                            }
                        }
                    }
                    tableCount++;
                }

                //split data into sub data objects
                Dictionary<string, SmartlingFileData> subData = GetSubData(dtmCultureData, tabelRowChunks);

                //now we need to create ExportFile objects for each sub data object
                if (subData?.Count > 0)
                {
                    foreach (KeyValuePair<string, SmartlingFileData> fileCollection in subData)
                    {
                        if (fileCollection.Key == subData.Last().Key) { dtmCultureData.GlobalizationMetaData.LastFile = true; }
                        subFiles.Add(fileCollection.Key, AddFile(fileCollection.Key, fileCollection.Value, dtmCultureData.GlobalizationMetaData));
                    }
                }

                return subFiles;
            }

            return new Dictionary<string, SmartlingExportFile>();
        }

        public virtual void AddDtmPackage(ref Dictionary<string, SmartlingExportFile> exportPackage, SmartlingFileData fileData, string packageId)
        {
            try
            {
                Dictionary<string, SmartlingExportFile> subFiles = BuildDtmFileDataCollection(fileData, ExportMaxTablesPerFile, MaxRowsPerTable);

                if (subFiles?.Count > 0)
                {
                    foreach (KeyValuePair<string, SmartlingExportFile> subFile in subFiles)
                    {
                        exportPackage.Add(subFile.Key, subFile.Value);
                    }
                }
                else
                {
                    fileData.GlobalizationMetaData.LastFile = true;
                    exportPackage.Add(packageId, new SmartlingExportFile(fileData, fileData.GlobalizationMetaData));
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

        private static Dictionary<string, SmartlingFileData> GetSubData(SmartlingFileData dtmCultureData, Dictionary<string, List<HashSet<Row>>> tabelRowChunks)
        {
            // Find the entry with the largest number of HashSet<Row> in its value list
            var maxEntry = tabelRowChunks
                .OrderByDescending(kvp => kvp.Value?.Count ?? 0)
                .FirstOrDefault();

            //we need to know how many files we will need to create for each table
            int totalFilesNeeded = maxEntry.Value?.Count ?? 0;

            Dictionary<string, SmartlingFileData> subData = new Dictionary<string, SmartlingFileData>();

            int fileCount = 0;
            
            while (fileCount < totalFilesNeeded)
            {
                SmartlingFileData subDataFile = new SmartlingFileData();
                subDataFile.Tables = new List<Table>();
                subDataFile.smartling = dtmCultureData.smartling;
                subDataFile.GlobalizationMetaData = dtmCultureData.GlobalizationMetaData;
                string fileName = $"{dtmCultureData.GlobalizationMetaData.PackageId}_{fileCount}";

                foreach (Table table in dtmCultureData.Tables)
                {
                    var rowSet = tabelRowChunks.FirstOrDefault(l => l.Key == table.FullBaseTableName).Value[fileCount];
                    if (rowSet != null && rowSet.Count > fileCount)
                    {
                        // Create a new table object for each chunk
                        Table newTable = new Table()
                        {
                            Rows = rowSet,
                            FullBaseTableName = table.FullBaseTableName,
                            LocalizedTable = table.LocalizedTable,
                            PrimaryKeyName = table.PrimaryKeyName,
                            SystemColumns = table.SystemColumns,
                            SourceLanguageColumns = table.SourceLanguageColumns,
                            TranslatedLanguageColumns = table.TranslatedLanguageColumns
                        };
                        subDataFile.Tables.Add(newTable);
                    }
                }
                subData.Add(fileName, subDataFile);
                fileCount++;
            }

            return subData;
        }

        private static List<HashSet<T>> SplitRowList<T>(HashSet<T> list, int chunkSize)
        {
            var chunks = new List<HashSet<T>>();
            if (list == null || chunkSize <= 0)
                return chunks;

            var array = list.ToArray();
            for (int i = 0; i < array.Length; i += chunkSize)
            {
                var chunk = array.Skip(i).Take(chunkSize);
                chunks.Add(new HashSet<T>(chunk));
            }
            return chunks;
        }
    }
}
