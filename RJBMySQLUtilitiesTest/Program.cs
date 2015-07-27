using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RJBMySQLUtilities.DataLogging;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using RJBMySQLUtilities.DataLogging.DataLogging.Controller;

namespace RJBMySQLUtilitiesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                string sSourceConnectionString = ConfigurationManager.ConnectionStrings["SourceDatabase"].ConnectionString;
                string sDestinationConnectionString = ConfigurationManager.ConnectionStrings["DestintaionDatabase"].ConnectionString;

                DataLogger oDataChangeControl = new DataLogger(sSourceConnectionString, sDestinationConnectionString);
                string sQuery = oDataChangeControl.GenerateLogSchemaFromSource();
                oDataChangeControl.GenerateAndRunLogSchemaQuery();
                int i = 0;

               
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.Write("Press any key to end...");
            Console.Read();
         }

      
    }
}
