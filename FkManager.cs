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
            int maxActivityIdList = new int();

            try
            {
                var reader = obj.sqlServerDataReader("Select max( " + IdName + " ) as Max_Id from " + TableName);
                while (reader.Read())
                {
                    maxActivityIdList = Convert.ToInt32(reader["Max_Id"]);
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxActivityIdList;

        }

        public  List<int> getIdsInOrderFromDBFile(ConnectionManagerST obj)
        {
            List<int> maxActivityIdList = new List<int>();

            try
            {
                var reader = obj.sqLiteDataReader("Select distinct(" + IdName + ") from " + TableName +  "   order by " + IdName + "  desc ");
                while (reader.Read())
                {
                    maxActivityIdList.Add(Convert.ToInt32(reader[IdName]));
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxActivityIdList;

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
            List<string> tablesWhereIdOccurs = new List<string>();
            string queryText = "Select distinct(table_name) from table_information where Column_Name = '" + IdName  + "'";
            var reader = obj.sqLiteDataReader(queryText);
            while (reader.Read())
            {
                tablesWhereIdOccurs.Add(reader["table_name"].ToString());
            }
            return tablesWhereIdOccurs;
        }

        public static List<int> getNewIdValueList(int maxIdInSQLServer, List<int> idDifferenceList)
        {
            List<int> updatedIdList = new List<int>();
            int newMaxId = maxIdInSQLServer + 1;
            updatedIdList.Add(newMaxId);
            foreach (int difference in idDifferenceList)
            {
                updatedIdList.Add(updatedIdList[updatedIdList.Count - 1] + difference);
            }

            return updatedIdList;

        }

        public  List<string> changeIdsInDBFileToTempValues(List<int> oldIdList, List<int> newIdList, List<string> tablesWithId,  ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();

            foreach (string tableName in tablesWithId)
            {
                for (int index = 0; index < newIdList.Count; index++)
                {
                    int tempId = 10000000 + newIdList[index];
                    string updateText = "Update " + tableName + " Set " + IdName +  "  = " + tempId.ToString() + " where "+ IdName  + " = " + oldIdList[index].ToString();
                    obj.executeQueriesInDbFile(updateText);
                    updateInfo.Add(updateText);
                }
            }

            return updateInfo;

        }

        public  List<string> changeIdsInDBFileToRealNewID( ConnectionManagerST obj , List<string>  tablesWithId)
        {
            List<string> updateInfo = new List<string>();

            foreach (string tableName in tablesWithId)
            {
                int tempActivityIdToDistract = 10000000;
                string updateText = "Update " + tableName + " Set " + IdName + " =  " + IdName  +  " - " + tempActivityIdToDistract.ToString() + " where 1 = 1 ;";
                obj.executeQueriesInDbFile(updateText);
                updateInfo.Add(updateText);
            }

            return updateInfo;

        }

        public List<string> convertIntListToStringList(List<int> inputStringList)
        {
            List<string> convertedStringList = inputStringList.ConvertAll<string>(delegate (int i) { return i.ToString(); });
            return convertedStringList;
        }

        public  List<string> changeAllIdInDbFileToFitSqlServer(ConnectionManagerST connectionManager)
        {
            List<string> changingIdsInfoList = new List<string>();

            try
            {
                List<int> idsInDbFile = new List<int>();
                List<int> idDifferenceList = new List<int>();
                List<string> idUpdateInfo = new List<string>();
                List<int> newIdList = new List<int>();

                List<string> tablesWithIdInDBFile = getAllTableNameWithIdInDBFile(connectionManager);
                int maxIdInSqlServer = getMaxIdFromSQLServer(connectionManager);

                idsInDbFile = getIdsInOrderFromDBFile(connectionManager);
                idDifferenceList = getIdDifferencesList(idsInDbFile);

                newIdList = getNewIdValueList(maxIdInSqlServer, idDifferenceList);
                changingIdsInfoList = changeIdsInDBFileToTempValues(idsInDbFile, newIdList, tablesWithIdInDBFile, connectionManager);

                changingIdsInfoList.Add("Ids In Db File : ");
                changingIdsInfoList.AddRange(convertIntListToStringList(idsInDbFile));
                changeIdsInDBFileToRealNewID(connectionManager, tablesWithIdInDBFile);

                changingIdsInfoList.Add("newActivitIdList : ");
                changingIdsInfoList.AddRange( convertIntListToStringList(newIdList));
                changingIdsInfoList.AddRange(idUpdateInfo);
            }
            catch (Exception ex)
            {
                changingIdsInfoList.Add(ex.ToString());
            }
            return changingIdsInfoList;
        }


    }
}