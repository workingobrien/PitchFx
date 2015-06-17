using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Db.Utilities
{
   internal sealed class DbConnector
   {
      private const string SERVER = "server";
      private const string DATABASE = "database";
      private const string UID = "uid";
      private const string PASSWORD = "password";

      private static volatile DbConnector _instance;
      private static readonly object _syncRoot = new object();

      public static DbConnector Instance
      {
         get
         {
            if (_instance == null)
            {
               lock (_syncRoot)
               {
                  _instance = new DbConnector();
               }
            }
            return _instance;
         }
      }

      private MySqlConnection _sqlConnection;

      public MySqlConnection SqlConnection
      {
         get
         {
            if (_sqlConnection == null)
               _sqlConnection = GetConnection();
           
            if (_sqlConnection.State != ConnectionState.Open)
               _sqlConnection.Open();

            return _sqlConnection;
         }
      }

      private MySqlConnection GetConnection()
      {
         var server = ConfigurationManager.AppSettings[SERVER];
         var database = ConfigurationManager.AppSettings[DATABASE];
         var uid = ConfigurationManager.AppSettings[UID];
         var password = ConfigurationManager.AppSettings[PASSWORD];

         var connectionStr = string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3}", server, database, uid, password);
         return new MySqlConnection(connectionStr);
         
      }

   }
}
