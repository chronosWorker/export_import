using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public static class DrawManager
    {
        //Process Design Id methods

        public static List<string> allTableWithProcessDesignId = new List<string>() { "T_PROCESS", "T_PROCESS_DESIGN", "T_PROC_DESIGN_DRAW", "T_ROUTING_DESIGN", "T_ACTIVITY_DESIGN" };


        public static int getMaxProcessDesignIdFromSQLServer(ConnectionManagerST obj)
        {
            int maxProcessDesignId = 0;

            try
            {
                var reader = obj.sqlServerDataReader("Select max(process_design_id) as Max_Process_Design_Id from T_PROC_DESIGN_DRAW");
                while (reader.Read())
                {
                    maxProcessDesignId = Convert.ToInt32(reader["Max_Process_Design_Id"]);
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxProcessDesignId;

        }

        //Process Design Draw Part Id methods

        public static List<string> allTableWithProcDesignDrawPartIds = new List<string>() { "T_PROC_DESIGN_DRAW_PART" };

        public static int getMaxProcessDesignDrawPartIdFromSQLServer(ConnectionManagerST obj)
        {
            int maxProcessDesignDrawPartId = 0;

            try
            {
                var reader = obj.sqlServerDataReader("Select max(Proc_Design_Draw_Part_ID) as Max_Process_Design_Draw_Part_Id from T_PROC_DESIGN_DRAW_PART");
                while (reader.Read())
                {
                    maxProcessDesignDrawPartId = Convert.ToInt32(reader["Max_Process_Design_Draw_Part_Id"]);
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxProcessDesignDrawPartId;

        }


        public static List<int> getProcDesignDrawPartIdsInOrderFromDBFile(ConnectionManagerST obj)
        {
            List<int> procDesginDrawPartIdList = new List<int>();

            try
            {
                var reader = obj.sqLiteDataReader("Select distinct(Proc_Design_Draw_Part_ID) from T_PROC_DESIGN_DRAW_PART  order by Proc_Design_Draw_Part_ID asc ");
                while (reader.Read())
                {
                    procDesginDrawPartIdList.Add(Convert.ToInt32(reader["Proc_Design_Draw_Part_ID"]));
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return procDesginDrawPartIdList;

        }

        public static List<int> getProcDesignDrawPartIdDifferences(List<int> ProcDesignDrawPartIdList)
        {
            List<int> procDesignDrawPartIdifferenceList = new List<int>();

            try
            {

                for (int outerIndex = 1; outerIndex < ProcDesignDrawPartIdList.Count; outerIndex++)
                {

                    int difference = ProcDesignDrawPartIdList[outerIndex - 1] - ProcDesignDrawPartIdList[outerIndex];
                    procDesignDrawPartIdifferenceList.Add(difference);

                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return procDesignDrawPartIdifferenceList;

        }

        public static List<int> getNewProcessDesignDrawPartIdValueList(int maxProcessDesignDrawPartIdInSQLServer, List<int> processDesignDrawPartIdDifferenceList)
        {
            List<int> updatedProcessDesignDrawPartIdList = new List<int>();
            int newMaxActivityId = maxProcessDesignDrawPartIdInSQLServer + 1;
            updatedProcessDesignDrawPartIdList.Add(newMaxActivityId);
            foreach (int difference in processDesignDrawPartIdDifferenceList)
            {
                updatedProcessDesignDrawPartIdList.Add(updatedProcessDesignDrawPartIdList[updatedProcessDesignDrawPartIdList.Count - 1] + difference);
            }


            return updatedProcessDesignDrawPartIdList;

        }

        public static List<string> changeProcDesignDrawPartIdsInDBFileByUpdatedList(List<int> oldProcDesignDrawPartIdsList, List<int> newProcDesignDrawPartIdsList, List<string> allTableWithProcDesignDrawPartIds, ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();

            foreach (string tableName in allTableWithProcDesignDrawPartIds)
            {
                for (int index = 0; index < newProcDesignDrawPartIdsList.Count; index++)
                {
                    int tempProcDesignDrawPartId = 10000000 + newProcDesignDrawPartIdsList[index];
                    string updateText = "Update " + tableName + " Set Proc_Design_Draw_Part_ID = " + tempProcDesignDrawPartId.ToString() + " where Proc_Design_Draw_Part_ID = " + oldProcDesignDrawPartIdsList[index].ToString();
                    obj.executeQueriesInDbFile(updateText);
                    updateInfo.Add(updateText);
                }
            }

            return updateInfo;

        }

        public static List<string> changetempProcDesignDrawPartIdsInDBFileToRealNewtempProcDesignDrawPartIds(List<string> allTableWithProcDesignDrawPartIds, ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();

            foreach (string tableName in allTableWithProcDesignDrawPartIds)
            {
                int tempActivityIdToDistract = 10000000;
                string updateText = "Update " + tableName + " Set Proc_Design_Draw_Part_ID =  Proc_Design_Draw_Part_ID  - " + tempActivityIdToDistract.ToString() + " where 1 = 1 ;";
                obj.executeQueriesInDbFile(updateText);
                updateInfo.Add(updateText);
            }

            return updateInfo;

        }

        public static List<string> changeAllProcessDesignDrawPartIdInDbFileToFitSqlServer(ConnectionManagerST connectionManager)
        {
            List<string> changingProcessDesignDrawPartIdsInfoList = new List<string>();
            try
            {
                List<int> processDesignDrawPartIdsInDbFile = new List<int>();
                List<int> processDesignDrawPartIdDifferenceList = new List<int>();
                List<string> processDesignDrawPartIdUpdateInfoList = new List<string>();
                List<int> newProcessDesignDrawPartIdList = new List<int>();

                List<string> tablesWithProcessDesignDrawPartIdInDBFile = allTableWithProcDesignDrawPartIds;
                int maxProcessDesignDrawPartIdInSqlServer = getProcDesignDrawPartIdsInOrderFromDBFile(connectionManager).First();

                processDesignDrawPartIdsInDbFile = getProcDesignDrawPartIdsInOrderFromDBFile(connectionManager);
                processDesignDrawPartIdDifferenceList = getProcDesignDrawPartIdDifferences(processDesignDrawPartIdsInDbFile);

                newProcessDesignDrawPartIdList = getNewProcessDesignDrawPartIdValueList(maxProcessDesignDrawPartIdInSqlServer, processDesignDrawPartIdDifferenceList);
                processDesignDrawPartIdUpdateInfoList = changeProcDesignDrawPartIdsInDBFileByUpdatedList(processDesignDrawPartIdsInDbFile, newProcessDesignDrawPartIdList, allTableWithProcDesignDrawPartIds, connectionManager);

                processDesignDrawPartIdUpdateInfoList.Add(" processDesignDrawPartIdsInDbFile : ");
                processDesignDrawPartIdUpdateInfoList.AddRange(convertIntListToStringList(processDesignDrawPartIdsInDbFile));
                changetempProcDesignDrawPartIdsInDBFileToRealNewtempProcDesignDrawPartIds(allTableWithProcDesignDrawPartIds, connectionManager);

                changingProcessDesignDrawPartIdsInfoList.Add("newprocessDesignDrawPartIdList : ");
                changingProcessDesignDrawPartIdsInfoList.AddRange(convertIntListToStringList(newProcessDesignDrawPartIdList));
                changingProcessDesignDrawPartIdsInfoList.AddRange(processDesignDrawPartIdUpdateInfoList);
            }
            catch (Exception ex)
            {
                changingProcessDesignDrawPartIdsInfoList.Add(ex.ToString());
            }
            return changingProcessDesignDrawPartIdsInfoList;
        }

        public static List<string> convertIntListToStringList(List<int> inputStringList)
        {
            List<string> convertedStringList = inputStringList.ConvertAll<string>(delegate (int i) { return i.ToString(); });
            return convertedStringList;
        }
    }

}