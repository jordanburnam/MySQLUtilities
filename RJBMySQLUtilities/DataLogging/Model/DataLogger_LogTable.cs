using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using RJBMySQLUtilities.DataLogging.Types;


namespace RJBMySQLUtilities.DataLogging.DataLogging.Model
{
    class DataLogger_LogTable
    {
        private string _DestinationTableName;
        private string _SourceTableName;
        private string _SourceDBName;
        private string _DestinationDBName;
        private Dictionary<int, DataLogger_LogColumn> _DestinationColumns;

        private DataTable _SourceSchema;
        private string _SourceTablePrimaryKey;
        /// <summary>
        /// Will create the columns needed to log the table described by the dtSourceSchema
        /// </summary>
        /// <param name="sSouceTable">The name of the table that will be logged from including the database name.</param>
        /// <param name="sDestinationTable">The name of the table that will be logged to including the database name.</param>
        /// <param name="dtSourceSchema">This is a datatable with the same schema that would be returned by the 'descrbe' command in MySql except with a NullToString Colum set to NULL OR NOT NULL based on the Null column from the describe command.</param>
        public DataLogger_LogTable(string sSouceTable, string sDestinationTable, string sSourceDBName, string sDestinationDBName,  DataTable dtSourceSchema)
        {
            this._SourceTableName = sSouceTable;
            this._DestinationTableName = sDestinationTable;
            this._SourceDBName = sSourceDBName;
            this._DestinationDBName = sDestinationDBName;
            this._DestinationColumns = new Dictionary<int, DataLogger_LogColumn>();
            this._SourceSchema = dtSourceSchema; 
            DataLogger_LogColumn oLogColumn_old;
            DataLogger_LogColumn oLogColumn_new;
            int iCurrentColumnCount = 0;
            int iTotalNumberOfColumnsToLog = dtSourceSchema.Rows.Count;
            foreach (DataRow dr in dtSourceSchema.Rows)
            {
                oLogColumn_old = new DataLogger_LogColumn(dr["Field"].ToString(), dr["Type"].ToString() + " " + dr["NullToString"].ToString() + " ", LogColumn.LogType.Old);
                oLogColumn_new = new DataLogger_LogColumn(dr["Field"].ToString(), dr["Type"].ToString() + " " + dr["NullToString"].ToString() + " ", LogColumn.LogType.New);
                this._DestinationColumns.Add(iCurrentColumnCount, oLogColumn_old);
                this._DestinationColumns.Add(iCurrentColumnCount + iTotalNumberOfColumnsToLog, oLogColumn_new);//I want the old columns to be ordinally next to one naother in the db
                if (dr["Key"] != DBNull.Value)
                {
                    if (dr["Key"].ToString().ToUpper().Equals("PRI"))
                    { 
                        this._SourceTablePrimaryKey = dr["Field"].ToString(); 
                    }
                }
                iCurrentColumnCount++;
            }
        }

        public string GetTableDefinition(string sIDentityColumnName = "LogID")
        {
           
            string sIdentityColumnDefinition = Environment.NewLine + string.Format("`{0}` BIGINT NOT NULL AUTO_INCREMENT COMMENT 'Primary Key For this table'", sIDentityColumnName);
            string sColumnDefinitions = GetColumnDefinitions();
            string sPrimaryKeyColumnDefinition = string.Format(",PRIMARY KEY (`{0}`)  COMMENT ''", sIDentityColumnName);
            string sUniqueIndexColumnDefintiion = Environment.NewLine + string.Format(",UNIQUE INDEX `{0}_UNIQUE` (`{0}` ASC)  COMMENT ''", sIDentityColumnName);

            string sTableDefintion = string.Format("CREATE TABLE IF NOT EXISTS {0} ({1} {2} {3} {4})" + Environment.NewLine + "ENGINE = InnoDB;", this._DestinationTableName, sIdentityColumnDefinition, sColumnDefinitions, sPrimaryKeyColumnDefinition, sUniqueIndexColumnDefintiion);

            return sTableDefintion;
        }

        public string GetSourceTableTrigger(string sTriggerName)
        {
            string sSourceTableTrigger = "delimiter ////" + Environment.NewLine +
                                         "DROP TRIGGER IF EXISTS {0}////" + Environment.NewLine +
                                         "CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW" + Environment.NewLine +
                                         "BEGIN" + Environment.NewLine +
                                         "INSERT INTO {2}(" + Environment.NewLine +
                                         "{3}" + Environment.NewLine +
                                         ")" + Environment.NewLine +
                                        "SELECT " + Environment.NewLine +
                                        "{4}" + Environment.NewLine +
                                        "FROM inserted AS A" + Environment.NewLine +
                                        "INNER JOIN deleted AS B ON B.{5} = A.{5};" + Environment.NewLine +
                                        "END;" + Environment.NewLine;

            
            string sSourceTableName = "`" + this._SourceDBName + "`" + "." + "`" + this._SourceTableName + "`"; //{1} SourceTableName
            string sLogTableName =  this._DestinationTableName; //{2} DestinationTableName
            string sLogTableColumns = GetColumnNames(); //{3} Columns in Log Table
            string sColumnChangesFromSource = GetSourceColumsOldAndNew("A", "B"); //{4} The inserted and deleted columns from SourceTable
            string sPrimaryKeyID = this._SourceTablePrimaryKey; //{5} the primary key from source table

            sSourceTableTrigger = string.Format(sSourceTableTrigger, sTriggerName, sSourceTableName, sLogTableName, sLogTableColumns, sColumnChangesFromSource, sPrimaryKeyID);
            return sSourceTableTrigger;
        }
        private string GetColumnDefinitions()
        {
            string sColumnDefinitions = "" + Environment.NewLine;
            foreach (DataLogger_LogColumn oLogColumn in this._DestinationColumns.Values)
            {
                sColumnDefinitions += "," + oLogColumn.SchemaDefinition + Environment.NewLine;
            }
            return sColumnDefinitions;
        }

        private string GetColumnNames()
        {
            string sColumnNames = "" + Environment.NewLine;
            int iCurrentColumnCount = 0;
            foreach (DataLogger_LogColumn oLogColumn in this._DestinationColumns.Values)
            {
                sColumnNames += (iCurrentColumnCount > 0 ? ",": "") + oLogColumn.SchemaName + Environment.NewLine;
                iCurrentColumnCount++;
            }
            
            return sColumnNames;
        }
        private string GetLogingColumnDefinitions()
        {
            string sLogColumnDefintiions = "";
            sLogColumnDefintiions =", " +  "CreatedWhen DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP() COMMENT ''"  + Environment.NewLine;
            return sLogColumnDefintiions;
        }

        private string GetSourceColumsOldAndNew(string sInsertedAlias, string sDeletedAlias)
        {
            string sColumnsOld = "";
            string sColumnNew = "";
            int iCurrentColumnCount = 0;

            foreach (DataRow dr in this._SourceSchema.Rows)
            {
                sColumnsOld += (iCurrentColumnCount > 0 ? "," : "") + sDeletedAlias + "." + dr["Field"].ToString() + Environment.NewLine;
                sColumnNew += (iCurrentColumnCount > 0 ? "," : "") + sInsertedAlias + "." + dr["Field"].ToString() + Environment.NewLine;
                iCurrentColumnCount++;
            }

            return sColumnsOld + "," + sColumnNew;
        }
  
    }
}
