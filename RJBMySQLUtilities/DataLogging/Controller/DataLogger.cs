using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RJBMySQLUtilities.DataLogging.DataLogging.Model;

namespace RJBMySQLUtilities.DataLogging.DataLogging.Controller
{
    public class DataLogger
    {
        private string _HistoryTableSuffix;
        private string _HistoryTablePrefix;
        private string _csSource;
        private string _csDestination;
        private MySqlUtility _oMySqlUtility_Source;
        private MySqlUtility _oMySqlUtility_Destination;
        private Dictionary<string, string> _DataChangeControlColumns;

        public string HistoryTableSuffix
        {
            get { return this._HistoryTableSuffix; }
            set { this._HistoryTableSuffix = value;  }
        }

        public string HistoryTablePrefix
        {
            get { return this._HistoryTablePrefix; }
            set { this._HistoryTablePrefix = value; }
        }

        #region Constructors
        public DataLogger(string csSource, string csDestination)
        {

            this._csSource = csSource;
            this._csDestination = csDestination;
            this._HistoryTablePrefix = "";
            this._HistoryTableSuffix = "_Log";
            this._DataChangeControlColumns = new Dictionary<string, string>();

            try
            {
                this._oMySqlUtility_Source = new MySqlUtility(csSource);
            }
            catch (Exception ex)
            {
                throw new Exception("Error when trying to create the source connection", ex);
            }

            try
            {
                this._oMySqlUtility_Destination = new MySqlUtility(csDestination);
            }
            catch (Exception ex)
            {
                throw new Exception("Error when trying to create the destination connection", ex);
            }

        }

        public DataLogger(string csSource, string csDestination, string sHistoryTableSchema, string sHistoryTableSuffix)
            : this(csSource, csDestination)
        {
            this._HistoryTablePrefix = sHistoryTableSchema;
            this._HistoryTableSuffix = sHistoryTableSuffix;
        }
        #endregion

        #region Methods
        public string GetLogTriggerForTable(string sSourceTableName)
        {
            string sLogTrigger = "";
            sSourceTableName = this._oMySqlUtility_Source.GetTableNameOnly(sSourceTableName);
            string sDestinationTableName = this._HistoryTablePrefix + sSourceTableName + this._HistoryTableSuffix;
            DataTable dt = GetTableSchema(sSourceTableName);
            sDestinationTableName = this._oMySqlUtility_Destination.AddSchemaToTableName(sDestinationTableName);
            DataLogger_LogTable oLogTable = new DataLogger_LogTable(sSourceTableName, sDestinationTableName, this._oMySqlUtility_Source.GetDBNameFromConnection(), this._oMySqlUtility_Destination.GetDBNameFromConnection(), dt);
            sLogTrigger = oLogTable.GetSourceTableTrigger(sSourceTableName + "_BEFORE_UPDATE");

            return sLogTrigger;
        }

        public string GenerateLogSchemaFromSource()
        {
            string sQuery = ""; 
            sQuery += GenerateLogTablesQuery() + Environment.NewLine;
            sQuery += GenerateLogTriggersQuery() + Environment.NewLine;

            return sQuery;

        }

        public string GetLogSchemaForTable(string sSourceTableName)
        {
            sSourceTableName = this._oMySqlUtility_Source.GetTableNameOnly(sSourceTableName);
            string sLogSchema = "";
            string sDestinationTableName = this._HistoryTablePrefix + sSourceTableName + this._HistoryTableSuffix;
            DataTable dt = GetTableSchema(sSourceTableName);
            sDestinationTableName = this._oMySqlUtility_Destination.AddSchemaToTableName(sDestinationTableName);
            DataLogger_LogTable oLogTable = new DataLogger_LogTable(sSourceTableName, sDestinationTableName, this._oMySqlUtility_Source.GetDBNameFromConnection(), this._oMySqlUtility_Destination.GetDBNameFromConnection(), dt);
            sLogSchema = oLogTable.GetTableDefinition();

            return sLogSchema;
        }

        private string GenerateLogTablesQuery()
        {
            string sQuery = ""; 
            string[] sTables = GetTablesToLogFromSource();
            foreach (string sTable in sTables)
            {
                sQuery += GetLogSchemaForTable(sTable) + Environment.NewLine;
            }

            return sQuery;
        }

        private string GenerateLogTriggersQuery()
        {
            string sQuery = "";
            string[] sTables = GetTablesToLogFromSource();
            foreach (string sTable in sTables)
            {
                sQuery += GetLogTriggerForTable(sTable) + Environment.NewLine;
            }

            return sQuery;
        }
        public void GenerateAndRunLogSchemaQuery()
        {
            string sTableQuery = GenerateLogTablesQuery();
            string sTriggerQuery = GenerateLogTriggersQuery();
            sTriggerQuery = sTriggerQuery.Replace("\r\n", " ").Replace("delimiter //", "");
            sTriggerQuery = "delimiter //" + Environment.NewLine + sTriggerQuery;
            int iRows = 0; 
            iRows += this._oMySqlUtility_Destination.GetRowsAffectedWithQuery(sTableQuery);
            iRows += this._oMySqlUtility_Source.GetRowsAffectedWithQuery(sTriggerQuery);


        }
        private string[] GetTablesToLogFromSource()
        {
           string[] sTables = null;
            string sQuery = string.Format("SHOW TABLES IN `{0}`;", this._oMySqlUtility_Source.GetDBNameFromConnection());
            DataTable dt = this._oMySqlUtility_Source.GetTableWithQuery(sQuery);
            if(dt.Rows.Count > 0)
            {
                sTables = new string[dt.Rows.Count];
                for(int i = 0; i < dt.Rows.Count; i++)
                {
                    sTables[i] = dt.Rows[i][0].ToString();
                }
            }
            return sTables;
        }

        private DataTable GetTableSchema(string sTableName)
        {
            DataTable dt = null;
            string sQuery = string.Format("describe {0}", sTableName);
            dt = this._oMySqlUtility_Source.GetTableWithQuery(sQuery);
            if (dt.Columns.Contains("Null"))
            {
                DataColumn dcNullToString = new DataColumn("NullToString", typeof(string), "IIF(Null = true, 'NULL', 'NOT NULL')");
                dt.Columns.Add(dcNullToString);
            }
            return dt; 
        }
        #endregion
    }
}
