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
        public int getMaxIdFromSQLServer(ConnectionManagerST obj)
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

        public List<int> getIdsInOrderFromDBFile(ConnectionManagerST obj)
        {
            List<int> maxIdList = new List<int>();

            try
            {
                var reader = obj.sqLiteDataReader("Select distinct(" + IdName + ") from " + TableName + "   order by cast(" + IdName + " as REAL)  asc ");
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

        public List<int> getIdDifferencesList(List<int> Id_Int_List)
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
                string queryText = "Select distinct(table_name) from table_information where Column_Name = '" + IdName + "'";
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

        public List<string> changeIdsInDBFileToTempValues(List<int> oldIdList, List<int> newIdList, string TableName, ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();
            try
            {
                //    foreach (string tableName in tablesWithId)
                //   {
                for (int index = 0; index < newIdList.Count; index++)
                {
                    int tempId = 10000000 + newIdList[index];
                    string updateText = "Update " + TableName + " Set " + IdName + "  = " + tempId.ToString() + " where " + IdName + " = " + oldIdList[index].ToString();
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

        public List<string> changeIdsInDBFileToRealNewID(ConnectionManagerST obj, string TableName)
        {
            List<string> updateInfo = new List<string>();
            try
            {

                //  foreach (string tableName in tablesWithId)
                //  {
                int tempIdToDistract = 10000000;
                string updateText = "Update " + TableName + " Set " + IdName + " =  " + IdName + " - " + tempIdToDistract.ToString() + " where 1 = 1 ;";
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
                    if (reader[0].ToString() != "0")
                    {
                        updateInfo.Add("Table where old Id Found : " + table);
                        string updateCommandText = " Update " + table + " Set " + IdName + " = " + newIdList[oldIddListIndex].ToString() + " where " + IdName + " = " + oldIdList[oldIddListIndex].ToString();
                        obj.executeQueriesInDbFile(updateCommandText);
                        updateInfo.Add(updateCommandText);
                        //ha találok olyan értéket ami routing_design activity_des_id-ban ami benne van ACTIVITY_DESIGn-ban  from vagy to act_design id-ként akkor azokat ugyanarra updatelem
                        if (IdName == "Activity_Design_ID")
                        {
                            string updateFromActivityDesignCommandText = " Update T_ROUTING_DESIGN  Set From_Activity_Design_ID = " + newIdList[oldIddListIndex].ToString() + " where From_Activity_Design_ID = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateFromActivityDesignCommandText);
                            string updateToActivityDesignCommandText = " Update T_ROUTING_DESIGN  Set To_Activity_Design_ID = " + newIdList[oldIddListIndex].ToString() + " where To_Activity_Design_ID = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateToActivityDesignCommandText);
                     //       updateInfo.Add(updateFromActivityDesignCommandText);
                     //       updateInfo.Add(updateToActivityDesignCommandText);
                        }
                        if (IdName == "Field_ID" && TableName == "T_FIELD")
                        {
                            string updateDependentFieldId =   " Update T_FIELD_TO_FIELD_DEPENDENCY  Set Dependent_Field_ID = " + newIdList[oldIddListIndex].ToString() + " where Dependent_Field_ID = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateDependentFieldId);
                            string updateIndependentFieldId =   " Update T_FIELD_TO_FIELD_DEPENDENCY  Set Independent_Field_ID = " + newIdList[oldIddListIndex].ToString() + " where Independent_Field_ID = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateIndependentFieldId);
                   //         updateInfo.Add(updateDependentFieldId);
                   //         updateInfo.Add(updateIndependentFieldId);
                        }
                        if (IdName == "Activity_ID")
                        {
                            string updateDependencyActivationId = " Update T_FIELD_TO_FIELD_DEPENDENCY  Set Dependency_Activation_Activity_ID = " + newIdList[oldIddListIndex].ToString() + " where Dependency_Activation_Activity_ID = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateDependencyActivationId);
                            string updateNotificationTriggerFromActivatyId = " Update T_NOTIFICATION_TRIGGER  Set From_Activity = " + newIdList[oldIddListIndex].ToString() + " where From_Activity = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateNotificationTriggerFromActivatyId);
                            string updateNotificationTriggerToActivatyId = " Update T_NOTIFICATION_TRIGGER  Set To_Activity = " + newIdList[oldIddListIndex].ToString() + " where To_Activity = " + oldIdList[oldIddListIndex].ToString();
                            obj.executeQueriesInDbFile(updateNotificationTriggerToActivatyId);
                        }
                    }

                }
            }
            //Ha megtalálja a oldIdlistet akkor cserélje az ujra
            return updateInfo;
        }



        public List<string> changeProcess(ConnectionManagerST obj)
        {
            List<string> newNameList = new List<string>();
            string processName = "";
            var reader = obj.sqLiteDataReader("Select Name from T_PROCESS");
            while (reader.Read())
            {
                processName = reader["Name"].ToString();
            }
            newNameList.Add("oiginal processName : ");
            newNameList.Add(processName);
            newNameList.Add("new processName :  : ");
            string importedName = processName + "_IMPORTED";
            newNameList.Add(importedName);
            string updateNameText = "UPDATE T_PROCES  SET Name  = " + importedName + " where 1 = 1";
            obj.executeQueriesInDbFile(updateNameText);

            return newNameList;

        }

        public int selectCountImportedProcesses(ConnectionManagerST obj, string processName)
        {
            int importedProcessQuantity = 0;
            string counterQuery = " Select count(*) as Counter from T_PROCESS where NAME like '%" + processName + " (IMP%'; ";
            var reader = obj.sqlServerDataReader(counterQuery);
            while (reader.Read())
            {
                importedProcessQuantity = Convert.ToInt32(reader["Counter"]);
            }
            return importedProcessQuantity;
        }

        public int selectCountImportedProcessesDesignName(ConnectionManagerST obj, string designName)
        {
            int importeddDsignNameQuantity = 0;
            string counterQuery = " Select count(*) as Counter from T_PROC_DESIGN_DRAW where NAME like '%" + designName + " (IMP%'; ";
            var reader = obj.sqlServerDataReader(counterQuery);
            while (reader.Read())
            {
                importeddDsignNameQuantity = Convert.ToInt32(reader["Counter"]);
            }
            return importeddDsignNameQuantity;
        }

        public List<string> changeProcessName(ConnectionManagerST obj)
        {
            List<string> newNameList = new List<string>();
            string processName = "";
            var reader = obj.sqLiteDataReader("Select Name from T_PROCESS");
            while (reader.Read())
            {
                processName = reader["Name"].ToString();
            }
            int processImportedQuantity = selectCountImportedProcesses(obj, processName);
            if (processImportedQuantity == 0)
            {
                string newProcessName = processName + " (IMPORTED)";
                string newProcessTechnicalName = processName.Replace(" ", "_") + "_IMPORTED";
                string newProcessNameUpdateQuery = " UPDATE T_PROCESS SET NAME = '" + newProcessName + "' WHERE NAME =  '" + processName + "'";
                obj.executeQueriesInDbFile(newProcessNameUpdateQuery);
            }
            else
            {
                string newProcessName = processName + " (IMPORTED_" + processImportedQuantity.ToString() + ")";
                string newProcessTechnicalName = processName.Replace(" ", "_") + "_IMPORTED_" + processImportedQuantity.ToString();
                string newProcessNameUpdateQuery = " UPDATE T_PROCESS SET NAME = '" + newProcessName + "' WHERE NAME =  '" + processName + "'";
                obj.executeQueriesInDbFile(newProcessNameUpdateQuery);
            }
            return newNameList;

        }

        public List<string> changeProcessDesignName(ConnectionManagerST obj)
        {
            List<string> newDesignNameList = new List<string>();
            string designName = "";
            var reader = obj.sqLiteDataReader("Select Name from T_PROC_DESIGN_DRAW");
            while (reader.Read())
            {
                designName = reader["Name"].ToString();
            }
            newDesignNameList.Add("oiginal processName : ");
            newDesignNameList.Add(designName);
            newDesignNameList.Add("new processName :  : ");
            int processImportedDesignQuantity = selectCountImportedProcessesDesignName(obj, designName);
            if (processImportedDesignQuantity == 0)
            {
                string newdesignName = designName + " (IMPORTED)";
                string newDesignNameUpdateQuery = " UPDATE T_PROC_DESIGN_DRAW SET NAME = '" + newdesignName + "' WHERE NAME = + '" + designName + "'";
                string newProcDesignNameUpdateQuery = " UPDATE T_PROCESS_DESIGN SET NAME = '" + newdesignName + "' WHERE NAME = + '" + designName + "'";
                obj.executeQueriesInDbFile(newDesignNameUpdateQuery);
                obj.executeQueriesInDbFile(newProcDesignNameUpdateQuery);
            }
            else
            {
                string newdesignName = designName + " (IMPORTED_" + processImportedDesignQuantity.ToString() + ")";
                string newDesignNameUpdateQuery = " UPDATE T_PROC_DESIGN_DRAW SET NAME = '" + newdesignName + "' WHERE NAME = + '" + designName + "'";
                string newProcDesignNameUpdateQuery = " UPDATE T_PROCESS_DESIGN SET NAME = '" + newdesignName + "' WHERE NAME = + '" + designName + "'";
                obj.executeQueriesInDbFile(newDesignNameUpdateQuery);
                obj.executeQueriesInDbFile(newProcDesignNameUpdateQuery);
            }

            return newDesignNameList;

        }

        public void updateProcessDesignDrawCreationDateToToday(ConnectionManagerST obj)
        {
            string updateQuery = " UPDATE T_PROC_DESIGN_DRAW set Creation_Date = datetime('now');";
            obj.executeQueriesInDbFile(updateQuery);
        }

        public List<string> convertIntListToStringList(List<int> inputStringList)
        {
            List<string> convertedStringList = inputStringList.ConvertAll<string>(delegate (int i) { return i.ToString(); });
            return convertedStringList;
        }

        public List<string> changeAllIdInDbFileToFitSqlServer(ConnectionManagerST connectionManager)
        {
            List<string> changingIdsInfoList = new List<string>();

            try
            {
                List<int> idsInDbFile = new List<int>();
                List<int> idDifferenceList = new List<int>();
                List<string> idUpdateInfo = new List<string>();
                List<int> newIdList = new List<int>();

                int maxIdInSqlServer = getMaxIdFromSQLServer(connectionManager);

                idsInDbFile = getIdsInOrderFromDBFile(connectionManager);
                changingIdsInfoList.Add(" ID in db file");
                changingIdsInfoList.AddRange(convertIntListToStringList(idsInDbFile));
                if (idsInDbFile.Count == 0)
                {

                }
                else
                {
                    idDifferenceList = getIdDifferencesList(idsInDbFile);

                    newIdList = getNewIdValueList(maxIdInSqlServer, idDifferenceList);
                    changingIdsInfoList.AddRange(changeIdsInDBFileToTempValues(idsInDbFile, newIdList, TableName, connectionManager));

                    changeIdsInDBFileToRealNewID(connectionManager, TableName);
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
        //Típus táblák.
        public List<string> getTwoDimensionalTypeTableValuesFromDbFile(ConnectionManagerST connectionManager)
        {
            List<string> twoDimensionalTypeTableValuesInDbFile = new List<string>();
            string readerQuery = "Select " + IdName + " from " + TableName;
            var reader = connectionManager.sqLiteDataReader(readerQuery);
            try
            {
                while (reader.Read())
                {
                    twoDimensionalTypeTableValuesInDbFile.Add(reader[IdName].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return twoDimensionalTypeTableValuesInDbFile;
        }

        public List<string> getTwoDimensionalTypeTableValuesFromSqlserver(ConnectionManagerST connectionManager)
        {
            List<string> twoDimensionalTypeTableValuesInSqlServer = new List<string>();
            string readerQuery = "Select " + IdName + " from " + TableName;
            var reader = connectionManager.sqlServerDataReader(readerQuery);
            try
            {
                while (reader.Read())
                {
                    twoDimensionalTypeTableValuesInSqlServer.Add(reader[IdName].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return twoDimensionalTypeTableValuesInSqlServer;
        }

        public List<string> compareTwoTypeTableListToGetSameRecords(List<string> twoDimensionalTypeTableValuesInDbFile , List<string> twoDimensionalTypeTableValuesInSqlServer)
        {
            List<string> recordsToDelete = new List<string>();
            try
            {
                foreach (string valueInDbFile in twoDimensionalTypeTableValuesInDbFile)
                {
                    foreach (string valueInDbFileSqlServer in twoDimensionalTypeTableValuesInSqlServer)
                    {
                        if (valueInDbFileSqlServer == valueInDbFile)
                        {
                            recordsToDelete.Add(valueInDbFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return recordsToDelete;
        }

        public void deleteSameRecordsFromTypeTable(ConnectionManagerST connectionManager , List<string> recordsToDelete )
        {
            try
            {
                foreach(string record in recordsToDelete)
                {
                    if (record.Contains("'"))
                    {
                        string currentRecordSingleQuoteFormalized = record.Replace("'", "''");
                        string deleteQuery = "Delete From " + TableName + " where " + IdName + " = '" + currentRecordSingleQuoteFormalized + "';";
                        connectionManager.executeQueriesInDbFile(deleteQuery);
                    }
                    else
                    {
                        string deleteQuery = "Delete From " + TableName + " where " + IdName + " = '" + record + "';" ;
                        connectionManager.executeQueriesInDbFile(deleteQuery);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void deleteUnnecessaryRecordsFromTypeTables(ConnectionManagerST connectionManager)
        {
            try
            {
                List<string> recordsDeleted = new List<string>();
                List<string> tempCategoryDictDbFile = getTwoDimensionalTypeTableValuesFromDbFile(connectionManager);
                List<string> tempCategoryDictInServer = getTwoDimensionalTypeTableValuesFromSqlserver(connectionManager);
                List<string> sameRecordInDictioaries = compareTwoTypeTableListToGetSameRecords(tempCategoryDictDbFile, tempCategoryDictInServer);
                deleteSameRecordsFromTypeTable(connectionManager, sameRecordInDictioaries);
                recordsDeleted.AddRange(sameRecordInDictioaries);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}