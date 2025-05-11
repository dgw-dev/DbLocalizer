
using Entities.BL;
using Entities.DAL;
using Entities.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static Entities.DtmFileData;

namespace Entities.Utilities
{
    public class ExportRuleEngine : IRuleEngine
    {
        private readonly Dictionary<string, ExportRule> _rules = new Dictionary<string, ExportRule>();
        private readonly IRulesBasedExportData _ruleData;
        private readonly RulesBL _rulesBL;
        private  List<ExportRule> _rulesList;

        public ExportRuleEngine(IRulesBasedExportData ruleData)
        {
            _ruleData = ruleData;
            //_rulesBL = new RulesBL(dal);
            //_rulesList = Task.Run(() => _rulesBL.GetRuleListAsync()).Result;
            LoadRules();
        }

        /// <summary>
        /// Load the rules from the db
        /// </summary>
        private void LoadRules()
        {
            ExportQueryCollection exportQueries = new ExportQueryCollection();

            if ((bool)(exportQueries.Queries?.Any()))
            {
                foreach (var queryList in exportQueries.Queries)
                {
                    //get the rule definition from the db by export type
                    if(_rulesList != null && _rulesList.Any())
                    {
                        var rule = _rulesList.FirstOrDefault(x => x.ExportType == queryList.Key);
                        if (rule != null)
                        {
                            rule.ExportType = queryList.Key;
                            rule.QueryList = queryList.Value;
                            _rules[rule.ExportType] = rule;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if the rule exists by export type
        /// </summary>
        /// <param name="exportType"></param>
        /// <returns></returns>
        public bool RuleExists(string exportType)
        {
            if (_rules.TryGetValue(exportType, out _))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the rule options by export type
        /// </summary>
        /// <param name="supportedAppId"></param>
        /// <param name="exportType"></param>
        /// <returns></returns>
        public ExportRule GetRule()
        {
            if(_ruleData == null || _ruleData?.ExportType == null)
            {
                return null;
            }
            if (_rules.TryGetValue(_ruleData.ExportType, out var rule))
            {
                //setup any default values
                //switch on the export type
                switch (_ruleData.ExportType)
                {
                    case "Nvarchar":
                        //we have default tables for type
                        if (_ruleData.TablesArray == null || _ruleData.TablesArray.Count == 0)
                        {
                            if (_ruleData.TablesArray == null)
                            {
                                _ruleData.TablesArray = new List<string>();
                            }

                            ////List<NvarcharTable> tables = Task.Run(() => _dal.GetNvarcharListAsync()).Result;
                            //for(int i = 0; i < tables.Count; i++)
                            //{
                            //    _ruleData.TablesArray.Add(tables[i].FullTableName);
                            //}
                        }
                        break;
                    case "CultureDescription":
                        //we have default tables for type
                        if (_ruleData.TablesArray == null)
                        {
                            _ruleData.TablesArray = new List<string>();
                        }
                        _ruleData.TablesArray.Add("[dbo].[Culture]");

                        break;
                }

                rule.TablesArray = _ruleData.TablesArray;
                rule.CulturesArray = _ruleData.CulturesArray;
            }

            return rule;
        }

        /// <summary>
        /// Get the table batch rule by export type and query type
        /// </summary>
        /// <param name="exportType"></param>
        /// <param name="queryType"></param>
        /// <param name="tableChunk"></param>
        /// <returns></returns>
        public string GetQueryByQueryType(IExportRule rule, QueryType queryType, DataTable tableChunk)
        {
            SqlUtility sqlUtility = new SqlUtility();

            //create the base table temp table
            string tables = sqlUtility.CreateBaseTableChunkTable(tableChunk);
            List<SqlParameter> parameters = new List<SqlParameter>();
            Dictionary<string, string> placeholders = new Dictionary<string, string>()
            {
                //Add the placeholder values
                { "0", tables },
            };


            #region Editable Region

            //switch on the export type
            switch (rule.ExportType)
            {
                case "Nvarchar":
                    //add any additional placeholder and params
                    //Example usage:
                    //parameters.Add(new SqlParameter("@CultureTableName", SqlDbType.NVarChar) { Value = tableToExport.FullCultureTableName });
                    break;
                case "CultureDescription":
                    //add any additional placeholder and params
                    //Example usage:
                    //parameters.Add(new SqlParameter("@CultureTableName", SqlDbType.NVarChar) { Value = tableToExport.FullCultureTableName });
                    break;
            }

            #endregion

            //Params and Plaeholders will be injected into the query
            SetRuleDependencies(ref rule, queryType, placeholders, parameters);

            return rule.GetMergedSqlString(queryType);
        }

        /// <summary>
        /// Get the table values rule by export type and query type
        /// </summary>
        /// <param name="exportType"></param>
        /// <param name="queryType"></param>
        /// <param name="tableToExport"></param>
        /// <returns></returns>
        public string GetQueryByQueryType(IExportRule rule, QueryType queryType, ExportTable tableToExport)
        {
            string colList = SqlUtility.GetColumns(tableToExport, false, "b.");
            List<SqlParameter> parameters = new List<SqlParameter>();
            Dictionary<string, string> placeholders = new Dictionary<string, string>()
            {
                //Add default the placeholder values
                { "0", tableToExport.TableName },
                { "1", tableToExport.TableSchema },
                { "2", colList },
                { "3", tableToExport.FullTableName },
                { "4", tableToExport.PrimaryKey },
                { "5", tableToExport.TableName + "Culture" },
                { "6", tableToExport.FullDestinationTableName },
            };

            #region Editable Region

            if (!string.IsNullOrEmpty(colList))
            {
                //switch on the export type
                switch (rule.ExportType)
                {
                    case "Nvarchar":
                        //add any additional placeholder and params
                        //Example usage:
                        //parameters.Add(new SqlParameter("@CultureTableName", SqlDbType.NVarChar) { Value = tableToExport.FullCultureTableName });
                        break;
                    case "CultureDescription":
                        //add any additional placeholder and params
                        //Example usage:
                        //parameters.Add(new SqlParameter("@CultureTableName", SqlDbType.NVarChar) { Value = tableToExport.FullCultureTableName });
                        break;
                }
            }

            #endregion

            SetRuleDependencies(ref rule, queryType, placeholders, parameters);

            //Params and Plaeholders will be injected into the query
            return rule.GetMergedSqlString(queryType);
        }

        private static void SetRuleDependencies(ref IExportRule rule, QueryType queryType, Dictionary<string, string> placeHolders, List<SqlParameter> parameters = null) 
        {
            if (placeHolders?.Count > 0) 
            {
                rule.QueryList.FirstOrDefault(x => x.QueryType == queryType).Dependencies.PlaceholderValues = placeHolders;
            }
            if (parameters?.Count > 0)
            {
                rule.QueryList.FirstOrDefault(x => x.QueryType == queryType).Dependencies.Parameters = parameters;
            }
        }
    }
}
