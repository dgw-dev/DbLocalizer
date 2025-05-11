
using Entities.Interfaces;
using Entities.Plugins.TranslationManagement.Smartling;
using Entities.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Entities.BL
{
    public abstract class FileProcessorBase
    {
        public Guid ProcessId { get; set; }
        protected ILogger _logger;
        public ICacheManager CacheManager { get; set; }
        protected readonly ISqlSchemaBuilder _sqlSchemaBuilder;

        public List<ExportErrorEntry> ErrorList { get; set; } = new List<ExportErrorEntry>();

        protected FileProcessorBase(ILogger logger, ICacheManager cacheManager, ISqlSchemaBuilder sqlSchemaBuilder)
        {
            _logger = logger;
            CacheManager = cacheManager;
            _sqlSchemaBuilder = sqlSchemaBuilder;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="metaData"></param>
        public static void AddMetaData(ref MultipartFormDataContent content, MetaData metaData)
        {
            StringContent databaseName = new StringContent(metaData.DatabaseName ?? string.Empty, Encoding.UTF8);
            content.Add(databaseName, "appName");
            StringContent locale = new StringContent(metaData.Locale ?? string.Empty, Encoding.UTF8);
            content.Add(locale, "locale");
            StringContent packageId = new StringContent(metaData.PackageId ?? string.Empty, Encoding.UTF8);
            content.Add(packageId, "packageId");
        }

        /// <summary>
        /// Creates the file as a stream
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected StreamContent CreateFileContent(string fileContent, string fileName, string contentType, Encoding encoding)
        {
            MemoryStream fileStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(fileStream, encoding);
            writer.Write(fileContent);
            writer.Flush();
            fileStream.Position = 0;

            StreamContent streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = fileName,
            };
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return streamContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static string ConvertFromSqlDataType(string dataType)
        {
            if (dataType.ToLower().Contains("nvarchar"))
            {
                return "System.String";
            }
            else if (dataType.ToLower().Contains("decimal"))
            {
                return "System.Decimal";
            }
            else
            {
                switch (dataType)
                {
                    case "int":
                        return "System.Int32";
                    case "bit":
                        return "System.Boolean";
                    case "datetime":
                        return "System.DateTime";
                    case "uniqueidentifier":
                        return "System.Guid";
                    default:
                        return "System.String";
                }
            }

        }

        public void PopulateExportTables(DataRow topLevelTableRowData, DataTable tableRowData, Dictionary<string, ExportTable> exportTables, List<ExportColumn> columns)
        {
            ExportTable exportTable = null;
            try
            {
                if (!exportTables.TryGetValue((string)topLevelTableRowData["FullTableName"], out exportTable))
                {
                    //build columns
                    List<TableColumn> tableColumns = new List<TableColumn>();
                    foreach (ExportColumn ec in columns)
                    {
                        if (ec.IsExportable)
                        {
                            tableColumns.Add(new TableColumn()
                            {
                                ColumnName = ec.Name,
                                ColumnDataType = ec.Type,
                                ColumnLength = ec.Length,
                                IsSourceColumn = ec.IsExportable
                            });
                        }
                    }

                    //build rows
                    List<ExportTableRow> tableRows = new List<ExportTableRow>();
                    foreach (DataRow row in tableRowData.Rows)
                    {
                        ExportTableRow exportTableRow = new ExportTableRow()
                        {
                            TableRowData = row
                        };
                        tableRows.Add(exportTableRow);
                    }

                    exportTable = new ExportTable(
                        (string)topLevelTableRowData["FullTableName"],
                        (string)topLevelTableRowData["TableName"],
                        columns.FirstOrDefault(x => x.IsPrimaryKey).Name,
                        tableColumns,
                        tableRows,
                        (string)topLevelTableRowData["TableSchema"],
                        (string)topLevelTableRowData["FullCultureTableName"]);

                    exportTables.Add((string)topLevelTableRowData["FullTableName"], exportTable);
                }
            }
            catch (Exception ex)
            {
                ErrorList.Add(new ExportErrorEntry(ProcessId, null, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public static List<List<string>> SplitList(List<string> allBasetableNames, int exportMaxTablesPerFile)
        {
            List<List<string>> splitLists = new List<List<string>>();

            for (int i = 0; i < allBasetableNames.Count; i += exportMaxTablesPerFile)
            {
                splitLists.Add(allBasetableNames.GetRange(i, Math.Min(exportMaxTablesPerFile, allBasetableNames.Count - i)));
            }

            return splitLists;
        }

        public static DataTable GetBaseTableSubset(DataTable allBaseTables, List<string> tables)
        {
            DataTable result = null;
            if (tables?.Count > 0)
            {
                //Only get the tables we need
                result = allBaseTables.Clone();
                foreach (string tableName in tables)
                {
                    DataRow tableRow = allBaseTables.AsEnumerable().Where(x => x.Field<string>("FullTableName") == tableName).FirstOrDefault();
                    if (tableRow != null)
                    {
                        result.ImportRow(tableRow);
                    }
                }
            }
            else
            {
                //do all tables
                result = allBaseTables.Clone();
                foreach (DataRow tableRow in allBaseTables.Rows)
                {
                    result.ImportRow(tableRow);
                }
            }

            return result;
        }

        public void AddFileContent(MultipartFormDataContent content, string fileName, string sqlFileContent, List<SmartlingLocaleId> localeIds, string callBackUrl)
        {
            //add to outputcontent
            StreamContent streamContent = CreateFileContent(sqlFileContent, fileName, "application/json", new UTF8Encoding(false));
            content.Add(streamContent, "file");
            content.Add(new StringContent(fileName ?? string.Empty), "fileUri");
            content.Add(new StringContent("json" ?? string.Empty), "fileType");
            foreach (SmartlingLocaleId locale in localeIds)
            {
                content.Add(new StringContent(locale.targetLocaleId ?? string.Empty), "localeIdsToAuthorize[]");
            }
            content.Add(new StringContent(callBackUrl ?? string.Empty), "callbackUrl");
        }

        public bool IsCancelled(CancellationToken ct)
        {
            if (ct.IsCancellationRequested || !TokenStore.Store.TryGetValue(ProcessId, out CancellationTokenSource _))
            {
                return true;
            }

            return false;
        }

        public void Abort()
        {
            if (TokenStore.Store.TryGetValue(ProcessId, out CancellationTokenSource source))
            {
                source.Cancel();
                TokenStore.Store.Remove(ProcessId);
            }
        }
    }
}
