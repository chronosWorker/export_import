using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class ConnectionManagerST
    {

        public static ConnectionManagerST _instance;
        public static readonly object _padlock;

        public ConnectionManagerST()
        {

        }

        public static ConnectionManagerST Instance
        {
            get
            {
                if (_instance == null)
                {

                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConnectionManagerST();
                        }
                    }

                }

                return _instance;
            }
        }

        public string connectionStringToDbFile { get; set; } = ConfigurationManager.ConnectionStrings["Default"].ToString();
        SQLiteConnection connSqlite;

        public string connectionStringToSqlServer { get; set; } = ConfigurationManager.AppSettings.Get("connstrRe");
        SqlConnection connectionToNewSqlServer;


        public string connectionStringToOldSqlServer { get; set; } = ConfigurationManager.AppSettings.Get("connstr");
        SqlConnection connectionToOldSqlServer;


        public void openSqLiteConnection(string connecSünString)
        {
            connSqlite = new SQLiteConnection(connecSünString);
            connSqlite.Open();
        }

        public void executeQueriesInDbFile(string Query_)
        {
            using (SQLiteCommand cmdTxt = new SQLiteCommand(Query_, connSqlite)) {
                
            cmdTxt.ExecuteNonQuery();

            }
        }

        public void executeQueriesInDbFile_v2(string Query_)
        {
            SQLiteCommand cmdTxt = new SQLiteCommand(Query_, connSqlite);
            cmdTxt.ExecuteNonQuery();
        }

        public SQLiteDataReader sqLiteDataReader(string Query_)
        {
            SQLiteCommand cmd = new SQLiteCommand(Query_, connSqlite);
            SQLiteDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public void closeSqLiteConnection()
        {
           connSqlite.Close();
           SQLiteConnection.ClearAllPools();
        }

        public void openSqlServerConnection()
        {
            connectionToNewSqlServer = new SqlConnection(connectionStringToSqlServer);
            connectionToNewSqlServer.Open();

        }

        public void executeQueriesInSqlServer(string Query_)
        {
            SqlCommand cmdText = new SqlCommand(Query_, connectionToNewSqlServer);
            cmdText.ExecuteNonQuery();
        }

        public SqlDataReader sqlServerDataReader(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, connectionToNewSqlServer);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public SqlDataReader sqlServerDataReaderWithSingleParameter(string Query_, string parameter)
        {
            SqlCommand cmd = new SqlCommand(Query_, connectionToNewSqlServer);
            cmd.Parameters.AddWithValue("@param", parameter);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public SqlDataReader sqlOldServerDataReaderWithSingleParameter(string Query_, string parameter)
        {
            SqlCommand cmd = new SqlCommand(Query_, connectionToOldSqlServer);
            cmd.Parameters.AddWithValue("@param", parameter);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public void closeSqlServerConnection()
        {
            connectionToNewSqlServer.Close();
        }

        public void openOldSqlServerConnection()
        {
            connectionToOldSqlServer = new SqlConnection(connectionStringToOldSqlServer);
            connectionToOldSqlServer.Open();

        }

        public void executeQueriesInOldSqlServer(string Query_)
        {
            SqlCommand cmdText = new SqlCommand(Query_, connectionToOldSqlServer);
            cmdText.ExecuteNonQuery();
        }

        public SqlDataReader sqlServerDataReaderOld(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, connectionToOldSqlServer);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public void closeOldSqlServerConnection()
        {
            connectionToOldSqlServer.Close();
        }
    }

}