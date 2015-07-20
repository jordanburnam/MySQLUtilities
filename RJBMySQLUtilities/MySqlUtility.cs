using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using System.Data;
using MySql.Data.MySqlClient;
using RJBMySQLUtilities.DataLogging.Helpers;


namespace RJBMySQLUtilities.DataLogging
{
    /// <summary>
    /// This class will be used to hold the connection object
    /// and have the resposibility of talking to the DB for the rest of the 
    /// dll
    /// </summary>
    public class MySqlUtility
    {
        private   string csConnectionString = "";
        private   MySqlConnection oMySqlConnection;
        public MySqlUtility(string csMySqlDB)
        {
            try
            {
                this.csConnectionString = csMySqlDB;

                oMySqlConnection = new MySqlConnection(csMySqlDB);

                //Make sure we can connect
                oMySqlConnection.Open();
                oMySqlConnection.Close();

            }
            catch (Exception ex)
            {
                throw new Exception("Error throw when trying to generate MySQL connection using the supplied connection string. See Inner Exception for details", ex);
            }
            finally
            {
                if (oMySqlConnection != null)
                {
                    if (oMySqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        oMySqlConnection.Close();
                    }
                }
            }

        }

        public DataTable GetTableWithQuery(string sQuery)
        {
           DataTable dt = null;
            try
            {
               
                MySqlConnection oMySqlConnection = this.oMySqlConnection;
                MySqlDataAdapter oAdapter = new MySqlDataAdapter(sQuery, oMySqlConnection);
                DataSet ds = new DataSet();
                oAdapter.Fill(ds);
                if (ds != null)
                {
                    if (ds.Tables.Count == 1)
                    {
                        dt = ds.Tables[0];
                    }
                    else
                    {
                        throw new Exception("The command returned " + (ds.Tables.Count > 1 ? "more than one table" : "zero tables") + ". This method requires that one table and only one table be returned from the command." + Environment.NewLine);
                    }
                }
                else
                {
                    throw new Exception("The passed in DataSet to the MySqlDataAdapter returned a DataSet that was null after it executed the command. There must of been an error in the command even though no exception was thrown." + Environment.NewLine);
                }

               
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }
    
        public DataSet GeTablesWithQuery(string sQuery)
        {
            DataSet ds = null;
            try
            {
                
                MySqlConnection oMySqlConnection = this.oMySqlConnection;
                MySqlDataAdapter oAdapter = new MySqlDataAdapter(sQuery, oMySqlConnection);
                DataSet dsTemp = new DataSet();
                oAdapter.Fill(dsTemp);
                if (ds != null)
                {
                    if (dsTemp.Tables.Count > 1)
                    {
                        ds = dsTemp;
                    }
                    else
                    {
                        throw new Exception("The command returned " + (dsTemp.Tables.Count == 1 ? "only one table" : "zero tables") + ". This method requires that more than one table be returned from the command.");
                    }
                }
                else
                {
                    throw new Exception("The passed in DataSet to the MySqlDataAdapter returned a DataSet that was null after it executed the command. There must of been an error in the command even though no exception was thrown.");
                }

              

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }

        public DataTable GetTableWithStoredProcedure(string sProcedure, params MySqlParameter[] oMySqlParameters)
        {

             DataTable dt = null;
 
             try
             {
                
                    MySqlConnection oMySqlConnection = this.oMySqlConnection;
                    MySqlCommand oMySqlCommand = oMySqlConnection.CreateCommand();
                    oMySqlCommand.CommandType = CommandType.StoredProcedure;
                    oMySqlCommand.CommandText = sProcedure;
                    foreach (MySqlParameter MP in oMySqlParameters)
                    {
                        oMySqlCommand.Parameters.Add(MP);
                    }
                    MySqlDataAdapter oAdapter = new MySqlDataAdapter(oMySqlCommand);
                    DataSet ds = new DataSet();
                    oAdapter.Fill(ds);
                    if (ds != null)
                    {
                        if (ds.Tables.Count == 1)
                        {
                            dt = ds.Tables[0];
                        }
                        else
                        {
                            throw new Exception("The command returned " + (ds.Tables.Count > 1 ? "more than one table" : "zero tables") + ". This method requires that one table and only one table be returned from the command.");
                        }
                    }
                    else
                    {
                        throw new Exception("The passed in DataSet to the MySqlDataAdapter returned a DataSet that was null after it executed the command. There must of been an error in the command even though no exception was thrown.");
                    }

                 
             }
             catch (Exception ex)
             {
                 throw ex;
             }
             return dt;
        }

        public DataSet GetTablesWithStoredProcedure(string sProcedure,  params MySqlParameter[] oMySqlParameters)
        {
            DataSet ds = null;
            try
            {
               
                MySqlConnection oMySqlConnection = this.oMySqlConnection;

                MySqlCommand oMySqlCommand = oMySqlConnection.CreateCommand();
                oMySqlCommand.CommandType = CommandType.StoredProcedure;
                oMySqlCommand.CommandText = sProcedure;
                foreach (MySqlParameter MP in oMySqlParameters)
                {
                    oMySqlCommand.Parameters.Add(MP);
                }
                MySqlDataAdapter oAdapter = new MySqlDataAdapter(oMySqlCommand);
                DataSet dsTemp = new DataSet();
                oAdapter.Fill(dsTemp);
                if (dsTemp != null)
                {
                    if (ds.Tables.Count == 1)
                    {
                        ds = dsTemp;
                    }
                    else
                    {
                        throw new Exception("The command returned " + (ds.Tables.Count > 1 ? "more than one table" : "zero tables") + ". This method requires that one table and only one table be returned from the command.");
                    }
                }
                else
                {
                    throw new Exception("The passed in DataSet to the MySqlDataAdapter returned a DataSet that was null after it executed the command. There must of been an error in the command even though no exception was thrown.");
                }

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }

        public int  GetRowsAffectedWithQuery(string sQuery)
        {

           int iRowsAffected = 1;
            try
            {
                
                MySqlConnection oMySqlConnection = this.oMySqlConnection;
                MySqlCommand oMySqlCommand = oMySqlConnection.CreateCommand();
                oMySqlCommand.CommandType = CommandType.Text;
                oMySqlCommand.CommandText = sQuery;
                iRowsAffected = oMySqlCommand.ExecuteNonQuery();
                if (iRowsAffected < 0)
                {
                    throw new Exception("The ExecuteNonQuery function did not throw an error but the number of rows returned was negative indicating an error!");
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iRowsAffected;
        }

        public int GetRowsAffectedWithStoredProcedure(string sProcedure, params MySqlParameter[] oMySqlParameters)
        {
            int iRowsAffected = 1;
            try
            {
                
                MySqlConnection oMySqlConnection = this.oMySqlConnection;
                MySqlCommand oMySqlCommand = oMySqlConnection.CreateCommand();
                oMySqlCommand.CommandType = CommandType.StoredProcedure;
                oMySqlCommand.CommandText = sProcedure;
                foreach (MySqlParameter oMySqlParameter in oMySqlParameters)
                {
                    oMySqlCommand.Parameters.Add(oMySqlParameter);
                }
                iRowsAffected = oMySqlCommand.ExecuteNonQuery();
                if (iRowsAffected < 0)
                {
                    throw new Exception("The ExecuteNonQuery function did not throw an error but the number of rows returned was negative indicating an error!");
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iRowsAffected;
        }

        public string AddSchemaToTableName(string sTableName)
        {
            string sFormalTalbeName = sTableName;

            if (sTableName.Contains(this.oMySqlConnection.Database + '.'))
            {//has database name but does it have the tick marks around it?
                if (sTableName.IndexOf("`" + this.oMySqlConnection.Database + "`" + ".") < 0)
                {//no tick marks were found around the database name so we will remove it and add it later
                    sFormalTalbeName = sTableName.Replace(this.oMySqlConnection.Database + ".", "");
                }
            }

            if (!sFormalTalbeName.Contains("."))
            {//Make sure there are no other dots because if there are well I don't know what the hell this string contains
                sFormalTalbeName = sFormalTalbeName.Replace("`", "");
                sFormalTalbeName = "`" + this.oMySqlConnection.Database + "`" + "." + "`" + sFormalTalbeName + "`";
            }
            else
            {//At this point there should be no dots left in the string it should just be the table name so I am going to 'undo' and return the string the way it was
                sFormalTalbeName = sTableName;
            }

            return sFormalTalbeName;

        }

        public string GetTableNameOnly(string sTableName)
        {
            string sTalbeNameOnly = sTableName;

            if (sTableName.Contains(this.oMySqlConnection.Database + '.'))
            {//has database name but does it have the tick marks around it?
                if (sTableName.IndexOf("`" + this.oMySqlConnection.Database + "`" + ".") > 0)
                {//no tick marks were found around the database name so we will remove it 
                    sTalbeNameOnly = sTableName.Replace("`" + this.oMySqlConnection.Database + "`" + ".", "");
                }
                else
                {
                    sTalbeNameOnly = sTableName.Replace(this.oMySqlConnection.Database + ".", "");
                }
            }
            sTalbeNameOnly = sTalbeNameOnly.Replace("`", "");
            sTalbeNameOnly = sTalbeNameOnly.Replace("'", "");

          
            return sTalbeNameOnly;

        }

    }
}
