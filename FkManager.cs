﻿using System;
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

		public List<int> fieldIdsWithProcAliasIdForDocRef(List<int> oldIdList,  ConnectionManagerST obj)
		{
			List<int> fieldIdsWithProcAliasIdList = new List<int>();
			try
			{
				foreach (int oldId in oldIdList)
				{
					string selectText = " Select Field_Id from T_FIELD_VALUE where Field_Value_ID in ( select Default_Field_Value_ID  from t_field where Document_Ref_List_Type = 2 )  and Field_Value = " + oldId.ToString() + " order by field_value asc ;";
					var reader = obj.sqLiteDataReader(selectText);
					while (reader.Read())
					{
						fieldIdsWithProcAliasIdList.Add(Convert.ToInt32(reader["Field_Id"]));
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return fieldIdsWithProcAliasIdList;
		}

		public List<string> changeFieldValuesForDocRef(List<int> fieldIdsWithProcAliasIdList, List<int> oldIdList  , List<int> newIdList, ConnectionManagerST obj)
		{
			List<string> updateTextList = new List<string>();
			try
			{
                foreach (int fieldId in fieldIdsWithProcAliasIdList)
                {
                    for (var i = 0; i < oldIdList.Count(); i++)
                    {
					    string updateText = " UPDATE T_FIELD_VALUE SET Field_Value = " + newIdList[i].ToString() + " where Field_Id =  " + fieldId + " and Field_Value = " + oldIdList[i].ToString();
					    obj.executeQueriesInDbFile(updateText);
					    updateTextList.Add(updateText);
                    }
                }

			}
			catch (Exception ex)
			{
				throw ex;
			}
			return updateTextList;
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
						//        updateInfo.Add("Table where old Id Found : " + table);
						string updateCommandText = " Update " + table + " Set " + IdName + " = " + newIdList[oldIddListIndex].ToString() + " where " + IdName + " = " + oldIdList[oldIddListIndex].ToString();
						obj.executeQueriesInDbFile(updateCommandText);
						updateInfo.Add(updateCommandText);

						//ha találok olyan értéket ami routing_design activity_des_id-ban ami benne van ACTIVITY_DESIGn-ban  from vagy to act_design id-ként akkor azokat ugyanarra updatelem
						if (IdName == "Activity_Design_ID")
						{
							string updateFromActivityDesignCommandText = " Update T_ROUTING_DESIGN  Set From_Activity_Design_ID = " + newIdList[oldIddListIndex].ToString() + " where From_Activity_Design_ID = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateFromActivityDesignCommandText);
							string updateToActivityDesignCommandText = " Update T_ROUTING_DESIGN  Set To_Activity_Design_ID = " + newIdList[oldIddListIndex].ToString() + " where To_Activity_Design_ID = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateToActivityDesignCommandText);
						}
						if (IdName == "Field_ID" && TableName == "T_FIELD")
						{
							string updateDependentFieldId = " Update T_FIELD_TO_FIELD_DEPENDENCY  Set Dependent_Field_ID = " + newIdList[oldIddListIndex].ToString() + " where Dependent_Field_ID = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateDependentFieldId);
							//updateInfo.Add(updateDependentFieldId);
							string updateIndependentFieldId = " Update T_FIELD_TO_FIELD_DEPENDENCY  Set Independent_Field_ID = " + newIdList[oldIddListIndex].ToString() + " where Independent_Field_ID = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateIndependentFieldId);
							//updateInfo.Add(updateIndependentFieldId);
						}
						if (IdName == "Activity_ID")
						{
							string updateDependencyActivationId = "Update T_FIELD_TO_FIELD_DEPENDENCY Set Dependency_Activation_Activity_ID = " + newIdList[oldIddListIndex].ToString() + " where Dependency_Activation_Activity_ID = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateDependencyActivationId);

							string updateNotificationTriggerFromActivatyId = "Update T_NOTIFICATION_TRIGGER Set From_Activity = " + newIdList[oldIddListIndex].ToString() + " where From_Activity = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateNotificationTriggerFromActivatyId);

							string updateNotificationTriggerToActivatyId = "Update T_NOTIFICATION_TRIGGER Set To_Activity = " + newIdList[oldIddListIndex].ToString() + " where To_Activity = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateNotificationTriggerToActivatyId);

							string updateSystemInterfaceTriggerToActivatyId = "Update T_SYSTEM_INTERFACE_TRIGGER Set To_Activity = " + newIdList[oldIddListIndex].ToString() + " where To_Activity = " + oldIdList[oldIddListIndex].ToString() + " ;";
							// updateInfo.Add(updateSystemInterfaceTriggerToActivatyId);
							obj.executeQueriesInDbFile(updateNotificationTriggerToActivatyId);

							string updateSystemInterfaceTriggerFromActivatyId = "Update T_SYSTEM_INTERFACE_TRIGGER Set From_Activity = " + newIdList[oldIddListIndex].ToString() + " where From_Activity = " + oldIdList[oldIddListIndex].ToString() + " ;";
							// updateInfo.Add(updateSystemInterfaceTriggerFromActivatyId);
							obj.executeQueriesInDbFile(updateNotificationTriggerToActivatyId);
						}
						if (IdName == "Process_ID")
						{
							string updateParentProcessIdTxt = "Update T_PROCESS SET Parent_Process_ID  = " + newIdList[oldIddListIndex].ToString() + " where Parent_Process_ID = " + oldIdList[oldIddListIndex].ToString() + " ;";
							obj.executeQueriesInDbFile(updateParentProcessIdTxt);
						}
						
					}

				}
			}
			//kell egy lista amiben betárazza melyik értékekre kell updatelni és csak azokat most tul sokszor futkosik le

			/*   if (IdName == "Activity_Id")
			   { 
				   List<string> fromToActivityIdTableList = new List<string>() { "T_SYSTEM_INTERFACE_TRIGGER", "T_NOTIFICATION_TRIGGER" };
				   foreach (string tableName in fromToActivityIdTableList)
				   {
					   for (int oldIddListIndex = 0; oldIddListIndex < oldIdList.Count; oldIddListIndex++)
					   {
							   string commandText = "Select Count(*) from " + tableName + " where " + IdName + " = " + oldIdList[oldIddListIndex].ToString();



					   }
				   }

			   }*/
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
			List<string> processNameList = new List<string>();
			int processImportedQuantity = 0;
			var reader = obj.sqLiteDataReader("Select Name from T_PROCESS");
			while (reader.Read())
			{
				processNameList.Add(reader["Name"].ToString());
			}
			foreach (string processName in processNameList)
			{
				processImportedQuantity += selectCountImportedProcesses(obj, processName);
			}
			if (processImportedQuantity == 0)
			{
				foreach (string processName in processNameList)
				{
					string newProcessName = processName + " (IMPORTED)";
					string newProcessTechnicalName = processName.Replace(" ", "_") + "_IMPORTED";
					string newProcessNameUpdateQuery = " UPDATE T_PROCESS SET NAME = '" + newProcessName + "' WHERE NAME =  '" + processName + "'";
					obj.executeQueriesInDbFile(newProcessNameUpdateQuery);
				}
			}
			else
			{
				foreach (string processName in processNameList)
				{
					string newProcessName = processName + " (IMPORTED_" + processImportedQuantity.ToString() + ")";
					string newProcessTechnicalName = processName.Replace(" ", "_") + "_IMPORTED_" + processImportedQuantity.ToString();
					string newProcessNameUpdateQuery = " UPDATE T_PROCESS SET NAME = '" + newProcessName + "' WHERE NAME =  '" + processName + "'";
					obj.executeQueriesInDbFile(newProcessNameUpdateQuery);
				}
			}
			return newNameList;

		}

		public List<string> changeProcessDesignName(ConnectionManagerST obj)
		{
			List<string> newDesignNameList = new List<string>();
			string designName = "";
			int designId = 0;
			var reader = obj.sqLiteDataReader("Select Name from T_PROC_DESIGN_DRAW");
			while (reader.HasRows && reader.Read())
			{
				designName = reader["Name"].ToString();
			}
			var reader2 = obj.sqLiteDataReader("Select Process_Design_ID from T_PROC_DESIGN_DRAW");
			while (reader2.HasRows && reader2.Read())
			{
				designId = Convert.ToInt32(reader2["Process_Design_ID"]);
			}
			newDesignNameList.Add("oiginal processName : ");
			newDesignNameList.Add(designName);
			newDesignNameList.Add("new processName :  : ");
			int processImportedDesignQuantity = selectCountImportedProcessesDesignName(obj, designName);
			if (processImportedDesignQuantity == 0)
			{
				string newdesignName = designName + " (IMPORTED)";
				string newDesignNameUpdateQuery = " UPDATE T_PROC_DESIGN_DRAW SET NAME = '" + newdesignName + "' WHERE Process_Design_ID = + '" + designId.ToString() + "'";
				string newProcDesignNameUpdateQuery = " UPDATE T_PROCESS_DESIGN SET NAME = '" + newdesignName + "' WHERE Process_Design_ID = + '" + designId.ToString() + "'";
				obj.executeQueriesInDbFile(newDesignNameUpdateQuery);
				obj.executeQueriesInDbFile(newProcDesignNameUpdateQuery);
			}
			else
			{
				string newdesignName = designName + " (IMPORTED_" + processImportedDesignQuantity.ToString() + ")";
				string newDesignNameUpdateQuery = " UPDATE T_PROC_DESIGN_DRAW SET NAME = '" + newdesignName + "' WHERE Process_Design_ID = + '" + designId.ToString() + "'";
				string newProcDesignNameUpdateQuery = " UPDATE T_PROCESS_DESIGN SET NAME = '" + newdesignName + "' WHERE Process_Design_ID = + '" + designId.ToString() + "'";
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
				//Ide jön  az újítás




				int maxIdInSqlServer = getMaxIdFromSQLServer(connectionManager);
				idsInDbFile = getIdsInOrderFromDBFile(connectionManager);
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


					changingIdsInfoList.Add(maxIdInSqlServer.ToString());

					changingIdsInfoList.AddRange(convertIntListToStringList(idDifferenceList));

					changingIdsInfoList.AddRange(convertIntListToStringList(newIdList));
					if (IdName == "Process_Alias_ID")
					{
						List<int> fieldIdsWithProcAliasIdForDocRefList = new List<int>();
						fieldIdsWithProcAliasIdForDocRefList = fieldIdsWithProcAliasIdForDocRef(idsInDbFile, connectionManager);
						changeFieldValuesForDocRef(fieldIdsWithProcAliasIdForDocRefList, idsInDbFile,  newIdList, connectionManager);

					}
					changeIdsInAllRelatedTableIfSameRecordFound(idsInDbFile, newIdList, connectionManager);
					//changingIdsInfoList.AddRange(idUpdateInfo);
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

		public List<string> compareTwoTypeTableListToGetSameRecords(List<string> twoDimensionalTypeTableValuesInDbFile, List<string> twoDimensionalTypeTableValuesInSqlServer)
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

		public void deleteSameRecordsFromTypeTable(ConnectionManagerST connectionManager, List<string> recordsToDelete)
		{
			try
			{
				foreach (string record in recordsToDelete)
				{
					if (record.Contains("'"))
					{
						string currentRecordSingleQuoteFormalized = record.Replace("'", "''");
						string deleteQuery = "Delete From " + TableName + " where " + IdName + " = '" + currentRecordSingleQuoteFormalized + "';";
						connectionManager.executeQueriesInDbFile(deleteQuery);
					}
					else
					{
						string deleteQuery = "Delete From " + TableName + " where " + IdName + " = '" + record + "';";
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

		//Notification raelted functions

		//     public bool checkIfNotificationAddressBeenSet(ConnectionManagerST connectionManager)
		//    {
		//        string query = ""
		//  }
		//Set importőr person to process owner
		public int getProcessOwnerType(ConnectionManagerST connectionManager)
		{
			int processOwnerType = -1;
			try
			{
				string query = "Select PARTICIPANT_TYPE from T_PROCESS_OWNER";
				var reader = connectionManager.sqLiteDataReader(query);
				while (reader.Read())
				{
					processOwnerType = Convert.ToInt32(reader["PARTICIPANT_TYPE"]);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return processOwnerType;
		}

		public void setImportPersonToProcessOwner(ConnectionManagerST connectionManager, int importPersonId)
		{
			int processOwnerType = getProcessOwnerType(connectionManager);
			if (processOwnerType == 1)
			{
				string updateOwnerQuery = "UPDATE T_PROCESS_OWNER SET Participant_ID = " + importPersonId.ToString() + " where 1 = 1";
				connectionManager.executeQueriesInDbFile(updateOwnerQuery);
			}
		}


		public bool detectNotificationEmailAddress(ConnectionManagerST obj)
		{
			bool foundNotificationAddress = false;
			string commandText = "select count(address_email) as 'mail_address' from T_NOTIFICATION_ADDRESS";
			var reader = obj.sqLiteDataReader(commandText);
			try
			{
				while (reader.Read())
				{
					if (Convert.ToInt32(reader["mail_address"]) != 0)
					{
						foundNotificationAddress = true;
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return foundNotificationAddress;
		}

		public void deleteNotificationAddresses(ConnectionManagerST obj)
		{
			string commandText = "UPDATE T_NOTIFICATION_ADDRESS SET Address_Email = ' '  where 1 = 1";
			obj.executeQueriesInDbFile(commandText);
		}

		public List<KeyValuePair<string, int>> detectActivityParticipants(ConnectionManagerST obj)
		{
			var ActivityParticipants = new List<KeyValuePair<string, int>>();
			string commandText = "Select distinct act.Name, act.Activity_ID , actP.Participant_type from T_ACTIVITY_PARTICIPANT actP left join t_ACTIVITY act on actP.Activity_Id = act.Activity_Id";
			var reader = obj.sqLiteDataReader(commandText);
			try
			{
				while (reader.Read())
				{
					if (reader["participant_type"] != "")
					{
						if (Convert.ToInt32(reader["participant_type"]) == 1)
						{
							ActivityParticipants.Add(new KeyValuePair<string, int>(reader["Name"].ToString(), Convert.ToInt32(reader["Activity_ID"])));
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return ActivityParticipants;
		}

		public List<KeyValuePair<string, string>> detectNotificationAddresses(ConnectionManagerST obj)
		{
			var NotificationAddresses = new List<KeyValuePair<string, string>>();
			string commandText = "select distinct  noA.address_email ,  noti.Notification_Name  from T_NOTIFICATION_ADDRESS noA left join T_NOTIFICATION noti on noti.Notification_ID = noA.Notification_ID;";
			var reader = obj.sqLiteDataReader(commandText);
			try
			{
				while (reader.Read())
				{
					NotificationAddresses.Add(new KeyValuePair<string, string>(reader["Notification_Name"].ToString(), reader["address_email"].ToString()));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return NotificationAddresses;
		}
		//
		public List<KeyValuePair<string, string>> getNullableTableAndColumnName(ConnectionManagerST obj)
		{
			List<KeyValuePair<string, string>> nullaAbleTableAndColumns = new List<KeyValuePair<string, string>>();
			string detectcommandText = "Select table_name , column_name from table_information where is_nullable = 'YES'";
			var reader = obj.sqLiteDataReader(detectcommandText);
			try
			{
				while (reader.Read())
				{
					nullaAbleTableAndColumns.Add(new KeyValuePair<string, string>(reader["table_name"].ToString(), reader["column_name"].ToString()));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return nullaAbleTableAndColumns;
		}

		public void updateNullableEmptyFieldsToNull(ConnectionManagerST obj)
		{
			foreach (KeyValuePair<string, string> tableColumnNamePair in getNullableTableAndColumnName(obj))
			{
				string updateNullCommandText = "Update " + tableColumnNamePair.Key + " SET " + tableColumnNamePair.Value + " = NULL  where " + tableColumnNamePair.Value + " = '' ;";
				obj.executeQueriesInDbFile(updateNullCommandText);
			}
		}

		/*  public string checkNotificaionEmailAddressIfFoundSendBackData(ConnectionManagerST obj)
		  {
			  if(detectNotificationEmailAddress(obj))
			  {
				  deleteNotificationAddresses(obj);
			  }

		  }*/

		//
		public int getMainProcessId(ConnectionManagerST obj)
		{
			int mainProcessId = 0;
			string commandText = "Select Process_Id from T_PROCESS where Parent_Process_ID  is null;";
			var reader = obj.sqLiteDataReader(commandText);
			try
			{
				while (reader.Read())
				{
					mainProcessId = Convert.ToInt32(reader["Process_Id"]);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return mainProcessId;

		}

        public List<int> getProcessIdList(ConnectionManagerST obj)
        {
            List<int> processIdList = new List<int>();
            string commandText = "select distinct process_id from t_process;";
            var reader = obj.sqLiteDataReader(commandText);
            try
            {
                while (reader.Read())
                {
                    processIdList.Add(Convert.ToInt32(reader["process_id"]));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return processIdList;

        }


        public void updateMainProcessIdForSubprocesses(ConnectionManagerST obj, int mainProcessId)
		{
			try
			{
				string commandText = "Update T_PROCESS set Parent_Process_ID = " + mainProcessId.ToString() + " where Parent_Process_ID is not null;";
				obj.executeQueriesInDbFile(commandText);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public List<KeyValuePair<int, int>> selectIdRowIdFromTable(ConnectionManagerST obj, string tableName, string idName)
		{
			List<KeyValuePair<int, int>> idValueRowIDValuePairs = new List<KeyValuePair<int, int>>();
			string selectTxt = "select " + idName + " , ROWID from " + tableName;
			var reader = obj.sqLiteDataReader(selectTxt);
			try
			{
				while (reader.Read())
				{
					idValueRowIDValuePairs.Add(new KeyValuePair<int, int>(Convert.ToInt32(reader[idName]), Convert.ToInt32(reader["ROWID"])));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return idValueRowIDValuePairs;
		}

		public List<KeyValuePair<int, int>> selectIdRowIdFromTable2(ConnectionManagerST obj)
		{
			List<KeyValuePair<int, int>> idValueRowIDValuePairs = new List<KeyValuePair<int, int>>();
			string selectTxt = "select " + IdName + " , ROWID from " + TableName;
			var reader = obj.sqLiteDataReader(selectTxt);
			try
			{
				while (reader.Read())
				{
					idValueRowIDValuePairs.Add(new KeyValuePair<int, int>(Convert.ToInt32(reader[IdName]), Convert.ToInt32(reader["ROWID"])));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return idValueRowIDValuePairs;
		}


		public List<int> selectMultipleOccurenceValueList(List<KeyValuePair<int, int>> rowAndIdList)
		{
			var IdList = new List<int>();
			foreach (var entry in rowAndIdList)
			{
				IdList.Add(entry.Value);
			}
			var MultipleOccurenceValueList = IdList.GroupBy(x => x).Where(y => y.Count() > 1).Select(z => z.Key).ToList();

			return MultipleOccurenceValueList;

		}


		public void deleteMultipleOccurenceIdValueFromTable(ConnectionManagerST connectionManager, TableManager TM)
		{
			List<KeyValuePair<string, string>> tableNameWitkFkIdNameList = TM.getTableNameWitkFkIdNameList();

			foreach (KeyValuePair<string, string> tableAndIdNamePair in tableNameWitkFkIdNameList)
			{
				var idAndRowIdValueList = new List<KeyValuePair<int, int>>();
				var duplicatedIdValuesList = new List<int>();
				var lowestRowIdForDuplicatedIdValues = new List<KeyValuePair<int, int>>();
				idAndRowIdValueList = selectIdRowIdFromTable(connectionManager, tableAndIdNamePair.Key, tableAndIdNamePair.Value);
				duplicatedIdValuesList = areThereDuplicates(idValues(idAndRowIdValueList));
				lowestRowIdForDuplicatedIdValues = selectLowestRowIdFromTableToIds(idAndRowIdValueList, duplicatedIdValuesList);
				deleteSameRecord(connectionManager, lowestRowIdForDuplicatedIdValues, tableAndIdNamePair.Key, tableAndIdNamePair.Value);
			}



		}

		/*  public void deleteMultipleOccurenceIdValueFromTable2(ConnectionManagerST connectionManager)
		  {

				  var idAndRowIdValueList = selectIdRowIdFromTable2(connectionManager);
				  var duplicatedIdValuesList = areThereDuplicates(idValues(idAndRowIdValueList));
				  var LoiwestRowIdForDuplicatedIdValues = selectLowestRowIdFromTableToIds(idAndRowIdValueList, duplicatedIdValuesList);
				  deleteSameRecord(connectionManager, LoiwestRowIdForDuplicatedIdValues);

		  }*/


		public List<int> idValues(List<KeyValuePair<int, int>> idValueRowIDValuePairs)
		{
			List<int> idValues = new List<int>();
			foreach (KeyValuePair<int, int> entry in idValueRowIDValuePairs)
			{
				idValues.Add(entry.Key);
			}
			return idValues;
		}


		public List<int> rowIdValues(List<KeyValuePair<int, int>> idValueRowIDValuePairs)
		{
			List<int> RowIdValues = new List<int>();
			foreach (KeyValuePair<int, int> entry in idValueRowIDValuePairs)
			{
				RowIdValues.Add(entry.Value);
			}
			return RowIdValues;
		}
		public List<int> areThereDuplicates(List<int> inputIntList)
		{
			inputIntList.Sort();
			List<int> duplicates = inputIntList.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
			return duplicates;
		}



		public List<KeyValuePair<int, int>> selectLowestRowIdFromTableToIds(List<KeyValuePair<int, int>> rowAndIdValueList, List<int> rowInputList)
		{
			List<KeyValuePair<int, int>> lowestRowIAndValueForMultipleRecordList = new List<KeyValuePair<int, int>>();
			List<KeyValuePair<int, int>> sortedRowAndIdValueList = new List<KeyValuePair<int, int>>();
			rowAndIdValueList.Sort((a, b) => (a.Key.CompareTo(b.Key)));
			var sortedMultipleRecordList = new List<int>();
			rowInputList.Sort((a, b) => a.CompareTo(b));


			foreach (int rowNum in rowInputList)
			{
				var lowestValuePairForEachKey = rowAndIdValueList.Where(p => p.Key == rowNum).OrderBy(k => k.Value).FirstOrDefault();
				if (lowestValuePairForEachKey.Key != 0)
				{
					lowestRowIAndValueForMultipleRecordList.Add(lowestValuePairForEachKey);
				}
			}

			return lowestRowIAndValueForMultipleRecordList;
		}

		/*   public void deleteSameRecord(ConnectionManagerST obj, List<KeyValuePair<int, int>> idValueRowIDValuePairs)
		   {
				   foreach (KeyValuePair<int, int> entry in idValueRowIDValuePairs)
				   {
					   string deleteText = "Delete from " + TableName + " Where " + IdName + " = " + entry.Key.ToString() + " AND ROWID != " + entry.Value.ToString();
					   obj.executeQueriesInDbFile(deleteText);
				   }
		   }
		   */

		public void deleteSameRecord(ConnectionManagerST obj, List<KeyValuePair<int, int>> idValueRowIDValuePairs, string tableName, string idName)
		{
			foreach (KeyValuePair<int, int> entry in idValueRowIDValuePairs)
			{
				string deleteText = "Delete from " + tableName + " Where " + idName + " = " + entry.Key.ToString() + " AND ROWID != " + entry.Value.ToString();
				obj.executeQueriesInDbFile(deleteText);
			}
		}

		//  public void updateDocRefValues(ConnectionManagerST obj)
		//  {

		//  }

		public List<int> selectdFieldIdsForDocRefList(ConnectionManagerST obj)
		{
			List<int> FieldIdsForDocRefList = new List<int>();
			string selectText = "	select  Field_Id from t_field where  Document_Ref_List_Type = 2; ";
			var reader = obj.sqLiteDataReader(selectText);
			try
			{
			   
				while (reader.Read())
				{
					FieldIdsForDocRefList.Add(Convert.ToInt32(reader["Field_Id"]));
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return FieldIdsForDocRefList;
		}

		public List<KeyValuePair<int, int>> selectFieldValueAndFieldId(ConnectionManagerST obj, List<int> FieldIdsForDocRefList)
		{
			List<KeyValuePair<int, int>> FieldValueAndFieldIdList = new List<KeyValuePair<int, int>>();
			try
			{
				foreach (int fieldId in FieldIdsForDocRefList)
				{
					string selectText = " select Field_Value_ID  , Field_Id from t_field_value where Field_Id = " + fieldId.ToString();
					var reader = obj.sqLiteDataReader(selectText);
					while (reader.Read())
					{
						FieldValueAndFieldIdList.Add(new KeyValuePair<int, int>(Convert.ToInt32(reader["Field_Value_ID"]), Convert.ToInt32(reader["Field_Id"])));
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return FieldValueAndFieldIdList;

		}



		public void updateDocRefValues(ConnectionManagerST obj, List<KeyValuePair<int, int>> defaultFieldValueAndFieldIdList)
		{
			try
			{
				foreach (var entry in defaultFieldValueAndFieldIdList)
				{
					string updateText = "Update T_FIELD set Default_Field_Value_ID = " + entry.Key + " where field_id = " + entry.Value;
					obj.executeQueriesInDbFile(updateText);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

	}
}
 