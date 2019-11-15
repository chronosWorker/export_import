using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public static class ActivityManager
    {

        public static List<int> activityIdsInDbFile = new List<int>();
        public static List<int> activityIdDifferenceList = new List<int>();
        public static List<string> activityIdUpdateInfo = new List<string>();
        public static List<int> newActivitIdList = new List<int>();

        public static List<int> getMaxActivityIdFromSQLServer(ConnectionManagerST obj)
        {
            List<int> maxActivityIdList = new List<int>();

            try
            {
                var reader = obj.sqlServerDataReader("Select max(Activity_id) as Max_Activity_Id from T_ACTIVITY ");
                while (reader.Read())
                {
                    maxActivityIdList.Add(Convert.ToInt32(reader["Max_Activity_Id"]));
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxActivityIdList;

        }


        public static List<int> getActivityIdsInOrderFromDBFile(ConnectionManagerST obj)
        {
            List<int> maxActivityIdList = new List<int>();

            try
            {
                var reader = obj.sqLiteDataReader("Select distinct(Activity_id) from T_ACTIVITY  order by Activity_id desc ");
                while (reader.Read())
                {
                    maxActivityIdList.Add(Convert.ToInt32(reader["Activity_id"]));
                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return maxActivityIdList;

        }

        public static List<int> getActivityIdDifferences(List<int> Activity_Ids)
        {
            List<int> activityIdDifferenceList = new List<int>();

            try
            {

                for (int outerIndex = 1; outerIndex < Activity_Ids.Count; outerIndex++)
                {

                    int difference = Activity_Ids[outerIndex - 1] - Activity_Ids[outerIndex];
                    activityIdDifferenceList.Add(difference);


                }

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return activityIdDifferenceList;

        }


        public static List<string> changeActivityIdsInDBFileByUpdatedList(List<int> oldActivityIdList, List<int> newActivityIdList, List<string> tablesWithActivityId, ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();

            foreach (string tableName in tablesWithActivityId)
            {
                for (int index = 0; index < newActivityIdList.Count; index++)
                {
                    int tempActivityId = 10000000 + newActivityIdList[index];
                    string updateText = "Update " + tableName + " Set Activity_ID = " + tempActivityId.ToString() + " where Activity_Id = " + oldActivityIdList[index].ToString();
                    obj.executeQueriesInDbFile(updateText);
                    updateInfo.Add(updateText);
                }
            }

            return updateInfo;

        }

        public static List<string> changeActivityIdsInDBFileToRealNewActivityID(List<string> tablesWithActivityId, ConnectionManagerST obj)
        {
            List<string> updateInfo = new List<string>();

            foreach (string tableName in tablesWithActivityId)
            {
                int tempActivityIdToDistract = 10000000;
                string updateText = "Update " + tableName + " Set Activity_ID =  Activity_ID  - " + tempActivityIdToDistract.ToString() + " where 1 = 1 ;";
                obj.executeQueriesInDbFile(updateText);
                updateInfo.Add(updateText);
            }

            return updateInfo;

        }


        public static List<string> getAllTableNameWithActivityIdInDBFile(ConnectionManagerST obj)
        {
            List<string> tablesWithActivityId = new List<string>();
            string queryText = "Select distinct(table_name) from table_information where Column_Name = 'Activity_ID';";
            var reader = obj.sqLiteDataReader(queryText);
            while (reader.Read())
            {
                tablesWithActivityId.Add(reader["table_name"].ToString());
            }
            return tablesWithActivityId;
        }

        public static List<int> getNewActivityIdValueList(int maxActivityIdInSQLServer, List<int> activityIdDifferenceList)
        {
            List<int> updatedActivityIdList = new List<int>();
            int newMaxActivityId = maxActivityIdInSQLServer + 1;
            updatedActivityIdList.Add(newMaxActivityId);
            foreach (int difference in activityIdDifferenceList)
            {
                updatedActivityIdList.Add(updatedActivityIdList[updatedActivityIdList.Count - 1] + difference);
            }


            return updatedActivityIdList;

        }

    }
}