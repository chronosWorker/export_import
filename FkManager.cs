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
                    if (reader[0] != DBNull.Value)
                    {
                        maxActivityIdList = Convert.ToInt32(reader["Max_Id"]);
                    }
                    else
                    {
                        maxActivityIdList = -1;
                    }
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
                var reader = obj.sqLiteDataReader("Select distinct(" + IdName + ") from " + TableName +  "   order by " + IdName + "  asc ");
                while (reader.Read())
                {
                    if (reader[0] != DBNull.Value)
                    {
                        maxActivityIdList.Add(Convert.ToInt32(reader[IdName]));
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

        public string noCheckConstraitForTableList(List<string> tableNames, ConnectionManagerST obj)
        {
            string queryText = "";
            foreach (string tableName in tableNames)
            {
                 queryText += "ALTER TABLE " + tableName + " NOCHECK CONSTRAINT ALL; ";

            }
            obj.executeQueriesInSqlServer(queryText);
            return queryText;
        }

        public static List<int> getNewIdValueList(int maxIdInSQLServer, List<int> idDifferenceList)
        {
            List<int> updatedIdList = new List<int>();
            int newMaxId = maxIdInSQLServer + 1;
            updatedIdList.Add(newMaxId);
            foreach (int difference in idDifferenceList)
            {
                updatedIdList.Add(updatedIdList[updatedIdList.Count - 1] - difference);
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
            //        updateInfo.Add(updateText);
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
           //     updateInfo.Add(updateText);
            }

            return updateInfo;

        }

        public List<string> convertIntListToStringList(List<int> inputStringList)
        {
            List<string> convertedStringList = inputStringList.ConvertAll<string>(delegate (int i) { return i.ToString(); });
            return convertedStringList;
        }

        public List<string> firstRoundInsertTables()
        {
            List<string> firstRoundInsertTables = new List<string>()
                {
                " T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE ",
               /*   " T_ACTIVITY_DEPENDENT_COMPONENTS ",
                " T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION ",
                " T_ACTIVITY_FIELDS_FOR_ESIGNING ",
                " T_ACTIVITY_FIELDS_UI_PARAMETERS ",
                " T_ACTIVITY_FINISH_STEP_MODE "
              " T_ACTIVITY_OWNER_BY_CONDITION_CONDITION ",
                " T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP ",
                " T_ACTIVITY_UI_COMPONENT ",
                " T_CALCULATED_FIELD_CONSTANT_TYPE ",
                " T_FIELD_DATE_CONSTRAINT ",
                " T_FIELD_EXTENSION_NUMBER 1 ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS ",
                " T_FIELD_TO_FIELD_DEPENDENCY ",
                " T_FIELD_TO_FIELD_DEPENDENCY_TYPE ",
                " T_FIELD_VALUE ",
                " T_FIELD_VALUE_TRANSLATION ",
                " T_GENERAL_DATA_PROTECTION_FIELD_TYPE ",
                " T_NOTIFICATION_RECIPIENT ",
                " T_NOTIFICATION_TYPE ",
                " T_PROC_DESIGN_DRAW ",
                " T_PROC_DESIGN_DRAW_PART ",
                " T_PROC_DESIGN_DRAW_PART_DETAIL ",
                " T_PROC_DESIGN_DRAW_PART_TYPE ",
                " T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE ",
                " T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE ",
                " T_REPORT_FIELD_UDT_COLUMNS ",
                " T_REPORT_FILTER ",
                " T_REPORT_GROUP ",
                " T_REPORT_REFERENCED_FIELD_LOCATION ",
                " T_REPORT_TYPE ",
                " T_SUBPROCESS_TYPE "*/
                };

            return firstRoundInsertTables;
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
             

                changingIdsInfoList.Add(noCheckConstraitForTableList(tablesWithIdInDBFile, connectionManager));
                int maxIdInSqlServer = getMaxIdFromSQLServer(connectionManager);
               
                idsInDbFile = getIdsInOrderFromDBFile(connectionManager);
                if (idsInDbFile.Count == 0)
                {

                }
                else
                { 
                idDifferenceList = getIdDifferencesList(idsInDbFile);

                newIdList = getNewIdValueList(maxIdInSqlServer, idDifferenceList);
                changingIdsInfoList.AddRange(changeIdsInDBFileToTempValues(idsInDbFile, newIdList, tablesWithIdInDBFile, connectionManager));

                changeIdsInDBFileToRealNewID(connectionManager, tablesWithIdInDBFile);
                changingIdsInfoList.Add("Most itt");
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