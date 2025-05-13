using Entities.BL;
using Entities.DAL;
using Entities.Interfaces;
using Entities.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading;
using static Entities.Plugins.TranslationManagement.Smartling.SmartlingFileData;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingExportFileProcessor : FileProcessorBase, ISmartlingExportFileProcessor
    {
        public Dictionary<string, List<DataSet>> DatabaseUpdatesForApp { get; set; }
        private readonly ISmartlingConfiguration _smartlingConfiguration;
        private readonly IExportDal _exportDal;
        public string ExportType { get; set; }

        public SmartlingExportFileProcessor(
            ILogger logger,
            IExportDal exportDal,
            string exportType,
            ICacheManager cacheManager,
            ISqlSchemaBuilder schemaBuilder,
            ISmartlingConfiguration smartlingConfig) : base(logger, cacheManager, schemaBuilder)
        {
            ErrorList = new List<ExportErrorEntry>();
            DatabaseUpdatesForApp = new Dictionary<string, List<DataSet>>();
            _logger = logger;
            ExportType = exportType;
            _exportDal = exportDal;
            _smartlingConfiguration = smartlingConfig;
        }

        public T ProcessExportData<T>(string exportLookbackInDays, int exportMaxTablesPerFile, int maxRowsPerFile, CancellationToken ct, Database db)
        {
            Dictionary<string, ExportTable> exportTables = new Dictionary<string, ExportTable>();
            Dictionary<string, ExportTable> allTablesToExport = new Dictionary<string, ExportTable>();
            SmartlingAppData appData = new SmartlingAppData();
            List<string> allBaseTables = db?.Tables;
            double totalNumberOfTables = 0;

            try
            {
                SmartlingFileData dataToExport = null;

                if (allBaseTables != null && allBaseTables.Count > 0)
                {
                    IExportData export = null;

                    totalNumberOfTables = Convert.ToDouble(allBaseTables.Count);

                    export = new DefaultExport(_logger, ProcessId, _exportDal, "Default");

                    //do we have tables
                    if (totalNumberOfTables > 0)
                    {
                        //create the chunks of tables we will iterate over
                        List<List<string>> basetableChunks = SplitList(allBaseTables, exportMaxTablesPerFile);

                        //main processing logic
                        ProcessExportTableChunks(basetableChunks, export, ct, ref exportTables, ref allTablesToExport, exportLookbackInDays, db);

                        //If we have tables to export then add them to the FileData object
                        if (allTablesToExport.Count > 0)
                        {
                            dataToExport = AddTableData(allTablesToExport, db);
                            appData.FileData = dataToExport;
                        }
                    }
                }
            }
            catch (Exception)
            {
                Abort();
                throw;
            }

            // Ensure the return type matches the generic type T
            if (appData is T result)
            {
                return result;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast AppData to type {typeof(T).Name}");
            }
        }

        public SmartlingFileData AddTableData(Dictionary<string, ExportTable> allTablesToExport, Database db)
        {
            SmartlingFileData data = CreateExportFileData();

            try
            {
                if (data != null && allTablesToExport != null)
                {
                    foreach (KeyValuePair<string, ExportTable> exportTable in allTablesToExport)
                    {
                        Table DtmTable = new Table()
                        {
                            PrimaryKeyName = exportTable.Value.PrimaryKey,
                            BaseTable = exportTable.Value.TableName,
                            FullBaseTableName = exportTable.Value.FullTableName,
                            LocalizedTable = exportTable.Value.TableName + db.LocalizedTableSuffix,
                            SourceLanguageColumns = new List<SourceLanguageColumn>(),
                            TranslatedLanguageColumns = new List<TranslatedLanguageColumn>(),
                            Rows = new HashSet<Row>(),
                            TableSchema = exportTable.Value.TableSchema,
                            FullLocalizedTableName = exportTable.Value.FullLocalizedTableName,
                        };

                        foreach (TableColumn col in exportTable.Value?.Columns)
                        {
                            if (col.IsSourceColumn)
                            {
                                DtmTable.SourceLanguageColumns.Add(new SourceLanguageColumn()
                                {
                                    ColumnName = col.ColumnName,
                                    MaxLength = (col.ColumnLength == 0) ? 1024 : col.ColumnLength,
                                    DataType = Type.GetType(ConvertFromSqlDataType(col.ColumnDataType)).ToString(),
                                    TranslatedLanguageColumnName = col.ColumnName
                                });
                            }
                            else
                            {
                                DtmTable.TranslatedLanguageColumns.Add(new TranslatedLanguageColumn()
                                {
                                    ColumnName = col.ColumnName,
                                    MaxLength = (col.ColumnLength == 0) ? 1024 : col.ColumnLength,
                                    DataType = Type.GetType(ConvertFromSqlDataType(col.ColumnDataType)).ToString(),
                                    SourceLanguageColumnName = col.ColumnName
                                });
                            }
                        }

                        //add rows
                        foreach (ExportTableRow row in exportTable.Value?.Rows)
                        {
                            List<Source> source = new List<Source>();

                            foreach (TableColumn col in exportTable.Value?.Columns)
                            {
                                bool columnFound = row.TableRowData.Table.Columns.Cast<DataColumn>().FirstOrDefault(c => c.ColumnName == col.ColumnName) != null;

                                if (columnFound && col.IsSourceColumn)
                                {
                                    source.Add(new Source()
                                    {
                                        BaseTable = exportTable.Value.TableName,
                                        LocalizedTable = exportTable.Value.TableName + db.LocalizedTableSuffix,
                                        MaxLength = (col.ColumnLength == 0) ? 1024 : col.ColumnLength,
                                        Column = col.ColumnName,
                                        Text = row.TableRowData[col.ColumnName]
                                    });
                                }
                            }

                            DtmTable.Rows.Add(new Row()
                            {
                                Source = source,
                                IsUpdate = row.IsUpdate,
                                PrimaryKeyValue = row.TableRowData[exportTable.Value.PrimaryKey].ToString(),
                            });
                        }

                        data.Tables.Add(DtmTable);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorList.Add(new ExportErrorEntry(ProcessId, null, ex.Message, ex.StackTrace));
                throw;
            }

            return data;
        }

        public MultipartFormDataContent GetCultureContent(SmartlingExportFile culturePackage, List<SmartlingLocaleId> localeIds, string callbackUrl = default(string))
        {
            MultipartFormDataContent content = null;
            content = new MultipartFormDataContent();
            content.Headers.ContentType.MediaType = "multipart/form-data";

            try
            {

                AddFileContent(content, culturePackage.FileName, culturePackage.JsonOutput, localeIds, callbackUrl);

            }
            catch (Exception ex)
            {
                ErrorList.Add(new ExportErrorEntry(ProcessId, null, ex.Message, ex.StackTrace));
                throw;
            }
            return content;
        }

        public SmartlingFileData CreateExportFileData()
        {
            SmartlingFileData data = new SmartlingFileData();
            data.smartling = _smartlingConfiguration.smartling;
            data.GlobalizationMetaData = new MetaData();
            data.Tables = new List<Table>();

            return data;
        }

        private void ProcessExportTableChunks(List<List<string>> basetableChunks, IExportData export, CancellationToken ct, ref Dictionary<string, ExportTable> exportTables, ref Dictionary<string, ExportTable> allTablesToExport, string exportLookbackInDays, Database db)
        {
            int chunkCount = 1;
            foreach (List<string> baseTableChunk in basetableChunks)
            {
                foreach (string table in baseTableChunk)
                {
                    if (IsCancelled(ct))
                    {
                        return;
                    }

                    export.BaseTableChunk = baseTableChunk;
                    export.ExportLookbackInDays = exportLookbackInDays;

                    DataTable tableRowData = _exportDal.GetTableDataToExport(table, db);

                    if (tableRowData != null && tableRowData.Rows.Count > 0)
                    {
                        //get the top level table date
                        DataRow topLevelTableRowData = null;
                        foreach (DataTable baseTable in db.ExportTables.Tables)
                        {
                            topLevelTableRowData = baseTable.AsEnumerable().FirstOrDefault(x => x.Field<string>("FullTableName") == table);
                        }

                        //get columns for this table
                        List<ExportColumn> columns = db.GetTableColumns(table);

                        if (columns != null && columns.Count > 0 && columns.Any(c => c.IsExportable))
                        {
                            PopulateExportTables(topLevelTableRowData, tableRowData, exportTables, columns);

                            if (exportTables.Count > 0)
                            {
                                export.ExportTable = exportTables[table];

                                if (!allTablesToExport.TryGetValue(topLevelTableRowData["FullTableName"].ToString(), out _))
                                {
                                    allTablesToExport.Add(topLevelTableRowData["FullTableName"].ToString(), exportTables[table]);
                                }
                            }
                        }
                    }
                }

                chunkCount++;
            }
        }
    }
}
