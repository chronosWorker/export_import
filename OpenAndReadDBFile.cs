using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using static Process_Export_Import.Process_Export_Import;

namespace Process_Export_Import
{
    public class OpenAndReadDBFile
    {

    public string file_location;
    private SQLiteConnection sqlite;
    private SQLiteConnection connSqlite;
    public List<int> operands = new List<int>();
    public List<string> tables;
        public OpenAndReadDBFile()
        {
            ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
            connSqlite = new SQLiteConnection();
            string connectionString = LoadConnectionString();
            sqlite = new SQLiteConnection(connectionString);


        }
        public  string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ToString();
           
        }
        public List<int> selectQuery()
        {
           
            string strSQL = "SELECT * from T_FIELD where process_id = 1022; ";
            string processName = "";
          
            {

                SQLiteCommand command = new SQLiteCommand(strSQL, sqlite);
                try
                {
                    sqlite.Open();
                    SQLiteDataReader reader;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        operands.Add(Convert.ToInt32(reader["Field_Type"]));

                    }
                    reader.Close();


                }
                catch (Exception ex)
                {
                    processName = ex.Message + ex.StackTrace.ToString();
                    operands.Add(-1);
                }
                finally
                {
                    
                    sqlite.Close();
                }
            }
            return operands;
        }
    }
}