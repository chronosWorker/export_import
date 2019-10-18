using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using static System.Collections.IEnumerable;
using static Process_Export_Import.Process_Export_Import;

namespace Process_Export_Import
{
    public class Import
    {
 
        public int process_id;
        public string process_name;
        public SQLiteConnection connSqlite;
        SqlCommand cmdMsSql;
        SqlConnection MSSQLConnection;
        string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
        public string[] tables;


        public bool first_insert(string tableName,Dictionary<string,string> tableColumnInfo)
        {
            bool insert_was_succesfull = false;
            string connstrRe = ConfigurationManager.AppSettings.Get("connstrRe");
            string strSQL = "";
            string strMsSQL = "";
            SqlConnection MSSQLConnection = new SqlConnection(connstrRe);
            SqlCommand cmdMsSql;
            cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
            ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
            SqlConnection connection = new SqlConnection(connstrRe);
            strSQL = "Iinsert into @tableName ";
            SqlCommand command = new SqlCommand(strSQL, connection);
            command.Parameters.AddWithValue("@tableName", tableName);






            return insert_was_succesfull;
        }
       
    }
   

    
}