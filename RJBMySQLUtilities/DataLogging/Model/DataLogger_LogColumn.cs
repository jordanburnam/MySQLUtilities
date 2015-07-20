using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using  RJBMySQLUtilities.DataLogging.Types;
namespace RJBMySQLUtilities.DataLogging.DataLogging.Model
{
    /// <summary>
    /// This class is for the columns used on the logging table. 
    /// This is an incomplete version and will later add functionality for 
    /// dynamic columns that will generate the MySql code based on the types
    /// I will need a mapp of all the DB types and possible outcomes along with the .Net Equivalent.
    /// It would not be hard but just time consuming
    /// </summary>
     class DataLogger_LogColumn
    {
        private string _ColumnName;
        
        private MySql.Data.MySqlClient.MySqlDbType _oMySqlDBType;
        private LogColumn.DefinitionType _eDefinitionType; 
        private string _sMySqlDBType;
        private int _DataTypeLength;
        private bool _Nullable;
        private string _Key;
        private string _Default;
        private string _Extras;
        private string _SchemaType;
        private string _SchemaDefinition;
        public string SchemaType
        {
            get {
                if (this._eDefinitionType == LogColumn.DefinitionType.Dynamic)
                {
                    return this._oMySqlDBType.ToString().ToUpper().Replace("INT32", "INT(11)").Replace("INT64", "BIGINT(20)") + " " + (this._DataTypeLength > 0 ? "(" + this._DataTypeLength + ")" : "") + " " + (this._Nullable ? "NULL" : "NOT NULL") + " " + this._Key + " " + this._Extras + this._Default;
                }
                else
                {
                    return this._SchemaType; 
                }
                }
        }
        public string SchemaName
        {
            get { return this._ColumnName  + (this._LogType == null ? "" : "_" + this._LogType.ToString().ToLower()); }
        }
        public string SchemaDefinition
        {
            get {
                if (this._eDefinitionType == LogColumn.DefinitionType.Dynamic)
                {
                    return this.SchemaName + " " + this.SchemaType;
                }
                else
                {
                    return this._SchemaDefinition;
                }
            
                }
        }
        LogColumn.LogType? _LogType; 

        

        public DataLogger_LogColumn(string sColumnName, MySql.Data.MySqlClient.MySqlDbType eMySqlType, int iDataTypeLength, bool bNullable, string sKey, string sDefault, string sExtras, RJBMySQLUtilities.DataLogging.Types.LogColumn.LogType? eLogType)
        {
            this._eDefinitionType = LogColumn.DefinitionType.Dynamic;
            this._ColumnName = sColumnName;
            this._oMySqlDBType = eMySqlType;
            this._DataTypeLength = iDataTypeLength;
            this._Nullable = bNullable;
            this._Key = sKey;
            this._Default = sDefault;
            this._LogType = eLogType;
            this._Extras = sExtras;
        }

        public DataLogger_LogColumn(string sColumnName, string sSchemaDefinition, RJBMySQLUtilities.DataLogging.Types.LogColumn.LogType? eLogType)
        {
            this._eDefinitionType = LogColumn.DefinitionType.Static;
            this._LogType = eLogType;
            this._ColumnName = sColumnName;
            this._SchemaDefinition = this.SchemaName + " " + sSchemaDefinition;
        }
    }
}
