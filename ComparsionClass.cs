using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class ComparsionClass
    {
        public Dictionary<string ,int> Main(ConnectionManagerST obj)
        {

            Dictionary<string, int> countListForFirstDbFile = new Dictionary<string, int>();
            Dictionary<string, int> countListForSecondDbFile = new Dictionary<string, int>();
            Dictionary<string, int> differences = new Dictionary<string, int>();
            string simpleConnectionForTesting = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\1.db; Version=3;";
            string simpleConnectionTwoForTesting = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\2.db; Version=3;";
            TableManager tableInfo = new TableManager();

            obj.openSqLiteConnection(simpleConnectionForTesting);
            foreach (string tableName in tableInfo.getAllTablesNameInDbFile())
            {
                string readerCmdTxt = " Select count(*) as rowNumber from " + tableName + ";";
                var reader = obj.sqLiteDataReader(readerCmdTxt);
                    while (reader.Read())
                {
                    countListForFirstDbFile.Add(tableName, Convert.ToInt32(reader["rowNumber"]));

                }

            }
            obj.closeSqLiteConnection();
            obj.openSqLiteConnection(simpleConnectionTwoForTesting);
            foreach (string tableName in tableInfo.getAllTablesNameInDbFile())
            {
                string readerCmdTxt = " Select count(*) as rowNumber from " + tableName + ";";
                var reader = obj.sqLiteDataReader(readerCmdTxt);
                while (reader.Read())
                {
                    countListForSecondDbFile.Add(tableName, Convert.ToInt32(reader["rowNumber"]));

                }

            }
            obj.closeSqLiteConnection();
    
            if (countListForFirstDbFile.Count == countListForSecondDbFile.Count)
            {
                for (int index = 0; index < countListForSecondDbFile.Count; index++)
                {
                    var item1 = countListForFirstDbFile.ElementAt(index);
                    var item2 = countListForSecondDbFile.ElementAt(index);
                    if (item1.Key == item2.Key)
                    {
                        if (item1.Value == item2.Value)
                        {
                            differences.Add("No difference spotted at "  + item1.Key.ToString() ,  0);
                        }
                        else if (item1.Value > item2.Value)
                        {
                            differences.Add("Difference at " + item1.Key + " in the 1.db file", item1.Value - item2.Value);
                        }
                        else if (item2.Value > item1.Value)
                        {
                            differences.Add("Difference at " + item2.Key + " in the 2.db file", item2.Value - item1.Value);
                        }
                    }
                }
            }
            else
            {
                differences.Add("Two db fil are not the same size", -1);
            }


            return differences;
        }


    }
}