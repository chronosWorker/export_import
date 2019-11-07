using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class ConnectionManager
    {
        public string connectionStringToDbFile { get; set; } = ConfigurationManager.ConnectionStrings["Default"].ToString();
        SQLiteConnection connSqlite;

        public string connectionStringToSqlServer { get; set; } = ConfigurationManager.AppSettings.Get("connstrRe");
        SqlConnection connection;



        public void  openSqLiteConnection()
        {
            connSqlite = new SQLiteConnection(connectionStringToDbFile);
            connSqlite.Open();
        }

        public void executeQueriesInDbFile(string Query_)
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
        }

       

        public void openSqlServerConnection()
        {
            connection = new SqlConnection(connectionStringToSqlServer);
            connection.Open();

        }

        public void executeQueriesInSqlServer(string Query_)
        {   
            SqlCommand cmdText = new SqlCommand(Query_, connection);
            cmdText.ExecuteNonQuery();
        }

        public SqlDataReader DataReader(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, connection);
            SqlDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public void closeSqlServerConnection()
        {
            connection.Close();
        }
    }

}