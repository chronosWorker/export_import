using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class FkManager
    {
        string TableName;
        string IdName;

        public FkManager(string TableName, string IdName)
        {
            this.TableName = TableName;
            this.IdName = IdName;

        }
        public int getMaxIdFromSQLServer( ConnectionManagerST obj)
        {
            int maxIdList = new int();

            try
            {
                var reader = obj.sqlServerDataReader("Select max( " + IdName + " ) as Max_Id from " + TableName);
                while (reader.Read())
                {
                    if (reader[0] != DBNull.Value)
                    {
                        maxIdList = Convert.ToInt32(reader["Max_Id"]);
                    }
                    else
                    {
                        maxIdList = -1;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxIdList;

        }

        public  List<int> getIdsInOrderFromDBFile(ConnectionManagerST obj)
        {
            List<int> maxIdList = new List<int>();

            try
            {
                var reader = obj.sqLiteDataReader("Select distinct(" + IdName + ") from " + TableName +  "   order by " + IdName + "  asc ");
                while (reader.Read())
                {
                    if (reader[0] != DBNull.Value)
                    {
                        maxIdList.Add(Convert.ToInt32(reader[IdName]));
                    }
                    else
                    {
                        List<int> nullList = new List<int>();
                        return nullList;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxIdList;

        }

        public  List<int> getIdDifferencesList(List<int> Id_Int_List)
        {
            List<int> IdDifferenceList = new List<int>();

            try
            {

                for (int outerIndex = 1; outerIndex < Id_Int_List.Count; outerIndex++)
                {

                    int difference = Id_Int_List[outerIndex - 1] - Id_Int_List[outerIndex];
                    IdDifferenceList.Add(difference);

                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return IdDifferenceList;

        }

        public List<string> getAllTableNameWithIdInDBFile(ConnectionManagerST obj)
        {
            try
            { 
                List<string> tablesWhereIdOccurs = new List<string>();
                string queryText = "Select distinct(table_name) from table_information where Column_Name = '" + IdName  + "'";
                var reader = obj.sqLiteDataReader(queryText);
                while (reader.Read())
                {
                    tablesWhereIdOccurs.Add(reader["table_name"].ToString());
                }
                return tablesWhereIdOccurs;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public string noCheckConstraitForTableList(List<string> tableNames, ConnectionManagerST obj)
        {
            try
            { 
                string queryText = "";
                foreach (string tableName in tableNames)
                {
                     queryText += "ALTER TABLE " + tableName + " NOCHECK CONSTRAINT ALL; ";

                }
                obj.executeQueriesInSqlServer(queryText);
                return queryText;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public static List<int> getNewIdValueList(int maxIdInSQLServer, List<int> idDifferenceList)
        {
            List<int> updatedIdList = new List<int>();
            try
            { 
                int newMaxId = maxIdInSQLServer + 1;
                updatedIdList.Add(newMaxId);
                foreach (int difference in idDifferenceList)
                {
                    updatedIdList.Add(updatedIdList[updatedIdList.Count - 1] - difference);
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return updatedIdList;

        }

        public  List<string> changeIdsInDBFileToTempValues(List<int> oldIdList, List<int> newIdList, string TableName,  ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();
            try
            { 
            //    foreach (string tableName in tablesWithId)
             //   {
                    for (int index = 0; index < newIdList.Count; index++)
                    {
                        int tempId = 10000000 + newIdList[index];
                        string updateText = "Update " + TableName + " Set " + IdName +  "  = " + tempId.ToString() + " where "+ IdName  + " = " + oldIdList[index].ToString();
                        obj.executeQueriesInDbFile(updateText);
                   //     updateInfo.Add(updateText);
                    //    updateInfo.Add("New ID List:");
                      //  updateInfo.AddRange(convertIntListToStringList(newIdList));
                }
              //  }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return updateInfo;

        }



        public  List<string> changeIdsInDBFileToRealNewID( ConnectionManagerST obj , string TableName)
        {
            List<string> updateInfo = new List<string>();
            try
            { 

              //  foreach (string tableName in tablesWithId)
              //  {
                    int tempIdToDistract = 10000000;
                    string updateText = "Update " + TableName + " Set " + IdName + " =  " + IdName  +  " - " + tempIdToDistract.ToString() + " where 1 = 1 ;";
                    obj.executeQueriesInDbFile(updateText);
               //     updateInfo.Add(updateText);
              //  }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return updateInfo;

        }
        public Dictionary<string, string> getColumnTypesDictionary_v2(string CWPTableName, ConnectionManagerST obj)
        {

            Dictionary<string, string> fields = new Dictionary<string, string>();
            try
            { 

            string commandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + CWPTableName + "'";
            var reader = obj.sqlServerDataReader(commandText);
            while (reader.Read())
            {
                fields.Add(reader["COLUMN_NAME"].ToString(), reader["DATA_TYPE"].ToString());
            }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return fields;

        }

        public List<string> changeIdsInAllRelatedTableIfSameRecordFound(List<int> oldIdList, List<int> newIdList, ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();
            List<string> allTableWithThatIdInIt = getAllTableNameWithIdInDBFile(obj);
            allTableWithThatIdInIt.Remove(TableName);
            foreach (string table in allTableWithThatIdInIt)
            {
                for (int oldIddListIndex = 0; oldIddListIndex < oldIdList.Count; oldIddListIndex++)
                {
                    string commandText = "Select Count(*) from " + table + " where " + IdName + " = " + oldIdList[oldIddListIndex].ToString();
                    var reader = obj.sqLiteDataReader(commandText);
                    updateInfo.Add(commandText);
                    reader.Read();
                    if(reader[0].ToString() == "1")
                    {
                        updateInfo.Add("Table where old Id Found : " + table);
                        string updateCommandText = " Update " + table + " Set " + IdName + " = " + newIdList[oldIddListIndex].ToString() + " where " + IdName + " = " + oldIdList[oldIddListIndex].ToString();
                        obj.executeQueriesInDbFile(updateCommandText);
                        updateInfo.Add(updateCommandText);
                    }

                }
            }
            //Ha megtalálja a oldIdlistet akkor cserélje az ujra
            return updateInfo;
        }


        public List<string> convertIntListToStringList(List<int> inputStringList)
        {
            List<string> convertedStringList = inputStringList.ConvertAll<string>(delegate (int i) { return i.ToString(); });
            return convertedStringList;
        }

        public List<string> insertValuesFromDbFileToSqlServer(string tableName, bool needToSetIdentityInsertOn, ConnectionManagerST obj)
        {
            List<string> insertresultInfo = new List<string>();

            List<string> columnNamesInDbFile = new List<string>();
            List<string> values = new List<string>();
            Dictionary<string, string> columnTypes = new Dictionary<string, string>();
            string commandText = "INSERT INTO " + tableName + "  ( ";
            try
            {
                columnTypes = getColumnTypesDictionary_v2(tableName, obj);

                var reader = obj.sqLiteDataReader("SELECT * FROM " + tableName);
                int fieldCount = reader.FieldCount;
                //		insertresultInfo.Add("Field Count :" + fieldCount);


                for (var index = 0; index < columnTypes.Count; index++)
                {
                    if (index == columnTypes.Count - 1)
                    {
                        commandText += columnTypes.ElementAt(index).Key;
                    }
                    else
                    {
                        commandText += columnTypes.ElementAt(index).Key + " ,";
                    }
                    columnNamesInDbFile.Add(columnTypes.ElementAt(index).Key);
                }

                commandText += ") Values ";
                while (reader.Read())
                {
                    if (reader.GetValue(0) != "NULL" || reader.GetValue(0) != "")
                    {

                        commandText += "( ";

                        for (var index = 0; index < columnTypes.Count; index++)
                        {

                            switch (columnTypes.ElementAt(index).Value)
                            {

                                case "bit":
                                case "binary":
                                case "varbinary":
                                case "image":
                                case "DateTime":
                                case "nvarchar":
                                case "varchar":
                                    commandText += "'" + reader[columnTypes.ElementAt(index).Key.ToString()] + "'";
                                    break;
                                default:
                                    commandText += (reader[columnTypes.ElementAt(index).Key.ToString()].GetType() == typeof(DBNull) || reader[columnTypes.ElementAt(index).Key.ToString()] == "") ? "NULL" :
                                    reader[columnTypes.ElementAt(index).Key.ToString()];
                                    break;
                            }
                            if (index < columnTypes.Count - 1)
                            {

                                commandText += ",";
                            }
                        }

                        commandText += ") ,";
                        commandText = commandText.Substring(0, commandText.Length - 1);

                      //  insertresultInfo.Add("commandText: " + commandText);

                        if (needToSetIdentityInsertOn)
                        {
                            		obj.executeQueriesInSqlServer("SET IDENTITY_INSERT " + tableName + " ON ;" + commandText + " ; SET IDENTITY_INSERT " + tableName + " OFF ;");
                        }
                        else
                        {
                            		obj.executeQueriesInSqlServer(commandText);

                        }
                    }
                    else
                    {
                        insertresultInfo.Add(tableName + " has 0 rows");
                    }
                }
                   

                    insertresultInfo.Add(commandText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return insertresultInfo;
        }
        public  List<string> changeAllIdInDbFileToFitSqlServer(ConnectionManagerST connectionManager )
        {
            List<string> changingIdsInfoList = new List<string>();

            try
            {
                List<int> idsInDbFile = new List<int>();
                List<int> idDifferenceList = new List<int>();
                List<string> idUpdateInfo = new List<string>();
                List<int> newIdList = new List<int>();

             //   List<string> tablesWithIdInDBFile = getAllTableNameWithIdInDBFile(connectionManager);
             //   changingIdsInfoList.AddRange(getAllTableNameWithIdInDBFile(connectionManager));
               // List<string> tablesWithIdInDBFile = tableList;
          // changingIdsInfoList.Add(noCheckConstraitForTableList(tablesWithIdInDBFile, connectionManager));
                int maxIdInSqlServer = getMaxIdFromSQLServer(connectionManager);
               
                idsInDbFile = getIdsInOrderFromDBFile(connectionManager);
                if (idsInDbFile.Count == 0)
                {

                }
                else
                { 
                idDifferenceList = getIdDifferencesList(idsInDbFile);

                newIdList = getNewIdValueList(maxIdInSqlServer, idDifferenceList);
                changingIdsInfoList.AddRange(changeIdsInDBFileToTempValues(idsInDbFile, newIdList, TableName, connectionManager));

                changeIdsInDBFileToRealNewID(connectionManager, TableName);
                 
                changingIdsInfoList.Add(" ID in db file");
                changingIdsInfoList.AddRange(convertIntListToStringList(idsInDbFile));



                 
                    //az összes táblában ahol megtalálja ezeket ott cserélje idsInDbFile --> newIdList
                    //fgv neveket átírni mert most félrevezet



                changingIdsInfoList.Add("Max ID");
                changingIdsInfoList.Add(maxIdInSqlServer.ToString());
                changingIdsInfoList.Add("idDifferenceList");
                changingIdsInfoList.AddRange(convertIntListToStringList(idDifferenceList));
                changingIdsInfoList.Add("newIdList");
                changingIdsInfoList.AddRange(convertIntListToStringList(newIdList));

                changingIdsInfoList.AddRange(changeIdsInAllRelatedTableIfSameRecordFound(idsInDbFile, newIdList, connectionManager));

                changingIdsInfoList.AddRange(idUpdateInfo);
                }

            }
            catch (Exception ex)
            {
                changingIdsInfoList.Add(ex.ToString());
            }
            return changingIdsInfoList;
        }


    }
}