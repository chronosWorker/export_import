using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using static System.Collections.IEnumerable;
using static System.Data.Common.DbDataReader;
using System.ServiceModel.Activation;


namespace Process_Export_Import
{

	[ServiceContract(Namespace = "")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]

	public class Process_Export_Import
	{


		private List<ProcessListItem> processes = new List<ProcessListItem>();
		private List<TableNameAndCondition> tables = new List<TableNameAndCondition>();
		private List<long> fieldsForProcess = new List<long>();
		private List<long> operands = new List<long>();
		private List<long> udts = new List<long>();
		private List<ReportListItem> reports = new List<ReportListItem>();
		private List<long> t_report_calculated_field_formula_tree_nodes = new List<long>();
		private List<long> reportFields = new List<long>();
		private List<long> udtReportFields = new List<long>();
		private List<long> activities = new List<long>();
		private List<long> activityOwnerByCondition = new List<long>();

		string connStrSQLite;
		SQLiteConnection connSqlite;
		string sqliteDbPath;

		public Process_Export_Import()
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			connSqlite = new SQLiteConnection();

		}




		[DataContract]
		public class ServiceCallResult
		{
			[DataMember]
			public string Source { get; set; }
			[DataMember]
			public int Code { get; set; }
			[DataMember]
			public string Description { get; set; }
			[DataMember]
			public string ExceptionContent { get; set; }
			[DataMember]
			public string InnerExceptionContent { get; set; }

			public static implicit operator ServiceCallResult(string v)
			{
				throw new NotImplementedException();
			}
		}

		[OperationContract]
		public ServiceCallResult Export_Process(int processId)
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			res = getSqlitePath(processId);
			sqliteDbPath = res.Description;
			bool processIdExistInLocalDb = CheckIfProcessExistInDatabase(processId);
			connSqlite.ConnectionString = string.Format("Data Source={0} ;Version=3;", res.Description);
			res = createDatabaseAndTables(processId);
			if (res.Code != 0)
			{
				return res;
			}
			connSqlite.Open();
			try
			{

				processes = new List<ProcessListItem>();
				FillProcesses(processId);
				fieldsForProcess = new List<long>();

				if (processes.Count > 0)
				{
					for (int i = 0; i < processes.Count; i++)
					{
						res = TransferProcess(processes[i].ProcessId);
					}
				}

				if (processIdExistInLocalDb)
				{

					res.Description = "MAXiD:" + detectMaxProcessId() + ".";

				}

				connSqlite.Close();
			}
			catch (Exception e)
			{
				connSqlite.Close();
				res = FillServiceCallResult(e);

			}

			return res;

		}



		public ServiceCallResult Import_Process_teszt(string name)
		{
			string connstrRe = ConfigurationManager.AppSettings.Get("connstrRe");
			string strSQL = "SELECT count(*) from T_PROCESS WHERE   Name = @parameter ";
			bool processNameFound = false;
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			using (SqlConnection connection = new SqlConnection(connstrRe))
			{
				SqlCommand command = new SqlCommand(strSQL, connection);
				command.Parameters.AddWithValue("@parameter", name);
				try
				{
					connection.Open();
					SqlDataReader reader;
					reader = command.ExecuteReader();
					reader.Read();
					if ((int)reader[0] == 1)
					{
						processNameFound = true;
					}

					reader.Close();
				}
				catch (Exception ex)
				{
					res = FillServiceCallResult(ex);
				}
			}
			res.Description = "van ilyen?" + processNameFound.ToString();

			return res;

		}
		[OperationContract]
		//	[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, UriTemplate = "postmethod/new")]

		public List<string> Import_Process()
		{

			List<string> insertResultInfo = new List<string>();
			var connectionManager = new ConnectionManagerST();

		//	List<string> allTableWithProcessDesignId = new List<string>() { "T_PROCESS", "T_PROCESS_DESIGN", "T_PROC_DESIGN_DRAW", "T_ROUTING_DESIGN", "T_ACTIVITY_DESIGN" };
			connectionManager.openSqLiteConnection();
			connectionManager.openSqlServerConnection();

			bool isTheDBStructuresAreTheSame = verifyThatDBStructuresAreTheSame(checkingDBStructureDifferences(connectionManager));
			if (isTheDBStructuresAreTheSame)
			{
				try
				{

                  /*  FkManager activityId = new FkManager ("T_ACTIVITY" , "Activity_ID");
                    insertResultInfo.AddRange(activityId.changeAllIdInDbFileToFitSqlServer(connectionManager));
                    FkManager processId = new FkManager ("T_PROCESS" , "Process_ID");
                    insertResultInfo.AddRange(processId.changeAllIdInDbFileToFitSqlServer(connectionManager));
                    FkManager activityDesignId = new FkManager ("T_ACTIVITY_DESIGN" , "Activity_Design_ID");
                    insertResultInfo.AddRange(activityDesignId.changeAllIdInDbFileToFitSqlServer(connectionManager));*/
                    FkManager fieldId = new FkManager ("T_FIELD" , "Field_ID");
                    insertResultInfo.AddRange(fieldId.changeAllIdInDbFileToFitSqlServer(connectionManager));



                    //	int maxProcessIdInSqlServer = Convert.ToInt32(getMaxdProcessIdFromSQLServer(connectionManager).First());
                    //	insertResultInfo.Add("getMaxdProcessIdFromSQLServer" + maxProcessIdInSqlServer.ToString());

                    //	insertResultInfo.AddRange(ActivityManager.changeAllActivityIdInDbFileToFitSqlServer(connectionManager));
                    //	insertResultInfo.AddRange(changeAllProcessDesignIdsInDBFileByNewValue(getMaxProcessDesignIdFromSQLServer(connectionManager), allTableWithProcessDesignId, connectionManager ));
                    //	foreach(string tableName in allTableWithProcessDesignId)
                    //	{
                    //	insertResultInfo.AddRange(getColumnTypesDictionary_v2(tableName , connectionManager).Keys);
                    //	}
                    //getMaxProcessDesignDrawIdFromSQLServer(connectionManager);
                    /*		List<string> allTableWithProcDesignDrawPartIds = new List<string>() { "T_PROC_DESIGN_DRAW_PART" };
                            int newMAx = getMaxProcessDesignDrawPartIdFromSQLServer(connectionManager);
                            insertResultInfo.Add("new max proc des id : ");
                            insertResultInfo.Add(newMAx.ToString());

                    getProcDesignDrawPartIdDifferences(getProcDesignDrawPartIdsInOrderFromDBFile(connectionManager));
					List<int>  uj = getNewProcessDesignDrawPartIdValueList(newMAx, getProcDesignDrawPartIdDifferences(getProcDesignDrawPartIdsInOrderFromDBFile(connectionManager)));
					changeProcDesignDrawPartIdsInDBFileByUpdatedList(getProcDesignDrawPartIdsInOrderFromDBFile(connectionManager), uj, allTableWithProcDesignDrawPartIds, connectionManager);

					insertResultInfo.AddRange(changetempProcDesignDrawPartIdsInDBFileToRealNewtempProcDesignDrawPartIds(allTableWithProcDesignDrawPartIds, connectionManager));*/
                    //    changetempProcDesignDrawPartIdsInDBFileToRealNewtempProcDesignDrawPartIds(allTableWithProcDesignDrawPartIds, connectionManager);
                    //changeProcDesignDrawPartIdsInDBFileByUpdatedList();
                    //	insertResultInfo.AddRange(insertValuesFromDbFileToSqlServer("T_PROC_DESIGN_DRAW", true , connectionManager));
                    //	;
                    //	insertResultInfo.AddRange(changeAllProcessDesignDrawIdsInDBFileByNewValue(getMaxProcessDesignDrawIdFromSQLServer(connectionManager), connectionManager));
                    //     insertValuesFromDbFileToSqlServer();


                    //insertResultInfo.AddRange(changeActivityIdsInDBFileToFitSQLServer(maxActivityIdInSqlServer, connectionManager));
                    //insertResultInfo.AddRange(changeProcessIDsInDBFileToFitSQLServer(maxProcessIdInSqlServer , connectionManager));

                }
				catch(Exception ex)
				{
					insertResultInfo.Add(ex.Message.ToString() + ex.StackTrace.ToString());
				}
			}
			connectionManager.closeSqLiteConnection();
			connectionManager.closeSqlServerConnection();

			return insertResultInfo;


		}
		


	
		public List<string> getMaxdProcessIdFromSQLServer(ConnectionManagerST obj)
		{
			List<string> maxProcessIdList = new List<string>();

			try
			{
				var reader = obj.sqlServerDataReader("SELECT MAX(Process_Id) as 'Max_Process_Id' FROM T_PROCESS");
				while (reader.Read())
				{
					maxProcessIdList.Add(reader["Max_Process_Id"].ToString());
				}

			}
			catch (Exception ex)
			{
				throw ex;

			}

			return maxProcessIdList;

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
				columnTypes = getColumnTypesDictionary_v2(tableName , obj);

				var reader = obj.sqLiteDataReader("SELECT * FROM " + tableName);

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


				while (reader.Read())
				{
					for (var index = 0; index < columnTypes.Count; index++)
					{
						switch (columnTypes.ElementAt(index).Value)
						{
							case "NULL":
								values.Add("0");
								break;
							case "bit":
							case "binary":
							case "varbinary":
							case "image":
							case "DateTime":
							case "nvarchar":
								values.Add("'" + reader[columnTypes.ElementAt(index).Key.ToString()] + "'");
								break;

							default:
								values.Add(reader[columnTypes.ElementAt(index).Key.ToString()].ToString());
								break;
						}

					}
				}
				int fieldCount = reader.FieldCount;

				commandText += ") Values (";
				for (var index = 0; index < values.Count; index++)
				{
					if ((index % fieldCount) == 0 && index != 0)
					{
						commandText += ") , (" + values[index].ToString() + ",";
					}

					else
					{

						if (values[index].ToString() == "")
						{
							if (index == values.Count - 1 || (index % fieldCount) == (fieldCount - 1))
							{
								commandText += "NULL";
							}
							else
							{
								commandText += "NULL ,";

							}

						}
						else
						{

							if (index == values.Count - 1 && (index % fieldCount) != 0)
							{
								commandText += values[index].ToString();
							}
							else
							{
								commandText += values[index].ToString() + ",";
							}
						}
					}
				}
				commandText += ")";
				insertresultInfo.Add("Insert for the table: " + tableName + " has " + values.Count.ToString() + " values ");
				insertresultInfo.Add("commandText: " + commandText);
				insertresultInfo.Add(tableName + " has " + fieldCount.ToString() + " column's ");
				if (needToSetIdentityInsertOn)
				{
					obj.executeQueriesInSqlServer("SET IDENTITY_INSERT " + tableName  +  " ON ;" + commandText);
				}
				else
				{
					obj.executeQueriesInSqlServer(commandText);

				}

				insertresultInfo.Add(commandText);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return insertresultInfo;
		}

	

#region rajzal_kapcsolatos_dolgok


	 //   public void changingAllProcessDesignIdInDbFileToFitSqlServer()
	 //   {
			
	//    }



		
		public List<string> allTableRelatedToDraws = new List<string> { "T_PROCESS", "T_PROCESS_DESIGN", "T_PROC_DESIGN_DRAW", "T_PROC_DESIGN_DRAW_PART",  "T_ROUTING_DESIGN", "T_ACTIVITY_DESIGN"  , "T_PROC_DESIGN_DRAW_PART_DETAIL" , "T_PROC_DESIGN_DRAW_PART_TYPE" };
		//51 T_PROC_DESIGN_DRAW
		/*
52 T_PROC_DESIGN_DRAW_PART
53 T_PROC_DESIGN_DRAW_PART_DETAIL
54 T_PROC_DESIGN_DRAW_PART_TYPE
55 T_PROCESS
56 T_PROCESS_DESIGN*/
		public int getMaxProcessDesignIdFromSQLServer(ConnectionManagerST obj)
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
		public int getMaxProcessDesignDrawPartIdFromSQLServer(ConnectionManagerST obj)
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

		public List<string> changeAllProcessDesignIdsInDBFileByNewValue( int maxProcessDesignIdInSqlServer , List<string> allTableWithProcessDesignId ,  ConnectionManagerST obj)
		{
			List<string> updateInfo = new List<string>();
			int newMaxProcessDesignId = maxProcessDesignIdInSqlServer + 1;

			foreach (string tableName in allTableWithProcessDesignId)
			{
				
					string updateText = "Update " + tableName + " Set Process_Design_ID = " + newMaxProcessDesignId.ToString() + " where 1 = 1 ";
					obj.executeQueriesInDbFile(updateText);
					updateInfo.Add(updateText);
			}

			return updateInfo;

		}
	

		public  List<string> changeProcDesignDrawPartIdsInDBFileByUpdatedList(List<int> oldProcDesignDrawPartIdsList, List<int> newProcDesignDrawPartIdsList, List<string> allTableWithProcDesignDrawPartIds , ConnectionManagerST obj)
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

		public static List<string> changetempProcDesignDrawPartIdsInDBFileToRealNewtempProcDesignDrawPartIds(List<string> allTableWithProcDesignDrawPartIds , ConnectionManagerST obj)
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

		public int getMaxProcessDesignDrawIdFromSQLServer(ConnectionManagerST obj)
		{
			int maxProcessDesignDrawId = 0;

			try
			{
				var reader = obj.sqlServerDataReader("select max(Proc_Design_Draw_ID) as Max_Proc_Design_Draw_ID from T_PROC_DESIGN_DRAW");
				while (reader.Read())
				{
					maxProcessDesignDrawId = Convert.ToInt32(reader["Max_Proc_Design_Draw_ID"]);
				}

			}
			catch (Exception ex)
			{
				throw ex;

			}

			return maxProcessDesignDrawId;

		}

		public List<string> changeAllProcessDesignDrawIdsInDBFileByNewValue(int maxProcessDesignDrawId,  ConnectionManagerST obj)
		{
			List<string> updateInfo = new List<string>();
			List<string> allTableWithProcessDesignDrawId = new List<string>() { "T_PROC_DESIGN_DRAW" , "T_PROC_DESIGN_DRAW_PART" } ;

			int newMaxProcessDesignDrawId = maxProcessDesignDrawId + 1;

			foreach (string tableName in allTableWithProcessDesignDrawId)
			{

				string updateText = "Update " + tableName + " Set Proc_Design_Draw_ID = " + newMaxProcessDesignDrawId.ToString() + " where 1 = 1 ";
				obj.executeQueriesInDbFile(updateText);
				updateInfo.Add(updateText);
			}

			return updateInfo;

		}


		#endregion
		#region table_infos_+_smaller_help_functions

		public List<string> convertIntListToStringList(List<int> inputStringList)
		{
			List<string> convertedStringList = inputStringList.ConvertAll<string>(delegate (int i) { return i.ToString(); });
			return convertedStringList;
		}
		public List<string> tableInfoListFromDBFile(ConnectionManagerST obj)
		{
			Tables_cwp table_info = new Tables_cwp();
			string[] tableNames = table_info.getCWPTableList();
			string commandText = "select table_name,column_name,data_type from table_information WHERE table_name in (";
			List<string> tableInformationInDBFile = new List<string>();
			for (int i = 0; i < tableNames.Length - 1; i++)
			{
				commandText += "'" + tableNames[i] + "',";
			}
			commandText += "'" + tableNames[tableNames.Length - 1] + "' )";

			var sqReader = obj.sqLiteDataReader(commandText);
			try
			{
				while (sqReader.Read())
				{
					string info = sqReader["table_name"].ToString() + " || " + sqReader["column_name"].ToString() + " || " + sqReader["data_type"].ToString();
					tableInformationInDBFile.Add(info);
				}

			}
			catch (Exception ex)
			{
				throw ex;
			}

			return tableInformationInDBFile;

		}

   

		public bool verifyThatDBStructuresAreTheSame(List<string> inputList)
		{
			bool theDbStructuresAreTheSame = false;
			if (inputList.Contains("No Difference Spotted"))
			{
				theDbStructuresAreTheSame = true;
			}
			return theDbStructuresAreTheSame;

		}
		public List<string> compareTwoStringList(List<string> dbFile, List<string> sqlServer)
		{
			List<string> resultInfo = new List<string>();
			int firstListLength = dbFile.Count;
			int secondListLength = sqlServer.Count;
			resultInfo.Add("Comparing The DB File And The Sql Server Result : ");

			try
			{
				if (firstListLength == secondListLength)
				{
					resultInfo.Add("No Difference Spotted");
				}
				else if (firstListLength > secondListLength)
				{
					resultInfo.Add("The DB File Has More Column's");
					resultInfo.Add("Table Name || Column Name || Data Type");
					resultInfo.AddRange(dbFile.Except(sqlServer));
				}
				else
				{
					resultInfo.Add("The SQL Server Has More Column's");
					resultInfo.Add("Table Name || Column Name || Data Type");
					resultInfo.AddRange(sqlServer.Except(dbFile));
				}

			}
			catch (Exception ex)
			{
				throw ex;
				return resultInfo;
			}

			return resultInfo;
		}


		private Dictionary<string, string> getColumnTypesDictionary_v2(string CWPTableName, ConnectionManagerST obj)
		{
			Dictionary<string, string> fields = new Dictionary<string, string>();
			string commandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + CWPTableName + "'";
			var reader = obj.sqlServerDataReader(commandText);
			while (reader.Read())
			{
				fields.Add(reader["COLUMN_NAME"].ToString(), reader["DATA_TYPE"].ToString());
			}
			return fields;

		}

		public List<string> checkingDBStructureDifferences(ConnectionManagerST obj)
		{
			List<string> comparingDBStructuresInfo = new List<string>();
			List<string> tableInfoInDBFile = new List<string>();
			List<string> tableInfoInSQLServer = new List<string>();
			try
			{
				tableInfoInDBFile.AddRange(tableInfoListFromDBFile(obj));
				tableInfoInSQLServer.AddRange(tableInfoListFromSQLServer(obj));
				comparingDBStructuresInfo.AddRange(compareTwoStringList(tableInfoInDBFile, tableInfoInSQLServer));
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return comparingDBStructuresInfo;
		}
		public List<string> tableInfoListFromSQLServer(ConnectionManagerST obj)
		{
			Tables_cwp table_info = new Tables_cwp();
			string[] tableNames = table_info.getCWPTableList();
			string commandTxt = "Select table_name,column_name,data_type from INFORMATION_SCHEMA.COLUMNS where table_name in (";
			List<string> tableInfoInSQLServer = new List<string>();
			for (int i = 0; i < tableNames.Length - 1; i++)
			{
				commandTxt += "'" + tableNames[i] + "',";

			}
			commandTxt += "'" + tableNames[tableNames.Length - 1] + "' )";
			var reader = obj.sqlServerDataReader    (commandTxt);
			try
			{
				while (reader.Read())
				{
					string info = reader["table_name"].ToString() + " || " + reader["column_name"].ToString() + " || " + reader["data_type"].ToString();
					tableInfoInSQLServer.Add(info);
				}

			}
			catch (Exception ex)
			{
				throw ex;
			}
			return tableInfoInSQLServer;
		}
#endregion
#region komment_kódok

		/*
		public List<string> insertValuesFromDbFileToSqlServer(string tableName, bool needToSetIdentityInsertOn)
		{
			List<string> insertresultInfo = new List<string>();
		
			List<string> columnNamesInDbFile = new List<string>();
			List<string> values = new List<string>();
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			ConnectionManager connectionManager = new ConnectionManager();
			string commandText = "INSERT INTO " + tableName + "  ( ";
			try
			{
				connectionManager.openSqlServerConnection();
				connectionManager.openSqLiteConnection();
				columnTypes = getColumnTypesDictionary(tableName);

				var reader = connectionManager.sqLiteDataReader("SELECT * FROM " + tableName);

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


				while (reader.Read())
				{
					for (var index = 0; index < columnTypes.Count; index++)
					{
						switch (columnTypes.ElementAt(index).Value)
						{
							case "NULL":
								values.Add("0");
								break;
							case "binary":
							case "varbinary":
							case "image":
							case "DateTime":
							case "nvarchar":
								values.Add("'" + reader[columnTypes.ElementAt(index).Key.ToString()] + "'");
								break;

							default:
								values.Add(reader[columnTypes.ElementAt(index).Key.ToString()].ToString());
								break;
						}

					}
				}
				int fieldCount = reader.FieldCount;

				commandText += ") Values (";
				for (var index = 0; index < values.Count; index++)
				{
					if ((index % fieldCount) == 0 && index != 0)
					{
						commandText += ") , (" + values[index].ToString() + ",";
					}

					else
					{

						if (values[index].ToString() == "")
						{
							if (index == values.Count - 1 || (index % fieldCount) == (fieldCount - 1))
							{
								commandText += "NULL";
							}
							else
							{
								commandText += "NULL ,";

							}

						}
						else
						{

							if (index == values.Count - 1 && (index % fieldCount) != 0)
							{
								commandText += values[index].ToString();
							}
							else
							{
								commandText += values[index].ToString() + ",";
							}
						}
					}
				}
				commandText += ")";
				insertresultInfo.Add("Insert for the table: " + tableName + " has " + values.Count.ToString() +" values ");
				insertresultInfo.Add("commandText: " + commandText);
				insertresultInfo.Add(tableName + " has " + fieldCount.ToString() + " column's ");
				if (needToSetIdentityInsertOn)
				{
					connectionManager.executeQueriesInSqlServer("SET IDENTITY_INSERT T_DEPARTMENT ON ;" + commandText);
				}
				else
				{
					connectionManager.executeQueriesInSqlServer(commandText);

				}

				insertresultInfo.Add(commandText);
			}
			catch (Exception ex)
			{
				insertresultInfo.Add(ex.Message.ToString() + ex.StackTrace.ToString());
				return insertresultInfo;
			}
			return insertresultInfo;
		}
		
		public List<string> tableInfoListFromDBFile()
		{
			Tables_cwp table_info = new Tables_cwp();
			string[] tableNames = table_info.getCWPTableList();
			string commandText = "select table_name,column_name,data_type from table_information WHERE table_name in (";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			List<string> tableInformationInDBFile = new List<string>();
			for (int i = 0; i < tableNames.Length - 1; i++)
			{
				commandText += "'" + tableNames[i] + "',";
			}
			commandText += "'" + tableNames[tableNames.Length - 1] + "' )";
			command.CommandText = commandText;
			connSqlite.Open();
			SQLiteDataReader sqReader = command.ExecuteReader();
			try
			{
				while (sqReader.Read())
				{
					string info = sqReader["table_name"].ToString() + " || " + sqReader["column_name"].ToString() + " || " + sqReader["data_type"].ToString();
					tableInformationInDBFile.Add(info);
				}

			}
			catch (Exception ex)
			{
				tableInformationInDBFile.Add(ex.Message.ToString() + ex.StackTrace.ToString());
				return tableInformationInDBFile;
			}
			connSqlite.Close();

			return tableInformationInDBFile;

		}

		public List<string> tableInfoListFromSQLServer()
		{
			Tables_cwp table_info = new Tables_cwp();
			string[] tableNames = table_info.getCWPTableList();
			string commandTxt = "Select table_name,column_name,data_type from INFORMATION_SCHEMA.COLUMNS where table_name in (";
			string connectionString = ConfigurationManager.AppSettings.Get("connstrRe");
			SqlConnection connection = new SqlConnection(connectionString);
			SqlCommand command = new SqlCommand(commandTxt, connection);
			List<string> tableInfoInSQLServer = new List<string>();
			SqlDataReader reader;
			for (int i = 0; i < tableNames.Length - 1; i++)
			{
				commandTxt += "'" + tableNames[i] + "',";

			}
			// commandTxt += "' )";
			commandTxt += "'" + tableNames[tableNames.Length - 1] + "' )";
			command.CommandText = commandTxt;
			connection.Open();
			reader = command.ExecuteReader();
			try
			{
				while (reader.Read())
				{
					string info =  reader["table_name"].ToString() + " || " + reader["column_name"].ToString() + " || " + reader["data_type"].ToString();
					tableInfoInSQLServer.Add(info);
				}

			}
			catch (Exception ex)
			{
				tableInfoInSQLServer.Add(ex.Message.ToString() + ex.StackTrace.ToString());
				return tableInfoInSQLServer;
			}
			return tableInfoInSQLServer;
		}
		
		public string insertIntoTargetDb(string tableName, string[] values, bool requireIdentityInsert)
		{
			string commandTxt = "";
			if (requireIdentityInsert)
			{
				commandTxt = " SET IDENTITY_INSERT " + tableName + " ON;";
			}
			string connectionString = ConfigurationManager.AppSettings.Get("connstrRe");
			commandTxt += "INSERT INTO " + tableName + " (";
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			SqlConnection connection = new SqlConnection(connectionString);
			SqlCommand command = new SqlCommand(commandTxt, connection);
			SqlDataReader reader;

			try
			{
				columnTypes = getColumnTypesDictionary(tableName);
				for (int i = 0; i < columnTypes.Count; i++)
				{
					switch (columnTypes.ElementAt(i).Value)
					{
						case "binary":
						case "varbinary":
							break;
						case "image":
							break;
						default:
							{
								if (i == columnTypes.Count - 1)
								{
									commandTxt += string.Join(",", columnTypes.ElementAt(i).Key.ToString());
								}
								else
								{
									commandTxt += string.Join(",", columnTypes.ElementAt(i).Key.ToString()) + ", ";
								}
								break;
							}
					}
				}
				commandTxt += " )  Values ( ";
				for (int i = 0; i < values.Length; i++)
				{
					if (i == values.Length - 1)
					{
						commandTxt += string.Join(", ", values[i]);

					}
					else
					{
						commandTxt += string.Join(", ", values[i]) + ", ";
					}

				}
				commandTxt += " )";
				command.CommandText = commandTxt;
				connection.Open();
				reader = command.ExecuteReader();
			}
			catch (Exception ex)
			{
				return commandTxt;
			}

			return commandTxt.Trim();


		}

		public Dictionary<string, string> writeDbFileContentToTargetDb(string tableName)
		{
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			columnTypes = getColumnTypesDictionary(tableName);
			string commandText = "SELECT * from @paramater";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			command.Parameters.AddWithValue("@parameter", tableName);
			return columnTypes;


		}
		public string getNumberOfRowsWhereFKIsProcessIdInDBFile(string tableName, int processId)
		{
			int rowCount = 2;
			string commandText = "SELECT count(*)  FROM  ";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			command.CommandText = commandText + tableName + " where Process_Id  = " + processId;
			connSqlite.Open();
			SQLiteDataReader sqReader = command.ExecuteReader();
			try
			{
				if (sqReader.Read())
				{
					rowCount = Convert.ToInt32(sqReader[0]);
				}

				//  return rowCount;

			}
			catch (Exception ex)
			{
				return ex.Message.ToString() + ex.StackTrace.ToString();
			}
			return rowCount.ToString();
			// return 0;
			//    return rowCount;
		}

		public string getNumberOfRowsInDBFile(string tableName)
		{
			int rowCount = 0;
			string commandText = "SELECT count(*)  FROM  ";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			command.CommandText = commandText + tableName;
			connSqlite.Open();
			SQLiteDataReader sqReader = command.ExecuteReader();
			try
			{
				if (sqReader.Read())
				{
					rowCount = Convert.ToInt32(sqReader[0]);
				}

				//  return rowCount;

			}
			catch (Exception ex)
			{
				return ex.Message.ToString() + ex.StackTrace.ToString();
			}
			return rowCount.ToString();
			// return 0;
			//    return rowCount;
		}
		public string getNumberOfRowsFromSQLServer(string tableName)
		{



			int rowCount = 0;
			string commandText = "SELECT count(*)  FROM  ";
			string connectionString = ConfigurationManager.AppSettings.Get("connstrRe");
			SqlDataReader reader;
			Tables_cwp tables = new Tables_cwp();
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand(commandText, connection);
				try
				{
					connection.Open();
					reader = command.ExecuteReader();
					if (reader.Read())
					{
						rowCount = Convert.ToInt32(reader[0]);
					}

					//  return rowCount;

				}
				catch (Exception ex)
				{
					return ex.Message.ToString() + ex.StackTrace.ToString();
				}
				return rowCount.ToString();
				// return 0;
				//    return rowCount;
			}
		}
		

		public List<string> getValuesFromTable(Tables_cwp table)
		{
			List<string> valuesInTable = new List<string>();

			string commandText = "SELECT *  FROM  ";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			command.CommandText = commandText + table.TableName;
			connSqlite.Open();
			SQLiteDataReader sqReader = command.ExecuteReader();
			try
			{
				while (sqReader.Read())
				{
					foreach (string colum in table.ColumnName)
					{
						List<string> TEMPvaluesInTable = new List<string>();

						if (sqReader[colum] == "")
						{
							TEMPvaluesInTable.Add("null");
							//  index++;
						}
						else
						{
							TEMPvaluesInTable.Add(sqReader[colum].ToString());
						}
						valuesInTable.AddRange(TEMPvaluesInTable);

					}
					//   if ((totalNumberOfRecord - 1) == index)
					//   {
					//     break;
					//   }
				}
			}
			catch (Exception ex)
			{
				valuesInTable.Add(ex.Message.ToString() + ex.StackTrace.ToString());
			}
			return valuesInTable;
		}

		public List<string> getValuesFromTableInSQLServer(Tables_cwp table)
		{
			List<string> valuesInTable = new List<string>();

			string commandText = "SELECT *  FROM  ";
			string connectionString = ConfigurationManager.AppSettings.Get("connstrRe");
			SqlDataReader reader;
			SqlConnection connection = new SqlConnection(connectionString);
			SqlCommand command = new SqlCommand(commandText, connection);

			command.CommandText = commandText + table.TableName;

			try
			{

				connection.Open();
				reader = command.ExecuteReader();
				while (reader.Read())
				{
					foreach (string colum in table.ColumnName)
					{
						List<string> TEMPvaluesInTable = new List<string>();

						if (reader[colum] == "")
						{
							TEMPvaluesInTable.Add("null");
							//  index++;
						}
						else
						{
							TEMPvaluesInTable.Add(reader[colum].ToString());
						}
						valuesInTable.AddRange(TEMPvaluesInTable);

					}
					//   if ((totalNumberOfRecord - 1) == index)
					//   {
					//     break;
					//   }
				}
			}
			catch (Exception ex)
			{
				valuesInTable.Add(ex.Message.ToString() + ex.StackTrace.ToString());
			}
			return valuesInTable;
		}

		public List<string> getValuesFromTablesWhichHasProcessIdInSQLServer(Tables_cwp table, int process_id)
		{
			List<string> valuesInTable = new List<string>();

			string commandText = "SELECT *  FROM  ";
			string connectionString = ConfigurationManager.AppSettings.Get("connstrRe");
			SqlDataReader reader;
			SqlConnection connection = new SqlConnection(connectionString);
			SqlCommand command = new SqlCommand(commandText, connection);
			command.CommandText = commandText + table.TableName + " where process_id = " + process_id;

			try
			{

				connection.Open();
				reader = command.ExecuteReader();
				while (reader.Read())
				{
					foreach (string colum in table.ColumnName)
					{
						List<string> TEMPvaluesInTable = new List<string>();

						if (reader[colum] == "")
						{
							TEMPvaluesInTable.Add("null");
							//  index++;
						}
						else
						{
							TEMPvaluesInTable.Add(reader[colum].ToString());
						}
						valuesInTable.AddRange(TEMPvaluesInTable);

					}
					//   if ((totalNumberOfRecord - 1) == index)
					//   {
					//     break;
					//   }
				}
			}
			catch (Exception ex)
			{
				valuesInTable.Add(ex.Message.ToString() + ex.StackTrace.ToString());
			}
			return valuesInTable;
		}

		public Dictionary<string, string> getValuesFromTableInDbFile(string tableName)
		{

			string commandText = "SELECT *  FROM  ";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			command.CommandText = commandText + tableName;
			Dictionary<string, string> valueDictionary = new Dictionary<string, string>();
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			columnTypes = getColumnTypesDictionary(tableName);
			string alma = "alma";
			connSqlite.Open();
			SQLiteDataReader sqReader = command.ExecuteReader();
			while (sqReader.Read())
			{
				for (int i = 0; i < columnTypes.Count; i++)
				{
					string columnName = columnTypes.ElementAt(i).Key.ToString();
					if (columnTypes.ElementAt(i).Key.ToString() == "Process_ID")
					{
						valueDictionary.Add(columnName, "1224");
					}
					else if (sqReader[columnName] == "")
					{
						valueDictionary.Add(columnName, "null");
					}

					else if (columnTypes.ElementAt(i).Value == "varchar" || columnTypes.ElementAt(i).Value == "nvarchar" || columnTypes.ElementAt(i).Value == "bit")
					{
						valueDictionary.Add(columnName, "'" + sqReader[columnName].ToString() + "'");
					}

					else
					{
						valueDictionary.Add(columnName, sqReader[columnName].ToString());
					}

				}
			}
			sqReader.Close();
			return valueDictionary;
		}


		public Dictionary<string, string> compareTwoObjectsAndGetDifferences(List<Tables_cwp> A, List<Tables_cwp> B)
		{
			int firstObjectLength = A.Count;
			int secondObjectLength = B.Count;
			Dictionary<string, string> Error = new Dictionary<string, string>();
			Dictionary<string, string> Difference = new Dictionary<string, string>();
			List<Tables_cwp> SortedListA = A.OrderBy(o => o.TableName).ToList();
			List<Tables_cwp> SortedListB = B.OrderBy(o => o.TableName).ToList();
			if (firstObjectLength != secondObjectLength)
			{
				Error.Add("Error", "Given Parameters Has Different size.");
				return Error;

			}
			else
			{
				for (var i = 0; i < firstObjectLength; i++)
				{
					if (SortedListA[i].ColumnName.Count != SortedListB[i].ColumnName.Count)
					{
						Difference.Add("Difference Spotted ", SortedListA[i].TableName.ToString());
					}

				}
				if (Difference.Count == 0)
				{
					Difference.Add("No Difference Spotted", "No Difference Spotted ");
				}
				return Difference;

			}
		}
		public List<Tables_cwp>  checkTableInfoInDbFile()
		{
	
			Dictionary<string, string> tableInfoInImportDb = new Dictionary<string, string>();
			Dictionary<string, string> exception = new Dictionary<string, string>();
			string columnName ;
			string dataType ;
		
			string commandText = "SELECT COLUMN_NAME,DATA_TYPE,TABLE_NAME FROM table_information WHERE TABLE_NAME IN ( {0} )";
			string connectionString = ConfigurationManager.ConnectionStrings["Default"].ToString();
			SQLiteConnection connSqlite = new SQLiteConnection(connectionString);
			SQLiteCommand command = new SQLiteCommand(connSqlite);
			Tables_cwp tables = new Tables_cwp();
			Tables_cwp dbInformationForEachTables = new Tables_cwp();

		
			int tableListLength = tableList.Length;

			var tableParameterList = new List<string>();
			
			List<Tables_cwp> tableObjectList = new List<Tables_cwp>();
			var index = 0;
			foreach (string table in tableList)
			{
				var paramName = "@parameter" + index;
				command.Parameters.AddWithValue(paramName, table);
				tableParameterList.Add(paramName);
				index++;

				Tables_cwp tableObject = new Tables_cwp();

				tableObject.TableName = table;
				tableObjectList.Add(tableObject);

			}
			try
			{
				
				command.CommandText =  String.Format(commandText, string.Join(",", tableParameterList)); ;
				connSqlite.Open();
				SQLiteDataReader sqReader = command.ExecuteReader();

				while (sqReader.Read())
				{
					for (var i = 0; i < tableObjectList.Count; i++)
					{
						if (tableObjectList[i].TableName.Equals(sqReader["TABLE_NAME"].ToString()))
						{
							tableObjectList[i].ColumDataType.Add(sqReader["DATA_TYPE"].ToString());
							tableObjectList[i].ColumnName.Add(sqReader["COLUMN_NAME"].ToString());
							break;
						}
					}
					
				}
				sqReader.Close();
			}
			catch (Exception ex)
			{
				exception.Add(ex.Message.ToString(), ex.StackTrace.ToString());
				for (var i = 0; i < tableObjectList.Count; i++)
				{
					tableObjectList[i].Exception = ex.Message.ToString() + ex.StackTrace.ToString();
				}
					//dbInformationForEachTables.Exception = ex.Message.ToString() +  ex.StackTrace.ToString();


				return tableObjectList;


			}

			

			return tableObjectList;

		}*/
		#endregion
	 
		public bool CheckIfProcessExistInDatabase(Int64 process_Id)
		{
			string connstrRe = ConfigurationManager.AppSettings.Get("connstrRe");
			string strSQL = "SELECT  count(*) from T_PROCESS where Process_ID = @processId";
			bool processIdFound = false;
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			using (SqlConnection connection = new SqlConnection(connstrRe))
			{
				SqlCommand command = new SqlCommand(strSQL, connection);
				command.Parameters.AddWithValue("@processId", process_Id);
				try
				{
					connection.Open();
					SqlDataReader reader;
					reader = command.ExecuteReader();
					reader.Read();
					if ((int)reader[0] == 1)
					{
						processIdFound = true;
					}
					reader.Close();

				}
				catch (Exception ex)
				{
					res = FillServiceCallResult(ex);
				}
			}
			return processIdFound;

		}



		public int detectMaxProcessId()
		{
			string connstrRe = ConfigurationManager.AppSettings.Get("connstrRe");
			string strSQL = "SELECT MAX(Process_ID) from T_PROCESS;";
			int maxProcessId;
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			using (SqlConnection connection = new SqlConnection(connstrRe))
			{
				SqlCommand command = new SqlCommand(strSQL, connection);
				try
				{
					connection.Open();
					SqlDataReader reader;
					reader = command.ExecuteReader();
					reader.Read();
					maxProcessId = (int)reader[0];
					reader.Close();
					return maxProcessId;

				}
				catch (Exception ex)
				{


				}
			}
			return 0;

		}


		private ServiceCallResult getSqlitePath(Int64 process_Id)
		{
			string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + "\\";
			string connStr = ConfigurationManager.AppSettings.Get("connstr");
			string strSQL = "SELECT Name FROM T_PROCESS WHERE Process_Id = @processId";
			string processName = "";
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			using (SqlConnection connection = new SqlConnection(connStr))
			{
				SqlCommand command = new SqlCommand(strSQL, connection);
				command.Parameters.AddWithValue("@processId", process_Id);
				try
				{
					connection.Open();
					SqlDataReader reader;
					reader = command.ExecuteReader();
					reader.Read();
					processName = reader["Name"].ToString();
					reader.Close();
				}
				catch (Exception ex)
				{
					res = FillServiceCallResult(ex);
				}
				//res.Description = 
				processName = processName.Replace(" ", "_");
				fileName = fileName + processName + ".db";
				res.Description = fileName;
			}
			return res;
		}

		public ServiceCallResult FillServiceCallResult(Exception ex)
		{
			ServiceCallResult ret = new ServiceCallResult();
			ret.Code = -1;
			ret.Source = ex.Source;

			ret.ExceptionContent = ex.ToString();
			if (ex.InnerException != null)
			{
				ret.InnerExceptionContent = ex.InnerException.ToString();
			}
			return ret;
		}


		private ServiceCallResult TransferReport(int processId, bool recurs = false)
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			string strSqLiteSQL = "";
			SQLiteCommand cmdSqlite;
			string strSQLiteValues = "";
			string strMsSQL = "";
			SqlCommand cmdMsSql;
			SqlDataReader readerMsSql;
			string strMsSQLData;
			string strMsSQLDataChild;
			string strMsSQLDataGrandChild;
			SqlCommand cmdMsSqlData;
			SqlCommand cmdMsSqlDataChild;
			SqlCommand cmdMsSqlDataGrandChild;
			SqlDataReader readerMsSqlData;
			SqlDataReader readerMsSqlDataChild;
			SqlDataReader readerMsSqlDataGrandChild;
			string currType = "";
			ServiceCallResult resGen;

			Dictionary<string, string> columnTypes;
			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			string sqliteDbPath;
			res = new ServiceCallResult { Code = 0, Description = "OK" };
			res = getSqlitePath(processId);
			if (res.Code != 0)
			{
				return res;
			}
			sqliteDbPath = res.Description;
			string connStr = String.Format("Data Source={0} ;Version=3;", res.Description);
			SQLiteConnection connSqlite = new SQLiteConnection(connStr);

			try
			{
				using (SqlConnection MSSQLConnection = new SqlConnection(connStrSQLServer))
				{
					MSSQLConnection.Open();
#region T_REPORT_FIELD

					columnTypes = getColumnTypesDictionary("T_REPORT_FIELD");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_FIELD";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
						{

							if (!IsReportInList(Convert.ToInt64(readerMsSql["REPORT_ID"].ToString())))
							{
								reports.Add(new ReportListItem { ReportId = Convert.ToInt64(readerMsSql["REPORT_ID"].ToString()), Processed = false });
							}
							reportFields.Add(Convert.ToInt64(readerMsSql["REPORT_FIELD_ID"].ToString()));
							if (readerMsSql["UDT_FIELD_ID"].ToString() != "")
							{
								udtReportFields.Add(Convert.ToInt64(readerMsSql["UDT_FIELD_ID"].ToString()));
							}
							strSqLiteSQL = "INSERT INTO T_REPORT_FIELD " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_REPORT
					columnTypes = getColumnTypesDictionary("T_REPORT");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (IsReportInList(Convert.ToInt64(readerMsSql["Report_id"].ToString())))
						{
							strSqLiteSQL = "INSERT INTO T_REPORT " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_REPORT_2_FIELD_COND_GROUP
					columnTypes = getColumnTypesDictionary("T_REPORT_2_FIELD_COND_GROUP");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_2_FIELD_COND_GROUP";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (IsReportInList(Convert.ToInt64(readerMsSql["Report_id"].ToString())))
						{
							strSqLiteSQL = "INSERT INTO T_REPORT_2_FIELD_COND_GROUP " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE
					columnTypes = getColumnTypesDictionary("T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE");
					// transfer  data
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_FIELD  WHERE PROCESS_ID=" + processId.ToString();
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						strMsSQLDataChild = "SELECT * FROM T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE WHERE REPORT_FIELD_ID=" + readerMsSql["REPORT_FIELD_ID"].ToString();

						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE " + " ( ";
						currType = "";
						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							t_report_calculated_field_formula_tree_nodes.Add(Convert.ToInt64(readerMsSqlDataChild["T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_ID"].ToString()));
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUES
					columnTypes = getColumnTypesDictionary("T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (t_report_calculated_field_formula_tree_nodes.FindIndex(a => a == Convert.ToInt64(readerMsSql["Report_Calculated_Field_Formula_Tree_Node_ID"].ToString())) > 0)
						{
							strSqLiteSQL = "INSERT INTO T_REPORT " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_REPORT_EDIT_OWNER
					columnTypes = getColumnTypesDictionary("T_REPORT_EDIT_OWNER");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_EDIT_OWNER";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if ((IsReportInList(Convert.ToInt64(readerMsSql["Report_id"].ToString()))))
						{
							strSqLiteSQL = "INSERT INTO T_REPORT_EDIT_OWNER " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_REPORT_FIELD_UDT_COLUMNS
					columnTypes = getColumnTypesDictionary("T_REPORT_FIELD_UDT_COLUMNS");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_FIELD_UDT_COLUMNS";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (IsReportInList(Convert.ToInt64(readerMsSql["Report_id"].ToString()))
						  && udts.FindIndex(a => a == Convert.ToInt64(readerMsSql["User_Defined_Table_ID"].ToString())) > 0)
						{
							strSqLiteSQL = "INSERT INTO T_REPORT_FIELD_UDT_COLUMNS " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_REPORT_FILTER
					columnTypes = getColumnTypesDictionary("T_REPORT_FILTER");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_FILTER";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if ((IsReportInList(Convert.ToInt64(readerMsSql["Report_id"].ToString()))))
						{
							strSqLiteSQL = "INSERT INTO T_REPORT_FILTER " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_REPORT_REFERENCED_FIELD_LOCATION
					columnTypes = getColumnTypesDictionary("T_REPORT_REFERENCED_FIELD_LOCATION");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (reportFields.FindIndex(a => a == Convert.ToInt64(readerMsSql["Report_Field_ID"].ToString())) > 0)
						{
							strSqLiteSQL = "INSERT INTO T_REPORT_REFERENCED_FIELD_LOCATION " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
						else
						{
							// we need insert the T_FIELD value pointed by Referenced_field_id 

							if (Convert.ToInt64(readerMsSql["Referenced_Field_ID"].ToString()) > 0)
							{
								if (reportFields.FindIndex(a => a == Convert.ToInt64(readerMsSql["Referenced_Field_ID"].ToString())) < 0)
								{
									reportFields.Add(Convert.ToInt64(readerMsSql["Referenced_Field_ID"].ToString()));
									// insert into t_field
									strMsSQLData = "SELECT * FROM T_FIELD WHERE FIELD_ID=" + readerMsSql["Referenced_Field_ID"].ToString();
									columnTypes = getColumnTypesDictionary("T_FIELD");
									cmdMsSqlData = new SqlCommand(strMsSQLData, MSSQLConnection);
									readerMsSqlData = cmdMsSqlData.ExecuteReader();
									strSqLiteSQL = "INSERT INTO T_FIELD  ( ";
									currType = "";
									foreach (KeyValuePair<string, string> entry in columnTypes)
									{
										switch (entry.Value)
										{
											case "binary":
											case "varbinary":
											case "image":
												break;
											default:
												{
													strSqLiteSQL += entry.Key + ",";
													break;
												}
										}
									}
									strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
									while (readerMsSqlData.Read())
									{
										strSQLiteValues = "";
										for (int j = 0; j < readerMsSqlData.FieldCount; j++)
										{
											columnTypes.TryGetValue(readerMsSqlData.GetName(j), out currType);
											switch (currType)
											{
												case "binary":
												case "varbinary":
												case "image":
													break;
												default:
													{

														strSQLiteValues += "'" + readerMsSqlData[j].ToString().Replace("'", "''") + "',";
														break;
													}
											}
										}
										strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
										cmdSqlite = new SQLiteCommand(connSqlite);

										cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;

										cmdSqlite.ExecuteNonQuery();
									}
								}
							}

						}
					}
#endregion
				}
			}
			catch (Exception ex)
			{
				res = FillServiceCallResult(ex);
			}



			return res;
		}

		private ServiceCallResult TransferProcess(Int64 processId, bool recurs = false)
		{
			ServiceCallResult res = new ServiceCallResult();
			res = getSqlitePath(processId);

			Int64 processDesignId = getProcessDesignIdFromProcess(processId);
			Int64 procDesignDrawId = getProcessDesignDrawId(processDesignId);

			fieldsForProcess = getProcessFields(processId);
			tables.Add(new TableNameAndCondition { TableName = "T_PROCESS", Condition = " WHERE PROCESS_ID = " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_PROCESS_DESIGN", Condition = " WHERE PROCESS_DESIGN_ID = " + processDesignId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_PROC_DESIGN_DRAW", Condition = " WHERE PROCESS_DESIGN_ID = " + processDesignId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_PROC_DESIGN_DRAW_PART", Condition = " WHERE PROC_DESIGN_DRAW_ID = " + procDesignDrawId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_PROC_DESIGN_DRAW_PART_TYPE", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_ROUTING", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_CONDITION_GROUP", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_DATE_TYPE", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE", Condition = " WHERE 1=1  " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE", Condition = " WHERE 1=1  " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_TEXT_FORMAT_TYPE", Condition = " WHERE 1=1  " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_TO_FIELD_DEPENDENCY_TYPE", Condition = " WHERE 1=1  " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_TYPE", Condition = " WHERE 1=1  " });
			tables.Add(new TableNameAndCondition { TableName = "T_FILE_FIELD_TYPE", Condition = " WHERE 1=1  " });
			tables.Add(new TableNameAndCondition { TableName = "T_ACTIVITY", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_FIELDS_UI_PARAMETERS", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_NOTIFICATION", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_PERSON", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_DEPARTMENT", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_DEPARTMENT_MEMBERS", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_CALCULATED_FIELD_RESULT_TYPE_ID", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_CATEGORY", Condition = " WHERE 1=1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_PROCESS_OWNER", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_PROCESS_READER", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_ROLE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_ROLE_MEMBERS", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_REPORT_GROUP", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_REPORT_GROUP_ADMINISTRATOR", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_REPORT_OWNERS", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_SUBPROCESS", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_UI_COMPONENT", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_TO_FIELD_DEPENDENCY", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_FIELD_VALUE_TRANSLATION", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_CHART_TYPE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_CHART_FIELD_TYPE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_LANGUAGE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_REPORT_TYPE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_FINISH_STEP_MODE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_PARTICIPANT_TYPE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_CALCULATED_FIELD_CONSTANT_TYPE", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_COMPARE_OPERATION", Condition = " WHERE 1 = 1 " });
			tables.Add(new TableNameAndCondition { TableName = "T_DB_CONNECTION", Condition = " WHERE 1 = 1 " });



			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");

			// tables that can be transfer in simple way
			try
			{
				//connSqlite = new SQLiteConnection(connStrSQLite);
				//connSqlite.Open();
				using (SqlConnection MSSQLConnection = new SqlConnection(connStrSQLServer))
				{
					string strSqLiteSQL = "";
					SQLiteCommand cmdSqlite;
					string strSQLiteValues = "";
					string strMsSQL = "";
					SqlCommand cmdMsSql;
					SqlDataReader readerMsSql;
					string strMsSQLData;
					string strMsSQLDataChild;
					string strMsSQLDataGrandChild;
					SqlCommand cmdMsSqlData;
					SqlCommand cmdMsSqlDataChild;
					SqlCommand cmdMsSqlDataGrandChild;
					SqlDataReader readerMsSqlData;
					SqlDataReader readerMsSqlDataChild;
					SqlDataReader readerMsSqlDataGrandChild;

					string currType = "";
					ServiceCallResult resGen;
					Dictionary<string, string> columnTypes;
					MSSQLConnection.Open();
					for (int i = 0; i < tables.Count; i++)
					{
						columnTypes = getColumnTypesDictionary(tables[i].TableName);
						strMsSQLData = "SELECT * FROM " + tables[i].TableName + tables[i].Condition;
						cmdMsSqlData = new SqlCommand(strMsSQLData, MSSQLConnection);
						readerMsSqlData = cmdMsSqlData.ExecuteReader();
						strSqLiteSQL = "INSERT INTO " + tables[i].TableName + " ( ";
						currType = "";
						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlData.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlData.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlData.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{

											strSQLiteValues += "'" + readerMsSqlData[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);

							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;

							cmdSqlite.ExecuteNonQuery();
						}
					}

#region other tables
					// fill activities array
					strMsSQL = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					readerMsSql = cmdMsSql.ExecuteReader();
					activities = new List<long>();
					while (readerMsSql.Read())
					{
						activities.Add(Convert.ToInt64(readerMsSql["activity_id"].ToString()));
					}
#region T_ACTIVITY_OWNER_BY_CONDITION
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_OWNER_BY_CONDITION");
					for (int i = 0; i < activities.Count; i++)
					{
						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION WHERE ACTIVITY_ID=" + activities[i].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						cmdMsSqlDataChild.CommandText = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION WHERE ACTIVITY_ID=" + activities[i].ToString();
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();

						strSqLiteSQL = "INSERT INTO T_ACTIVITY_OWNER_BY_CONDITION " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							activityOwnerByCondition.Add(Convert.ToInt64(readerMsSqlDataChild["Activity_Owner_By_Condition_ID"].ToString()));
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}
#endregion
#region T_ACTIVITY_OWNER_BY_COND_PARTICIPANT
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_OWNER_BY_COND_PARTICIPANT");
					for (var i = 0; i < activityOwnerByCondition.Count; i++)
					{
						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_COND_PARTICIPANT WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition[i].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						cmdMsSqlDataChild.CommandText = "SELECT * FROM T_ACTIVITY_OWNER_BY_COND_PARTICIPANT WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition[i].ToString();
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();

						strSqLiteSQL = "INSERT INTO T_ACTIVITY_OWNER_BY_COND_PARTICIPANT " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							//activityOwnerByCondition.Add(Convert.ToInt64(readerMsSqlDataChild[""].ToString()));
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}
#endregion
#region T_ACTIVITY_OWNER_BY_CONDITION_CONDITION
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_OWNER_BY_CONDITION_CONDITION");
					for (var i = 0; i < activityOwnerByCondition.Count; i++)
					{
						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION_CONDITION WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition[i].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						cmdMsSqlDataChild.CommandText = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION_CONDITION WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition[i].ToString();
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();

						strSqLiteSQL = "INSERT INTO T_ACTIVITY_OWNER_BY_CONDITION_CONDITION " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							activityOwnerByCondition.Add(Convert.ToInt64(readerMsSqlDataChild["Activity_Owner_By_Condition_Id"].ToString()));
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}

#endregion
#region T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP");
					for (var i = 0; i < activityOwnerByCondition.Count; i++)
					{
						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition[i].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						cmdMsSqlDataChild.CommandText = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition[i].ToString();
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();

						strSqLiteSQL = "INSERT INTO T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							//activityOwnerByCondition.Add(Convert.ToInt64(readerMsSqlDataChild[""].ToString()));
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}

#endregion
#region T_ACTIVITY_PARTICIPANT
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_PARTICIPANT");
					for (var i = 0; i < activities.Count; i++)
					{
						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_PARTICIPANT WHERE Activity_ID=" + activities[i].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						cmdMsSqlDataChild.CommandText = "SELECT * FROM T_ACTIVITY_PARTICIPANT WHERE Activity_ID=" + activities[i].ToString();
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();

						strSqLiteSQL = "INSERT INTO T_ACTIVITY_PARTICIPANT " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{

							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}

#endregion
#region T_PROC_DESIGN_DRAW_PART_DETAIL
					columnTypes = getColumnTypesDictionary("T_PROC_DESIGN_DRAW_PART_DETAIL");
					// transfer  data
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_PROC_DESIGN_DRAW_PART  WHERE PROC_DESIGN_DRAW_ID=" + procDesignDrawId.ToString();
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						strMsSQLDataChild = "SELECT * FROM T_PROC_DESIGN_DRAW_PART_DETAIL WHERE PROC_DESIGN_DRAW_PART_ID=" + readerMsSql["PROC_DESIGN_DRAW_PART_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_PROC_DESIGN_DRAW_PART_DETAIL " + " ( ";
						currType = "";
						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_ROUTING_CONDITION
					columnTypes = getColumnTypesDictionary("T_ROUTING_CONDITION");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ROUTING WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{
						strMsSQLDataChild = "SELECT * FROM T_ROUTING_CONDITION WHERE ROUTING_ID = " + readerMsSql["ROUTING_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ROUTING_CONDITION " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_ROUTING_CONDITION_GROUP
					columnTypes = getColumnTypesDictionary("T_ROUTING_CONDITION_GROUP");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ROUTING WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ROUTING_CONDITION_GROUP WHERE ROUTING_ID = " + readerMsSql["ROUTING_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ROUTING_CONDITION_GROUP " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_ROUTING_DESIGN
					columnTypes = getColumnTypesDictionary("T_ROUTING_DESIGN");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ROUTING WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{
						strMsSQLDataChild = "SELECT * FROM T_ROUTING_DESIGN WHERE ROUTING_DESIGN_ID = " + readerMsSql["ROUTING_DESIGN_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ROUTING_DESIGN " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_FIELD_CONDITION
					columnTypes = getColumnTypesDictionary("T_FIELD_CONDITION");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_FIELD_CONDITION WHERE FIELD_ID = " + readerMsSql["FIELD_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_FIELD_CONDITION " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_FIELD_DATE_CONSTRAINT
					columnTypes = getColumnTypesDictionary("T_FIELD_DATE_CONSTRAINT");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_FIELD_DATE_CONSTRAINT WHERE FIELD_ID = " + readerMsSql["FIELD_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_FIELD_DATE_CONSTRAINT " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_FIELD_EXTENSION_NUMBER
					columnTypes = getColumnTypesDictionary("T_FIELD_EXTENSION_NUMBER");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_FIELD_EXTENSION_NUMBER WHERE FIELD_ID = " + readerMsSql["FIELD_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_FIELD_EXTENSION_NUMBER " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS
					columnTypes = getColumnTypesDictionary("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS WHERE FIELD_ID = " + readerMsSql["FIELD_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_FIELD_LABEL_TRANSLATION
					columnTypes = getColumnTypesDictionary("T_FIELD_LABEL_TRANSLATION");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_FIELD_LABEL_TRANSLATION WHERE FIELD_ID = " + readerMsSql["FIELD_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_FIELD_LABEL_TRANSLATION " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_FIELD_VALUE
					columnTypes = getColumnTypesDictionary("T_FIELD_VALUE");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_FIELD_VALUE WHERE FIELD_ID = " + readerMsSql["FIELD_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_FIELD_VALUE " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}


#endregion
#region T_ACTIVITY_DESIGN
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_DESIGN");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_DESIGN WHERE ACTIVITY_DESIGN_ID = " + readerMsSql["ACTIVITY_DESIGN_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ACTIVITY_DESIGN " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_ACTIVITY_FIELDS
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_FIELDS");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_FIELDS WHERE ACTIVITY_ID = " + readerMsSql["ACTIVITY_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ACTIVITY_FIELDS " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_ACTIVITY_FIELDS_FOR_ESIGNING
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_FIELDS_FOR_ESIGNING");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_FIELDS_FOR_ESIGNING WHERE ACTIVITY_ID = " + readerMsSql["ACTIVITY_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ACTIVITY_FIELDS_FOR_ESIGNING " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION WHERE ACTIVITY_ID = " + readerMsSql["ACTIVITY_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_ACTIVITY_DEPENDENT_COMPONENTS
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_DEPENDENT_COMPONENTS");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_DEPENDENT_COMPONENTS WHERE ACTIVITY_ID = " + readerMsSql["ACTIVITY_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
						strSqLiteSQL = "INSERT INTO T_ACTIVITY_DEPENDENT_COMPONENTS " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}

#endregion
#region T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION
					columnTypes = getColumnTypesDictionary("T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
					readerMsSql = cmdMsSql.ExecuteReader();
					strSQLiteValues = "";
					while (readerMsSql.Read())
					{

						strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_DEPENDENT_COMPONENTS WHERE ACTIVITY_ID = " + readerMsSql["ACTIVITY_ID"].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();


						while (readerMsSqlDataChild.Read())
						{
							strMsSQLDataGrandChild = "SELECT *  T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION  WHERE Activity_Dependent_UI_Components_ID = " + readerMsSqlDataChild["Activity_Dependent_UI_Components_ID"].ToString();
							cmdMsSqlDataGrandChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
							readerMsSqlDataGrandChild = cmdMsSqlDataGrandChild.ExecuteReader();


							while (readerMsSqlDataGrandChild.Read())
							{
								strSqLiteSQL = "INSERT INTO T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION " + " ( ";
								currType = "";
								strSQLiteValues = "";
								foreach (KeyValuePair<string, string> entry in columnTypes)
								{
									switch (entry.Value)
									{
										case "binary":
										case "varbinary":
										case "image":
											break;
										default:
											{
												strSqLiteSQL += entry.Key + ",";
												break;
											}
									}
								}
								strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";

								for (int j = 0; j < readerMsSqlDataGrandChild.FieldCount; j++)
								{
									columnTypes.TryGetValue(readerMsSqlDataGrandChild.GetName(j), out currType);
									switch (currType)
									{
										case "binary":
										case "varbinary":
										case "image":
											break;
										default:
											{
												strSQLiteValues += "'" + readerMsSqlDataGrandChild[j].ToString().Replace("'", "''") + "',";
												break;
											}
									}
								}
								strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
								cmdSqlite = new SQLiteCommand(connSqlite);
								cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
								cmdSqlite.ExecuteNonQuery();
							}
						}
					}

#endregion
#region T_DYNAMIC ROUTING
					columnTypes = getColumnTypesDictionary("T_DYNAMIC_ROUTING");
					List<long> selectedActivities = getActivities(processId);
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_DYNAMIC_ROUTING";
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{
						if ((selectedActivities.FindIndex(a => a == Convert.ToInt64(readerMsSql["from_activity_id"])) > 0)
							 || selectedActivities.FindIndex(a => a == Convert.ToInt64(readerMsSql["to_activity_id"])) > 0)
						{
							strSQLiteValues = "";
							strSqLiteSQL = "INSERT INTO T_DYNAMIC_ROUTING " + " ( ";
							currType = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";

							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}

					}

#endregion
#region T_CALCFIELD_FORMULA_STEPS___T_CALCFIELD_OPERAND
					// load process fields to list   
					fieldsForProcess = getProcessFields(processId);
					columnTypes = getColumnTypesDictionary("T_CALCFIELD_FORMULA_STEPS");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_CALCFIELD_FORMULA_STEPS";
					readerMsSql = cmdMsSql.ExecuteReader();

					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_REF"].ToString())) > 0)
						{
							if (operands.FindIndex(a => a == Convert.ToInt64(readerMsSql["CALCFIELD_OPERAND1_REF"])) == -1)
							{
								operands.Add(Convert.ToInt64(readerMsSql["CALCFIELD_OPERAND1_REF"]));
							}
							if (operands.FindIndex(a => a == Convert.ToInt64(readerMsSql["CALCFIELD_OPERAND2_REF"])) == -1)
							{
								operands.Add(Convert.ToInt64(readerMsSql["CALCFIELD_OPERAND2_REF"]));
							}
							strSQLiteValues = "";
							strSqLiteSQL = "INSERT INTO T_CALCFIELD_FORMULA_STEPS " + " ( ";
							currType = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";

							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}


					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_CALCFIELD_OPERAND";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (operands.FindIndex(a => a == Convert.ToInt64(readerMsSql["CALCFIELD_OPERAND_ID"])) > 0)
						{
							strSQLiteValues = "";
							strSqLiteSQL = "INSERT INTO T_CALCFIELD_OPERAND " + " ( ";
							currType = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";

							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}
#endregion
#region T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS
					columnTypes = getColumnTypesDictionary("T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS");
					// transfer structure info
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
						{

							strSqLiteSQL = "INSERT INTO T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_USER_DEFINED_TABLE
					columnTypes = getColumnTypesDictionary("T_USER_DEFINED_TABLE");

					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_USER_DEFINED_TABLE";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
						{
							udts.Add(Convert.ToInt64(readerMsSql["USER_DEFINED_TABLE_ID"]));
							strSQLiteValues = "";
							strSqLiteSQL = "INSERT INTO T_USER_DEFINED_TABLE " + " ( ";
							currType = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":

										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":

										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_FORMULA_STEPS
					columnTypes = getColumnTypesDictionary("T_FORMULA_STEPS");

					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FORMULA_STEPS";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (udts.FindIndex(a => a == Convert.ToInt64(readerMsSql["USER_DEFINED_TABLE_REF"].ToString())) > 0)
						{
							strSQLiteValues = "";
							strSqLiteSQL = "INSERT INTO T_FORMULA_STEPS " + " ( ";
							currType = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":

										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":

										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_OPERAND
					columnTypes = getColumnTypesDictionary("T_OPERAND");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_OPERAND";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (readerMsSql["OPERAND_USER_DEFINED_TABLE_REF"].ToString() != "")
						{
							if (udts.FindIndex(a => a == Convert.ToInt64(readerMsSql["OPERAND_USER_DEFINED_TABLE_REF"].ToString())) > 0)
							{
								strSQLiteValues = "";
								strSqLiteSQL = "INSERT INTO T_OPERAND " + " ( ";
								currType = "";
								foreach (KeyValuePair<string, string> entry in columnTypes)
								{
									switch (entry.Value)
									{
										case "binary":
										case "varbinary":
										case "image":

											break;
										default:
											{
												strSqLiteSQL += entry.Key + ",";
												break;
											}
									}
								}
								strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
								strSQLiteValues = "";
								for (int j = 0; j < readerMsSql.FieldCount; j++)
								{
									columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
									switch (currType)
									{
										case "binary":
										case "varbinary":
										case "image":

											break;
										default:
											{
												strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
												break;
											}
									}
								}
								strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
								cmdSqlite = new SQLiteCommand(connSqlite);
								cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
								cmdSqlite.ExecuteNonQuery();
							}
						}

					}

#endregion
#region T_PROCFIELD_PARTICIPANT
					columnTypes = getColumnTypesDictionary("T_PROCFIELD_PARTICIPANT");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_PROCFIELD_PARTICIPANT";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
						{

							strSqLiteSQL = "INSERT INTO T_PROCFIELD_PARTICIPANT " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_PROCFIELD_WORD_MERGE
					columnTypes = getColumnTypesDictionary("T_PROCFIELD_WORD_MERGE");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_PROCFIELD_WORD_MERGE";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
						{

							strSqLiteSQL = "INSERT INTO T_PROCFIELD_WORD_MERGE " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":
										strSqLiteSQL += entry.Key + ",";
										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
										strSQLiteValues += "'" + Convert.ToBase64String(((byte[])readerMsSql[j])) + "',";
										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_PROCFIELD_WORD_MERGE_FIELD
					columnTypes = getColumnTypesDictionary("T_PROCFIELD_WORD_MERGE_FIELD");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_PROCFIELD_WORD_MERGE_FIELD";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
						{

							strSqLiteSQL = "INSERT INTO T_PROCFIELD_WORD_MERGE_FIELD " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_AUTOMATIC_PROCESS
					columnTypes = getColumnTypesDictionary("T_AUTOMATIC_PROCESS");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_AUTOMATIC_PROCESS";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (activities.FindIndex(a => a == Convert.ToInt64(readerMsSql["Activity_Id"].ToString())) > 0)
						{

							strSqLiteSQL = "INSERT INTO T_AUTOMATIC_PROCESS " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion

#region T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY
					columnTypes = getColumnTypesDictionary("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY");
					cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
					cmdMsSql.CommandText = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY";
					readerMsSql = cmdMsSql.ExecuteReader();
					while (readerMsSql.Read())
					{
						if (activities.FindIndex(a => a == Convert.ToInt64(readerMsSql["Activity_Id"].ToString())) > 0)
						{

							strSqLiteSQL = "INSERT INTO T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY " + " ( ";
							currType = "";
							strSQLiteValues = "";
							foreach (KeyValuePair<string, string> entry in columnTypes)
							{
								switch (entry.Value)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSqLiteSQL += entry.Key + ",";
											break;
										}
								}
							}
							strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
							strSQLiteValues = "";
							for (int j = 0; j < readerMsSql.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSql.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":

										break;
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSql[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();
						}
					}
#endregion
#region T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY
					columnTypes = getColumnTypesDictionary("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY");
					for (var i = 0; i < activities.Count; i++)
					{
						strMsSQLDataChild = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY WHERE Activity_ID=" + activities[i].ToString();
						cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
						cmdMsSqlDataChild.CommandText = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY WHERE Activity_ID=" + activities[i].ToString();
						readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();

						strSqLiteSQL = "INSERT INTO T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY " + " ( ";
						currType = "";

						foreach (KeyValuePair<string, string> entry in columnTypes)
						{
							switch (entry.Value)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSqLiteSQL += entry.Key + ",";
										break;
									}
							}
						}
						strSqLiteSQL = strSqLiteSQL.Substring(0, strSqLiteSQL.Length - 1) + ") VALUES (";
						while (readerMsSqlDataChild.Read())
						{

							strSQLiteValues = "";
							for (int j = 0; j < readerMsSqlDataChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(readerMsSqlDataChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + readerMsSqlDataChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							cmdSqlite = new SQLiteCommand(connSqlite);
							cmdSqlite.CommandText = strSqLiteSQL + strSQLiteValues;
							cmdSqlite.ExecuteNonQuery();

						}
					}

					connSqlite.Close();
				}
			}
			catch (Exception ex)
			{
				connSqlite.Close();
				res = FillServiceCallResult(ex);

			}

			return res;
		}

		private ServiceCallResult createDatabaseAndTables(int processId)
		{
			SQLiteConnection connSQLite = new SQLiteConnection();
			string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + "\\";
			if (!Directory.Exists(ConfigurationManager.AppSettings.Get("sqlite_databases_root")))
			{
				Directory.CreateDirectory(ConfigurationManager.AppSettings.Get("sqlite_databases_root"));
			}

			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			// get process name
			string connStr = ConfigurationManager.AppSettings.Get("connstr");
			string strSQL = "SELECT Name FROM T_PROCESS WHERE Process_Id = @processId";
			string processName = "";
			using (SqlConnection connection = new SqlConnection(connStr))
			{
				SqlCommand command = new SqlCommand(strSQL, connection);
				command.Parameters.AddWithValue("@processId", processId);
				try
				{
					connection.Open();
					SqlDataReader reader;
					reader = command.ExecuteReader();
					reader.Read();
					processName = reader["Name"].ToString();
					reader.Close();
				}
				catch (Exception ex)
				{
					res = FillServiceCallResult(ex);
				}
				processName = processName.Replace(" ", "_");
				if (processName == "")
				{
					processName = "John_Doe";
				}
				fileName = fileName + processName + ".db";
				try
				{
					if (File.Exists(fileName))
					{
						GC.Collect();
						GC.WaitForPendingFinalizers();
						File.Delete(fileName);
					}
					SQLiteConnection.CreateFile(fileName);
					connSQLite = new SQLiteConnection(String.Format("Data Source={0} ;Version=3;", fileName));
					string strSql = "create table table_information(";
					strSql += "TABLE_NAME VARCHAR(100),COLUMN_NAME VARCHAR(100), COLUMN_DEFAULT VARCHAR(10) NULL,";
					strSql += "IS_NULLABLE  VARCHAR(10) ,DATA_TYPE VARCHAR(30),";
					strSql += "CHARACTER_MAXIMUM_LENGTH INTEGER NULL, NUMERIC_PRECISION INT NULL";
					strSql += ")";
					connSQLite.Open();
					//conn.SetPassword("password");
					SQLiteCommand sqliteCommand = new SQLiteCommand(strSql, connSQLite);
					sqliteCommand.ExecuteNonQuery();
					sqliteCommand = null;
					connSQLite.Close();

				}
				catch (Exception ex)
				{
					res = FillServiceCallResult(ex);
				}

				string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
				string sqliteDbPath;
				res = new ServiceCallResult { Code = 0, Description = "OK" };
				res = getSqlitePath(processId);
				string[] tablenames = {
			"T_PROCESS",
			"T_PROCESS_DESIGN",
			"T_PROC_DESIGN_DRAW",
			"T_PROC_DESIGN_DRAW_PART",
			"T_PROC_DESIGN_DRAW_PART_TYPE",
			"T_ROUTING",
			"T_FIELD",
			"T_FIELD_CONDITION_GROUP",
			"T_FIELD_DATE_TYPE",
			"T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE",
			"T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR",
			"T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY",
			"T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE",
			"T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE",
			"T_FIELD_TEXT_FORMAT_TYPE",
			"T_FIELD_TO_FIELD_DEPENDENCY_TYPE",
			"T_FIELD_TYPE",
			"T_FILE_FIELD_TYPE",
			"T_ACTIVITY",
			"T_ACTIVITY_FIELDS_UI_PARAMETERS",
			"T_NOTIFICATION",
			"T_PERSON",
			"T_DEPARTMENT",
			"T_DEPARTMENT_MEMBERS",
			"T_CALCULATED_FIELD_RESULT_TYPE_ID",
			"T_CATEGORY",
			"T_PROCESS_OWNER",
			"T_PROCESS_READER",
			"T_ROLE",
			"T_ROLE_MEMBERS",
			"T_REPORT_GROUP",
			"T_REPORT_GROUP_ADMINISTRATOR",
			"T_REPORT_OWNERS",
			"T_PROC_DESIGN_DRAW_PART_DETAIL",
			"T_ROUTING_CONDITION",
			"T_ROUTING_CONDITION_GROUP",
			"T_ROUTING_DESIGN",
			"T_FIELD_CONDITION",
			"T_FIELD_DATE_CONSTRAINT",
			"T_FIELD_EXTENSION_NUMBER",
			"T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS",
			"T_FIELD_LABEL_TRANSLATION",
			"T_FIELD_VALUE",
			"T_ACTIVITY_DESIGN",
			"T_ACTIVITY_FIELDS",
			"T_ACTIVITY_FIELDS_FOR_ESIGNING",
			"T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION",
			"T_ACTIVITY_DEPENDENT_COMPONENTS",
			"T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION",
			"T_DYNAMIC_ROUTING",
			"T_CALCFIELD_FORMULA_STEPS",
			"T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS",
			"T_USER_DEFINED_TABLE",
			"T_FORMULA_STEPS",
			"T_OPERAND",
			"T_PROCFIELD_PARTICIPANT",
			"T_PROCFIELD_WORD_MERGE",
			"T_PROCFIELD_WORD_MERGE_FIELD",
			"T_REPORT_FIELD",
			"T_REPORT",
			"T_REPORT_2_FIELD_COND_GROUP",
			"T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE",
			"T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE",
			"T_REPORT_EDIT_OWNER",
			"T_REPORT_FIELD_UDT_COLUMNS",
			"T_REPORT_FILTER",
			"T_REPORT_REFERENCED_FIELD_LOCATION",
			"T_SUBPROCESS",
			"T_ACTIVITY_OWNER_BY_CONDITION",
			"T_ACTIVITY_OWNER_BY_COND_PARTICIPANT",
			"T_ACTIVITY_OWNER_BY_CONDITION_CONDITION",
			"T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP",
			"T_ACTIVITY_PARTICIPANT",
			"T_ACTIVITY_UI_COMPONENT",
			"T_AUTOMATIC_PROCESS"  ,
			"T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY",
			"T_FIELD_TO_FIELD_DEPENDENCY",
			"T_FIELD_VALUE_TRANSLATION",
			"T_CHART_TYPE",
			"T_CHART_FIELD_TYPE",
			"T_LANGUAGE",
			"T_REPORT_TYPE",
			"T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE",
			"T_ACTIVITY_FINISH_STEP_MODE",
			"T_ACTIVITY_PARTICIPANT_TYPE",
			"T_CALCULATED_FIELD_CONSTANT_TYPE",
			"T_COMPARE_OPERATION",
			"T_DB_CONNECTION",
			"T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA",

		  };
				if (res.Code != 0)
				{
					return res;
				}
				sqliteDbPath = res.Description;

				connSQLite.Open();
				using (SqlConnection MSSQLConnection = new SqlConnection(connStrSQLServer))
				{
					string strSqLiteSQL = "";
					SQLiteCommand cmdSqlite;
					string strMsSQL = "";
					SqlCommand cmdMsSql;
					SqlDataReader readerMsSql;
					ServiceCallResult resGen;
					Dictionary<string, string> columnTypes;
					MSSQLConnection.Open();
					for (int i = 0; i < tablenames.Length; i++)
					{
						// transfer structure info
						cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
						cmdMsSql.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + tablenames[i] + "'";
						readerMsSql = cmdMsSql.ExecuteReader();
						columnTypes = getColumnTypesDictionary(tablenames[i]);
						while (readerMsSql.Read())
						{
							strSqLiteSQL = "INSERT INTO table_information (TABLE_NAME,COLUMN_NAME,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,";
							strSqLiteSQL += "CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION) ";
							strSqLiteSQL += "VALUES('%TABLE_NAME%','%COLUMN_NAME%', ";
							strSqLiteSQL += "'%COLUMN_DEFAULT%','%IS_NULLABLE%','%DATA_TYPE%',%CHARACTER_MAXIMUM_LENGTH%,%NUMERIC_PRECISION%)";
							strSqLiteSQL = strSqLiteSQL.Replace("%TABLE_NAME%", readerMsSql["TABLE_NAME"].ToString());
							strSqLiteSQL = strSqLiteSQL.Replace("%COLUMN_NAME%", readerMsSql["COLUMN_NAME"].ToString());
							strSqLiteSQL = strSqLiteSQL.Replace("%COLUMN_DEFAULT%", readerMsSql["COLUMN_DEFAULT"].ToString().Replace("'", "''"));
							strSqLiteSQL = strSqLiteSQL.Replace("%IS_NULLABLE%", readerMsSql["IS_NULLABLE"].ToString());
							strSqLiteSQL = strSqLiteSQL.Replace("%DATA_TYPE%", readerMsSql["DATA_TYPE"].ToString());
							switch (readerMsSql["DATA_TYPE"].ToString())
							{
								case "nvarchar":
								case "varchar":
									strSqLiteSQL = strSqLiteSQL.Replace("%CHARACTER_MAXIMUM_LENGTH%", readerMsSql["CHARACTER_MAXIMUM_LENGTH"].ToString());
									break;
								default:
									strSqLiteSQL = strSqLiteSQL.Replace("%CHARACTER_MAXIMUM_LENGTH%", "NULL");
									break;
							}
							if (readerMsSql["NUMERIC_PRECISION"].ToString() == "")
							{
								strSqLiteSQL = strSqLiteSQL.Replace("%NUMERIC_PRECISION%", "NULL");
							}
							else
							{
								strSqLiteSQL = strSqLiteSQL.Replace("%NUMERIC_PRECISION%", readerMsSql["NUMERIC_PRECISION"].ToString());
							}

							cmdSqlite = new SQLiteCommand(connSQLite);
							cmdSqlite.CommandText = strSqLiteSQL;
							cmdSqlite.ExecuteNonQuery();
						}
						// create SQLite table  
						resGen = GenerateSqliteTableCreationScript(tablenames[i]);
						if (resGen.Code == 0)
						{
							strSqLiteSQL = resGen.Description;
							cmdSqlite = new SQLiteCommand(connSQLite);
							cmdSqlite.CommandText = strSqLiteSQL;
							cmdSqlite.ExecuteNonQuery();
						}
						else
						{
							return res;
						}
					}
				}
				return res;
			}
		}
		private ServiceCallResult GenerateSqliteTableCreationScript(string CWPTableName)
		{
			string ret = "";
			string strSQLServer;
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };

			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			try
			{
				using (SqlConnection connection = new SqlConnection(connStrSQLServer))
				{
					connection.Open();
					strSQLServer = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + CWPTableName + "'";
					SqlCommand command = new SqlCommand(strSQLServer, connection);
					SqlDataReader table_structure_reader;
					table_structure_reader = command.ExecuteReader();
					ret = "CREATE TABLE " + CWPTableName + "(";
					while (table_structure_reader.Read())
					{
						switch (table_structure_reader["DATA_TYPE"].ToString())
						{
							case "binary":
							case "varbinary":
							case "image":
								ret += table_structure_reader["COLUMN_NAME"].ToString() + " BLOB, ";
								break;
							default:
								ret += table_structure_reader["COLUMN_NAME"].ToString() + " NVARCHAR, ";
								break;
						}
					}
					ret = ret.Substring(0, ret.Length - 2); // cut ","
					ret += ")";
					connection.Close();
					res.Code = 0;
					res.Description = ret;
				}
			}
			catch (Exception ex)
			{
				res = FillServiceCallResult(ex);

			}
			return res;
		}
		private Dictionary<string, string> getColumnTypesDictionary(string CWPTableName)
		{
			Dictionary<string, string> fields = new Dictionary<string, string>();
			string strSQLServer;
			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");

			using (SqlConnection connection = new SqlConnection(connStrSQLServer))
			{
				connection.Open();
				strSQLServer = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + CWPTableName + "'";
				SqlCommand command = new SqlCommand(strSQLServer, connection);
				SqlDataReader table_structure_reader;
				table_structure_reader = command.ExecuteReader();
				while (table_structure_reader.Read())
				{
					fields.Add(table_structure_reader["COLUMN_NAME"].ToString(), table_structure_reader["DATA_TYPE"].ToString());
				}
				connection.Close();
			}
			return fields;

		}
		private Int64 getProcessDesignIdFromProcess(Int64 processId)
		{
			Int64 ret = 0;
			string strSQLServer;
			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			using (SqlConnection connection = new SqlConnection(connStrSQLServer))
			{
				connection.Open();
				strSQLServer = "SELECT PROCESS_DESIGN_ID FROM T_PROCESS WHERE PROCESS_ID =" + processId.ToString();
				SqlCommand command = new SqlCommand(strSQLServer, connection);
				SqlDataReader reader;
				reader = command.ExecuteReader();
				reader.Read();
				ret = Convert.ToInt64(reader["PROCESS_DESIGN_ID"]);
				connection.Close();
			}
			return ret;
		}
		private Int64 getProcessDesignDrawId(Int64 processDesignId)
		{
			Int64 ret = 0;
			string strSQLServer;
			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			using (SqlConnection connection = new SqlConnection(connStrSQLServer))
			{
				connection.Open();
				strSQLServer = "SELECT PROC_DESIGN_DRAW_ID FROM T_PROC_DESIGN_DRAW WHERE PROCESS_DESIGN_ID =" + processDesignId.ToString();
				SqlCommand command = new SqlCommand(strSQLServer, connection);
				SqlDataReader reader;
				reader = command.ExecuteReader();
				reader.Read();
				ret = Convert.ToInt64(reader["PROC_DESIGN_DRAW_ID"]);
				connection.Close();
			}
			return ret;
		}
		private List<Int64> getActivities(Int64 pid)
		{
			List<long> ret = new List<long>();
			string strSQLServer;
			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			using (SqlConnection connection = new SqlConnection(connStrSQLServer))
			{
				connection.Open();
				strSQLServer = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID =" + pid.ToString();
				SqlCommand command = new SqlCommand(strSQLServer, connection);
				SqlDataReader reader;
				reader = command.ExecuteReader();
				while (reader.Read())
				{
					ret.Add(Convert.ToInt64(reader["ACTIVITY_ID"]));
				}
				connection.Close();
			}
			return ret;
		}
		private List<Int64> getProcessFields(Int64 pid)
		{
			List<long> ret = new List<long>();
			string strSQLServer;
			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			using (SqlConnection connection = new SqlConnection(connStrSQLServer))
			{
				connection.Open();
				//strSQLServer = "SELECT * FROM T_FIELD WHERE PROCESS_ID =" + pid.ToString();
				strSQLServer = "SELECT T_ACTIVITY.Process_ID, T_ACTIVITY_FIELDS.Field_ID FROM T_ACTIVITY_FIELDS INNER JOIN " +
							 " T_ACTIVITY ON T_ACTIVITY_FIELDS.Activity_ID = T_ACTIVITY.Activity_ID WHERE PROCESS_ID=" + pid.ToString();

				SqlCommand command = new SqlCommand(strSQLServer, connection);
				SqlDataReader reader;
				reader = command.ExecuteReader();
				while (reader.Read())
				{
					ret.Add(Convert.ToInt64(reader["FIELD_ID"]));
				}
				connection.Close();
			}
			return ret;
		}

		private bool IsProcessInList(Int64 processId)
		{
			bool boolFound = false;
			for (int i = 0; i < processes.Count; i++)
			{
				if (processes[i].ProcessId == processId)
				{
					boolFound = true;
				}
			}
			return boolFound;
		}

		private bool IsReportInList(Int64 reportId)
		{
			bool boolFound = false;
			for (int i = 0; i < reports.Count; i++)
			{
				if (reports[i].ReportId == reportId)
				{
					boolFound = true;
				}
			}
			return boolFound;
		}

		public void FillProcesses(Int64 processId)
		{
			fieldsForProcess = getProcessFields(processId);

			string strMsSQL = "";
			SqlCommand cmdMsSql;
			SqlDataReader readerMsSql;
			SqlCommand cmdMsSqlChild;
			SqlDataReader readerMsSqlChild;
			SqlCommand cmdMsSqlGrandChild;
			SqlDataReader readerMsSqlGrandChild;
			string strMSSqlChild;

			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			processes.Add(new ProcessListItem { ProcessId = processId, Processed = false, ReasonType = ProcessReasonType.MainProcess });
			using (SqlConnection MSSQLConnection = new SqlConnection(connStrSQLServer))
			{
				MSSQLConnection.Open();
				// -----  ProcessReasonType.SubProcess
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY_FIELDS";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if ((fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["Field_Id"].ToString())) > -1) &&
						  Convert.ToInt64(readerMsSql["Field_Type"].ToString()) > 1)
					{
						if ((readerMsSql["sub_process_id"] != null) && (readerMsSql["sub_process_id"].ToString() != ""))
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["sub_process_id"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["sub_process_id"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.SubProcess
								});
							}
						}
						if ((readerMsSql["parent_process_id"] != null) && (readerMsSql["parent_process_id"].ToString() != ""))
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["parent_process_id"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["parent_process_id"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.ParentProcess
								});
							}
						}
					}
				}

				// -----  ProcessReasonType.Report ----------------------------------------
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_REPORT_FIELD";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
					{
						if (!IsReportInList(Convert.ToInt64(readerMsSql["REPORT_ID"].ToString())))
						{
							reports.Add(new ReportListItem { ReportId = Convert.ToInt64(readerMsSql["REPORT_ID"].ToString()), Processed = false, });
						}
						reportFields.Add(Convert.ToInt64(readerMsSql["REPORT_FIELD_ID"].ToString()));
						if (readerMsSql["UDT_FIELD_ID"].ToString() != "")
						{
							udtReportFields.Add(Convert.ToInt64(readerMsSql["UDT_FIELD_ID"].ToString()));
						}
					}
				}
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if (reportFields.FindIndex(a => a == Convert.ToInt64(readerMsSql["Report_Field_ID"].ToString())) > 0)
					{
						if (readerMsSql["Referenced_Process_ID"] != null)
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.Report
								});
							}
						}
					}
				}
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if (reportFields.FindIndex(a => a == Convert.ToInt64(readerMsSql["Report_Field_ID"].ToString())) > 0)
					{
						if (readerMsSql["Referenced_Process_ID"] != null)
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.Report
								});
							}
						}
					}
				}



				// T_AUTOMATIC_PROCESS

				strMsSQL = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					strMSSqlChild = "SELECT T_PROCESS.Process_Alias_Id, T_PROCESS.Process_ID, T_AUTOMATIC_PROCESS.Activity_ID,T_PROCESS.Version_Status" +
							   " FROM  T_PROCESS INNER JOIN T_AUTOMATIC_PROCESS ON T_PROCESS.Process_Alias_Id = T_AUTOMATIC_PROCESS.Process_Alias_ID_To_Start " +
										  "WHERE Version_Status = 2 AND Activity_Id = " + Convert.ToInt64(readerMsSql["activity_id"].ToString()).ToString();

					cmdMsSqlChild = new SqlCommand(strMSSqlChild, MSSQLConnection);
					readerMsSqlChild = cmdMsSqlChild.ExecuteReader();
					while (readerMsSqlChild.Read())
					{
						if (!IsProcessInList(Convert.ToInt64(readerMsSqlChild["process_id"].ToString())))
						{
							processes.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(readerMsSqlChild["process_id"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.AutomaticProcess
							});

						}
					}
				}


			}
		}

		public void getFil(Int64 processId)
		{
			fieldsForProcess = getProcessFields(processId);

			string strMsSQL = "";
			SqlCommand cmdMsSql;
			SqlDataReader readerMsSql;
			SqlCommand cmdMsSqlChild;
			SqlDataReader readerMsSqlChild;
			SqlCommand cmdMsSqlGrandChild;
			SqlDataReader readerMsSqlGrandChild;
			string strMSSqlChild;

			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");
			processes.Add(new ProcessListItem { ProcessId = processId, Processed = false, ReasonType = ProcessReasonType.MainProcess });
			using (SqlConnection MSSQLConnection = new SqlConnection(connStrSQLServer))
			{
				MSSQLConnection.Open();
				// -----  ProcessReasonType.SubProcess
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY_FIELDS";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if ((fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["Field_Id"].ToString())) > -1) &&
							Convert.ToInt64(readerMsSql["Field_Type"].ToString()) > 1)
					{
						if ((readerMsSql["sub_process_id"] != null) && (readerMsSql["sub_process_id"].ToString() != ""))
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["sub_process_id"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["sub_process_id"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.SubProcess
								});
							}
						}
						if ((readerMsSql["parent_process_id"] != null) && (readerMsSql["parent_process_id"].ToString() != ""))
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["parent_process_id"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["parent_process_id"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.ParentProcess
								});
							}
						}
					}
				}

				// -----  ProcessReasonType.Report ----------------------------------------
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_REPORT_FIELD";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if (fieldsForProcess.FindIndex(a => a == Convert.ToInt64(readerMsSql["FIELD_ID"].ToString())) > 0)
					{
						if (!IsReportInList(Convert.ToInt64(readerMsSql["REPORT_ID"].ToString())))
						{
							reports.Add(new ReportListItem { ReportId = Convert.ToInt64(readerMsSql["REPORT_ID"].ToString()), Processed = false, });
						}
						reportFields.Add(Convert.ToInt64(readerMsSql["REPORT_FIELD_ID"].ToString()));
						if (readerMsSql["UDT_FIELD_ID"].ToString() != "")
						{
							udtReportFields.Add(Convert.ToInt64(readerMsSql["UDT_FIELD_ID"].ToString()));
						}
					}
				}
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if (reportFields.FindIndex(a => a == Convert.ToInt64(readerMsSql["Report_Field_ID"].ToString())) > 0)
					{
						if (readerMsSql["Referenced_Process_ID"] != null)
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.Report
								});
							}
						}
					}
				}
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				cmdMsSql.CommandText = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					if (reportFields.FindIndex(a => a == Convert.ToInt64(readerMsSql["Report_Field_ID"].ToString())) > 0)
					{
						if (readerMsSql["Referenced_Process_ID"] != null)
						{
							if (!IsProcessInList(Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString())))
							{
								processes.Add(new ProcessListItem
								{
									ProcessId = Convert.ToInt64(readerMsSql["Referenced_Process_ID"].ToString()),
									Processed = false,
									ReasonType = ProcessReasonType.Report
								});
							}
						}
					}
				}




				strMsSQL = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
				cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				readerMsSql = cmdMsSql.ExecuteReader();
				while (readerMsSql.Read())
				{
					strMSSqlChild = "SELECT T_PROCESS.Process_Alias_Id, T_PROCESS.Process_ID, T_AUTOMATIC_PROCESS.Activity_ID,T_PROCESS.Version_Status" +
								" FROM  T_PROCESS INNER JOIN T_AUTOMATIC_PROCESS ON T_PROCESS.Process_Alias_Id = T_AUTOMATIC_PROCESS.Process_Alias_ID_To_Start " +
											"WHERE Version_Status = 2 AND Activity_Id = " + Convert.ToInt64(readerMsSql["activity_id"].ToString()).ToString();

					cmdMsSqlChild = new SqlCommand(strMSSqlChild, MSSQLConnection);
					readerMsSqlChild = cmdMsSqlChild.ExecuteReader();
					while (readerMsSqlChild.Read())
					{
						if (!IsProcessInList(Convert.ToInt64(readerMsSqlChild["process_id"].ToString())))
						{
							processes.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(readerMsSqlChild["process_id"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.AutomaticProcess
							});

						}
					}
				}


			}
		}

	}
	public enum ProcessReasonType
	{
		MainProcess,
		DocRef,
		SubProcess,
		ParentProcess,
		Report,
		AutomaticProcess
	}
	public class TableNameAndCondition
	{
		public string TableName { get; set; }
		public string Condition { get; set; }

	}


	public class DynamicRouting
	{
		public Int64 From_Activity_ID { get; set; }
		public Int64 To_Activity_ID { get; set; }

	}
	public class ProcessListItem
	{

		private Int64 m_processId;
		private bool m_processed;
		private ProcessReasonType m_ReasonType;
		private bool m_CheckFlag;

		public Int64 ProcessId
		{
			get { return m_processId; }
			set { m_processId = value; }
		}
		public bool CheckFlag
		{
			get { return m_CheckFlag; }
			set { m_CheckFlag = value; }
		}
		public bool Processed
		{
			get { return m_processed; }
			set { m_processed = value; }
		}
		public ProcessReasonType ReasonType
		{
			get { return m_ReasonType; }
			set { m_ReasonType = value; }
		}

	}



	public class ReportListItem
	{
		private Int64 m_reportId;
		private bool m_processed;
		public Int64 ReportId
		{
			get { return m_reportId; }
			set { m_reportId = value; }
		}
		public bool Processed
		{
			get { return m_processed; }
			set { m_processed = value; }
		}

	}



}
/*
 T_PROCESS
 T_PROCESS_DESIGN
 T_PROC_DESIGN_DRAW
 T_PROC_DESIGN_DRAW_PART
 T_PROC_DESIGN_DRAW_PART_DETAIL

 T_ROUTING
 T_ROUTING_CONDITION
 T_ROUTING_CONDITION_GROUP
 T_ROUTING_DESIGN
 T_FIELD 
 T_FIELD_CONDITION_GROUP

 T_FIELD_CONDITION
 T_FIELD_DATE_CONSTRAINT
 T_FIELD_DATE_TYPE
 T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE
 T_FIELD_EXTENSION_NUMBER
 T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY
 T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE
 T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE
 T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS
 T_FIELD_LABEL_TRANSLATION
 T_FIELD_TEXT_FORMAT_TYPE
 T_FIELD_TYPE
 T_FIELD_VALUE
 T_FILE_FIELD_TYPE

 T_ACTIVITY
 T_ACTIVITY_DESIGN
 T_ACTIVITY_FIELDS
 T_ACTIVITY_FIELDS_FOR_ESIGNING
 T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION
 T_ACTIVITY_DEPENDENT_COMPONENT
 T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION
 T_ACTIVITY_FIELDS_UI_PARAMETERS
 T_DYNAMIC ROUTING 

 T_PERSON
 T_DEPARTMENT
 T_DEPARTMENT_MEMBERS
 T_CALCFIELD
 T_CALCFIELD_FORMULA_STEPS
 T_CALCFIELD_OPERAND
 T_CALCULATED_FIELD_RESULT_TYPE_ID
 T_CATEGORY
 T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS

 T_USER_DEFINED_TABLE
 T_FORMULA_STEPS 
 T_OPERAND
 T_PROCESS_OWNER
 T_PROCESS_READER

 T_PROCFIELD_PARTICIPANT
 T_PROCFIELD_WORD_MERGE
 T_PROCFIELD_WORD_MERGE_FIELD
 T_REPORT_FIELD
 T_REPORT
 T_REPORT_2_FIELD_COND_GROUP
 T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE
 T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE

 T_REPORT_EDIT_OWNER
 T_REPORT_FIELD_UDT_COLUMNS
 T_REPORT_FILTER
 T_REPORT_GROUP

 T_REPORT_GROUP_ADMINISTRATOR
 T_REPORT_OWNERS
 T_REPORT_REFERENCED_FIELD_LOCATION
 T_ROLE
 T_ROLE_MEMBERS


  T_SUBPROCESS
  T_ACTIVITY_OWNER_BY_COND_PARTICIPANT
  T_ACTIVITY_OWNER_BY_CONDITION
  T_ACTIVITY_OWNER_BY_CONDITION_CONDITION
  T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP
  T_ACTIVITY_PARTICIPANT
  T_ACTIVITY_UI_COMPONENT

  
 * *  
*  */

/*  EZEK KIMARADTAK, KELLENEK !!!!
*  
*   documentum referenciált processek
*   subprocessek
*   a report is hivatkozhat másik processre  ** -- OK beleírva
*   
  T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY
  T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS
  T_FIELD_TO_FIELD_DEPENDENCY
  T_FIELD_VALUE_TRANSLATION
*/
//private  ServiceCallResult CreateDatabase( int process_id) {
//  string fileName        = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + "/" ;
//  if (!Directory.Exists(ConfigurationManager.AppSettings.Get("sqlite_databases_root")))   {
//      Directory.CreateDirectory(ConfigurationManager.AppSettings.Get("sqlite_databases_root"));
//  }

//  ServiceCallResult res = new ServiceCallResult {Code=0,Description="OK" };
//  // get process name
//  string connStr = ConfigurationManager.AppSettings.Get("connstr");
//  string strSQL  = "SELECT Name FROM T_PROCESS WHERE Process_Id = @processId";
//  string processName = "";
//  using (SqlConnection connection = new SqlConnection(connStr)) {
//      SqlCommand command = new SqlCommand(strSQL, connection);
//      command.Parameters.AddWithValue("@processId", process_id);
//      try
//      {
//        connection.Open();
//        SqlDataReader reader;
//        reader = command.ExecuteReader();
//        reader.Read();
//        processName = reader["Name"].ToString();
//        reader.Close();
//      }
//      catch (Exception ex)
//      {
//        res = FillServiceCallResult(ex);
//      }
//      processName = processName.Replace(" ","_");
//      if( processName == "") {
//        processName = "John_Doe";
//      } 
//      fileName = fileName + processName + ".db";
//      try {
//        if( File.Exists(fileName)) {
//            GC.Collect();
//            GC.WaitForPendingFinalizers();
//            File.Delete(fileName);
//         }
//        SQLiteConnection.CreateFile(fileName);
//        string connSqlite = String.Format("Data Source={0} ;Version=3;",fileName);

//        SQLiteConnection connSQLite = new SQLiteConnection(connSqlite);
//        string strSql = "create table table_information(";
//        strSql += "TABLE_NAME VARCHAR(100),COLUMN_NAME VARCHAR(100), COLUMN_DEFAULT VARCHAR(10) NULL,";
//        strSql += "IS_NULLABLE  VARCHAR(10) ,DATA_TYPE VARCHAR(30),";
//        strSql += "CHARACTER_MAXIMUM_LENGTH INTEGER NULL, NUMERIC_PRECISION INT NULL";
//        strSql+= ")";
//        connSQLite.Open();
//       //conn.SetPassword("password");
//        SQLiteCommand sqliteCommand = new SQLiteCommand(strSql, connSQLite);
//        sqliteCommand.ExecuteNonQuery();
//        sqliteCommand = null;         
//        connSQLite.Close();

//      }
//      catch(Exception ex) {
//        res = FillServiceCallResult(ex);
//        return res;
//      }

//  }

//  return res;
//}

#endregion

#endregion