using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;
using System.Web;
using System.Data.SqlClient;
using static Process_Export_Import.Process_Export_Import;
using System.Data.SQLite;

namespace Process_Export_Import
{
	public class Export
	{
		public List<ProcessListItem> processes_v2 = new List<ProcessListItem>();

		public List<TableNameAndCondition> tables_v2 = new List<TableNameAndCondition>();

		public List<long> fieldsForProcess_v2 = new List<long>();

		public List<long> operands_v2 = new List<long>();

		public List<long> udts_v2 = new List<long>();

		public List<ReportListItem> reports_v2 = new List<ReportListItem>();

		public List<long> t_report_calculated_field_formula_tree_nodes_v2 = new List<long>();

		public List<long> reportFields_v2 = new List<long>();

		public List<long> udtReportFields_v2 = new List<long>();

		public List<long> activities_v2 = new List<long>();

		public List<long> activityOwnerByCondition_v2 = new List<long>();

		public Export()
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			
		}

		private Int64 getProcessDesignIdFromProcess_v2(Int64 processId, ConnectionManagerST obj)
		{
			Int64 ret = 0;
			string strSQLServer;
			strSQLServer = "SELECT PROCESS_DESIGN_ID FROM T_PROCESS WHERE PROCESS_ID =" + processId.ToString();
			var reader = obj.sqlServerDataReaderOld(strSQLServer);
			reader.Read();
			ret = Convert.ToInt64(reader["PROCESS_DESIGN_ID"]);

			return ret;
		}

		private Int64 getProcessDesignDrawId_v2(Int64 processDesignId, ConnectionManagerST obj)
		{
			Int64 ret = 0;
			string strSQLServer;
			strSQLServer = "SELECT PROC_DESIGN_DRAW_ID FROM T_PROC_DESIGN_DRAW WHERE PROCESS_DESIGN_ID =" + processDesignId.ToString();
			var reader = obj.sqlServerDataReaderOld(strSQLServer);
			reader.Read();
			ret = Convert.ToInt64(reader["PROC_DESIGN_DRAW_ID"]);


			return ret;
		}

		private List<Int64> getProcessFields_v2(Int64 pid, ConnectionManagerST obj)
		{
			List<long> ret = new List<long>();
			string strSQLServer;



			//strSQLServer = "SELECT * FROM T_FIELD WHERE PROCESS_ID =" + pid.ToString();
			strSQLServer = "SELECT T_ACTIVITY.Process_ID, T_ACTIVITY_FIELDS.Field_ID FROM T_ACTIVITY_FIELDS INNER JOIN " +
						 " T_ACTIVITY ON T_ACTIVITY_FIELDS.Activity_ID = T_ACTIVITY.Activity_ID WHERE PROCESS_ID=" + pid.ToString();


			var reader = obj.sqlServerDataReader(strSQLServer);
			while (reader.Read())
			{
				ret.Add(Convert.ToInt64(reader["FIELD_ID"]));
			}

			return ret;
		}

		private bool IsProcessInList(Int64 processId)
		{
			bool boolFound = false;
			for (int i = 0; i < processes_v2.Count; i++)
			{
				if (processes_v2[i].ProcessId == processId)
				{
					boolFound = true;
				}
			}
			return boolFound;
		}

		private bool IsReportInList(Int64 reportId)
		{
			bool boolFound = false;
			for (int i = 0; i < reports_v2.Count; i++)
			{
				if (reports_v2[i].ReportId == reportId)
				{
					boolFound = true;
				}
			}
			return boolFound;
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

		private Dictionary<string, string> getColumnTypesDictionary_v3(string CWPTableName, ConnectionManagerST obj)
		{
			Dictionary<string, string> fields = new Dictionary<string, string>();
			string commandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + CWPTableName + "'";
			var reader = obj.sqlServerDataReaderOld(commandText);
			while (reader.Read())
			{
				fields.Add(reader["COLUMN_NAME"].ToString(), reader["DATA_TYPE"].ToString());
			}
			return fields;

		}
		private List<Int64> getActivities_v2(Int64 pid , ConnectionManagerST obj)
		{
			List<long> ret = new List<long>();
		   
			string strSQLServer = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID =" + pid.ToString();
			var reader = obj.sqlServerDataReader(strSQLServer);
			while (reader.Read())
			{
				ret.Add(Convert.ToInt64(reader["ACTIVITY_ID"]));
			}
			
			return ret;
		}

		public ServiceCallResult TransferProcess_v2(Int64 processId, ConnectionManagerST obj, bool recurs = false)
		{

			ServiceCallResult res = new ServiceCallResult();
			res = getSqlitePath_v2(processId, obj);

			Int64 processDesignId = getProcessDesignIdFromProcess_v2(processId, obj);
			Int64 procDesignDrawId = getProcessDesignDrawId_v2(processDesignId, obj);
			#region tablesAdd
			fieldsForProcess_v2 = getProcessFields_v2(processId, obj);

			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROCESS", Condition = " WHERE PROCESS_ID = " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROCESS_DESIGN", Condition = " WHERE PROCESS_DESIGN_ID = " + processDesignId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROC_DESIGN_DRAW", Condition = " WHERE PROCESS_DESIGN_ID = " + processDesignId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROC_DESIGN_DRAW_PART", Condition = " WHERE PROC_DESIGN_DRAW_ID = " + procDesignDrawId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROC_DESIGN_DRAW_PART_TYPE", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ROUTING", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_CONDITION_GROUP", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_DATE_TYPE", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE", Condition = " WHERE 1=1  " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE", Condition = " WHERE 1=1  " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_TEXT_FORMAT_TYPE", Condition = " WHERE 1=1  " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_TO_FIELD_DEPENDENCY_TYPE", Condition = " WHERE 1=1  " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_TYPE", Condition = " WHERE 1=1  " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FILE_FIELD_TYPE", Condition = " WHERE 1=1  " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ACTIVITY", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_FIELDS_UI_PARAMETERS", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_NOTIFICATION", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PERSON", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_DEPARTMENT", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_DEPARTMENT_MEMBERS", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_CALCULATED_FIELD_RESULT_TYPE_ID", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_CATEGORY", Condition = " WHERE 1=1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROCESS_OWNER", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_PROCESS_READER", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ROLE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ROLE_MEMBERS", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_REPORT_GROUP", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_REPORT_GROUP_ADMINISTRATOR", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_REPORT_OWNERS", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_SUBPROCESS", Condition = " WHERE PROCESS_ID =  " + processId.ToString() });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_UI_COMPONENT", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_VALUE_TRANSLATION", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_CHART_TYPE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_CHART_FIELD_TYPE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_LANGUAGE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_REPORT_TYPE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_FINISH_STEP_MODE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_ACTIVITY_PARTICIPANT_TYPE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_CALCULATED_FIELD_CONSTANT_TYPE", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_COMPARE_OPERATION", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_DB_CONNECTION", Condition = " WHERE 1 = 1 " });
			tables_v2.Add(new TableNameAndCondition { TableName = "T_FIELD_TO_FIELD_DEPENDENCY", Condition = @" WHERE
			(( exists ( Select f.Field_ID from T_FIELD f where f.Process_ID =  " + processId.ToString() + @" and f.Field_ID = T_FIELD_TO_FIELD_DEPENDENCY.dependent_Field_ID ) )
			or
			( exists ( Select f.Field_ID from T_FIELD f where f.Process_ID = " + processId.ToString() + @" and f.Field_ID = T_FIELD_TO_FIELD_DEPENDENCY.independent_Field_ID ) ))		
			and
			((T_FIELD_TO_FIELD_DEPENDENCY.Dependency_Activation_Activity_ID = 0
			or 
			( exists ( Select a.Activity_ID from T_ACTIVITY a where a.Process_ID =" + processId.ToString() + " and a.Activity_ID = T_FIELD_TO_FIELD_DEPENDENCY.Dependency_Activation_Activity_ID ) ) ) )" });

			#endregion



			string connStrSQLServer = ConfigurationManager.AppSettings.Get("connstr");

			#region tables that can be transfer in simple way
			try
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

				for (int i = 0; i < tables_v2.Count; i++)
				{
					columnTypes = getColumnTypesDictionary_v3(tables_v2[i].TableName, obj);
					strMsSQLData = "SELECT * FROM " + tables_v2[i].TableName + tables_v2[i].Condition;
					var reader = obj.sqlServerDataReaderOld(strMsSQLData);

					strSqLiteSQL = "INSERT INTO " + tables_v2[i].TableName + " ( ";
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
					while (reader.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{

										strSQLiteValues += "'" + reader[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

					}
				}
				#endregion

				// fill activities array
				strMsSQL = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();

				var reader2 = obj.sqlServerDataReaderOld(strMsSQL);

				activities_v2 = new List<long>();
				while (reader2.Read())
				{
					activities_v2.Add(Convert.ToInt64(reader2["activity_id"].ToString()));
				}
				#region T_FIELD_TO_FIELD_DEPENDENCY
 
				
				#endregion
				#region T_ACTIVITY_OWNER_BY_CONDITION
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_OWNER_BY_CONDITION", obj);
				for (int i = 0; i < activities_v2.Count; i++)
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION WHERE ACTIVITY_ID=" + activities_v2[i].ToString();
					strSqLiteSQL = "INSERT INTO T_ACTIVITY_OWNER_BY_CONDITION " + " ( ";
					currType = "";

					var reader3 = obj.sqlServerDataReaderOld(strMsSQLDataChild);

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
					while (reader3.Read())
					{
						activityOwnerByCondition_v2.Add(Convert.ToInt64(reader3["Activity_Owner_By_Condition_ID"].ToString()));
						strSQLiteValues = "";
						for (int j = 0; j < reader3.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader3.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader3[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_ACTIVITY_OWNER_BY_COND_PARTICIPANT
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_OWNER_BY_COND_PARTICIPANT", obj);
				for (var i = 0; i < activityOwnerByCondition_v2.Count; i++)
				{
					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_COND_PARTICIPANT WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition_v2[i].ToString();
					strSqLiteSQL = "INSERT INTO T_ACTIVITY_OWNER_BY_COND_PARTICIPANT " + " ( ";
					currType = "";

					var reader4 = obj.sqlServerDataReaderOld(strMsSQLDataChild);

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
					while (reader4.Read())
					{
						//activityOwnerByCondition_v2.Add(Convert.ToInt64(reader4[""].ToString()));
						strSQLiteValues = "";
						for (int j = 0; j < reader4.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader4.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader4[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_ACTIVITY_OWNER_BY_CONDITION_CONDITION
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_OWNER_BY_CONDITION_CONDITION", obj);
				for (var i = 0; i < activityOwnerByCondition_v2.Count; i++)
				{
					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION_CONDITION WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition_v2[i].ToString();
					var reader5 = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader5.Read())
					{
						activityOwnerByCondition_v2.Add(Convert.ToInt64(reader5["Activity_Owner_By_Condition_Id"].ToString()));
						strSQLiteValues = "";
						for (int j = 0; j < reader5.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader5.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader5[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

					}
				}

				#endregion
				#region T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP", obj);
				for (var i = 0; i < activityOwnerByCondition_v2.Count; i++)
				{
					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP WHERE Activity_Owner_By_Condition_ID=" + activityOwnerByCondition_v2[i].ToString();
					var reader6 = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader6.Read())
					{
						//activityOwnerByCondition_v2.Add(Convert.ToInt64(reader6[""].ToString()));
						strSQLiteValues = "";
						for (int j = 0; j < reader6.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader6.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader6[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_ACTIVITY_PARTICIPANT
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_PARTICIPANT", obj);
				for (var i = 0; i < activities_v2.Count; i++)
				{
					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_PARTICIPANT WHERE Activity_ID=" + activities_v2[i].ToString();
					var reader7 = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader7.Read())
					{

						strSQLiteValues = "";
						for (int j = 0; j < reader7.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader7.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader7[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_PROC_DESIGN_DRAW_PART_DETAIL
				columnTypes = getColumnTypesDictionary_v3("T_PROC_DESIGN_DRAW_PART_DETAIL", obj);
				// transfer  data                
				string reader8CmdTxt = "SELECT * FROM T_PROC_DESIGN_DRAW_PART  WHERE PROC_DESIGN_DRAW_ID=" + procDesignDrawId.ToString();
				var reader8 = obj.sqlServerDataReaderOld(reader8CmdTxt);

				while (reader8.Read())
				{
					strMsSQLDataChild = "SELECT * FROM T_PROC_DESIGN_DRAW_PART_DETAIL WHERE PROC_DESIGN_DRAW_PART_ID=" + reader8["PROC_DESIGN_DRAW_PART_ID"].ToString();
					var reader8Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader8Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader8Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader8Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader8Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_ROUTING_CONDITION
				columnTypes = getColumnTypesDictionary_v3("T_ROUTING_CONDITION", obj);
				string reader9CmdTxt = "SELECT * FROM T_ROUTING WHERE PROCESS_ID=" + processId;
				var reader9 = obj.sqlServerDataReaderOld(reader9CmdTxt);

				while (reader9.Read())
				{
					strMsSQLDataChild = "SELECT * FROM T_ROUTING_CONDITION WHERE ROUTING_ID = " + reader9["ROUTING_ID"].ToString();
					var reader9Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader9Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader9Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader9Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader9Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_ROUTING_CONDITION_GROUP
				columnTypes = getColumnTypesDictionary_v3("T_ROUTING_CONDITION_GROUP", obj);
				string reader10CmdTxt = "SELECT * FROM T_ROUTING WHERE PROCESS_ID=" + processId;
				var reader10 = obj.sqlServerDataReaderOld(reader10CmdTxt);
				while (reader10.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ROUTING_CONDITION_GROUP WHERE ROUTING_ID = " + reader10["ROUTING_ID"].ToString();
					var reader10Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader10Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader10Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader10Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader10Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_ROUTING_DESIGN
				columnTypes = getColumnTypesDictionary_v3("T_ROUTING_DESIGN", obj);
				string reader11CmdTxt = "SELECT * FROM T_ROUTING WHERE PROCESS_ID=" + processId;
				var reader11 = obj.sqlServerDataReaderOld(reader11CmdTxt);
				while (reader11.Read())
				{
					strMsSQLDataChild = "SELECT * FROM T_ROUTING_DESIGN WHERE ROUTING_DESIGN_ID = " + reader11["ROUTING_DESIGN_ID"].ToString();
					var reader11Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader11Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader11Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader11Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader11Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_FIELD_CONDITION
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_CONDITION", obj);
				string reader12CmdTxt = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
				var reader12 = obj.sqlServerDataReaderOld(reader12CmdTxt);
				while (reader12.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_FIELD_CONDITION WHERE FIELD_ID = " + reader12["FIELD_ID"].ToString();
					var reader12Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader12Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader12Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader12Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader12Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_FIELD_DATE_CONSTRAINT
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_DATE_CONSTRAINT", obj);
				string reader13CmdTxt = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
				var reader13 = obj.sqlServerDataReaderOld(reader13CmdTxt);
				while (reader13.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_FIELD_DATE_CONSTRAINT WHERE FIELD_ID = " + reader13["FIELD_ID"].ToString();
					var reader13Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader13Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader13Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader13Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader13Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_FIELD_EXTENSION_NUMBER
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_EXTENSION_NUMBER", obj);
				string reader14CmdTxt = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
				var reader14 = obj.sqlServerDataReaderOld(reader14CmdTxt);
				while (reader14.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_FIELD_EXTENSION_NUMBER WHERE FIELD_ID = " + reader14["FIELD_ID"].ToString();
					var reader14Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader14Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader14Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader14Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader14Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS", obj);
				string reader15CmdTxt = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
				var reader15 = obj.sqlServerDataReaderOld(reader15CmdTxt);
				while (reader15.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS WHERE FIELD_ID = " + reader15["FIELD_ID"].ToString();
					var reader15Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader15Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader15Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader15Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader15Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_FIELD_LABEL_TRANSLATION
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_LABEL_TRANSLATION", obj);
				string reader16CmdTxt = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
				var reader16 = obj.sqlServerDataReaderOld(reader16CmdTxt);
				while (reader16.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_FIELD_LABEL_TRANSLATION WHERE FIELD_ID = " + reader16["FIELD_ID"].ToString();
					var reader16Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader16Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader16Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader16Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader16Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_FIELD_VALUE
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_VALUE", obj);
				string reader17CmdTxt = "SELECT * FROM T_FIELD WHERE PROCESS_ID=" + processId;
				var reader17 = obj.sqlServerDataReaderOld(reader17CmdTxt);
				while (reader17.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_FIELD_VALUE WHERE FIELD_ID = " + reader17["FIELD_ID"].ToString();
					strSqLiteSQL = "INSERT INTO T_FIELD_VALUE " + " ( ";
					var reader17Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader17Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader17Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader17Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader17Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}


				#endregion
				#region T_ACTIVITY_DESIGN
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_DESIGN", obj);
				string reader18CmdTxt = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
				var reader18 = obj.sqlServerDataReaderOld(reader18CmdTxt);
				while (reader18.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_DESIGN WHERE ACTIVITY_DESIGN_ID = " + reader18["ACTIVITY_DESIGN_ID"].ToString();
					var reader18Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader18Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader18Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader18Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader18Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_ACTIVITY_FIELDS
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_FIELDS", obj);
				string reader19CmdTxt = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
				var reader19 = obj.sqlServerDataReaderOld(reader19CmdTxt);
				while (reader19.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_FIELDS WHERE ACTIVITY_ID = " + reader19["ACTIVITY_ID"].ToString();
					var reader19Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader19Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader19Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader19Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader19Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_ACTIVITY_FIELD_TYPE
				/* columnTypes = getColumnTypesDictionary("T_ACTIVITY_FIELD_TYPE");
				 cmdMsSql = new SqlCommand(strMsSQL, MSSQLConnection);
				 cmdMsSql.CommandText = "SELECT * FROM T_ACTIVITY_FIELD_TYPE" + processId;
				 readerMsSql = cmdMsSql.ExecuteReader();

				 while (readerMsSql.Read())
				 {

					 strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_FIELD_TYPE ;";
					 cmdMsSqlDataChild = new SqlCommand(strMsSQLDataChild, MSSQLConnection);
					 readerMsSqlDataChild = cmdMsSqlDataChild.ExecuteReader();
					 strSqLiteSQL = "INSERT INTO T_ACTIVITY_FIELD_TYPE " + " ( ";
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

 */
				#endregion
				#region T_ACTIVITY_FIELDS_FOR_ESIGNING
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_FIELDS_FOR_ESIGNING", obj);
				string reader20CmdTxt = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
				var reader20 = obj.sqlServerDataReaderOld(reader20CmdTxt);
				while (reader20.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_FIELDS_FOR_ESIGNING WHERE ACTIVITY_ID = " + reader20["ACTIVITY_ID"].ToString();
					var reader20Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader20Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader20Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader20Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader20Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION", obj);
				string reader21CmdTxt = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
				var reader21 = obj.sqlServerDataReaderOld(reader21CmdTxt);

				while (reader21.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION WHERE ACTIVITY_ID = " + reader21["ACTIVITY_ID"].ToString();
					var reader21Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader21Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader21Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader21Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader21Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_ACTIVITY_DEPENDENT_COMPONENTS
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_DEPENDENT_COMPONENTS", obj);
				string reader22CmdTxt = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
				var reader22 = obj.sqlServerDataReaderOld(reader22CmdTxt);
				while (reader22.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_DEPENDENT_COMPONENTS WHERE ACTIVITY_ID = " + reader22["ACTIVITY_ID"].ToString();
					var reader22Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);
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
					while (reader22Child.Read())
					{
						strSQLiteValues = "";
						for (int j = 0; j < reader22Child.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader22Child.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader22Child[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				#endregion
				#region T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION
				columnTypes = getColumnTypesDictionary_v3("T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION", obj);
				strSQLiteValues = "";
				string reader23CmdTxt = "SELECT * FROM T_ACTIVITY WHERE PROCESS_ID=" + processId;
				var reader23 = obj.sqlServerDataReaderOld(reader23CmdTxt);

				while (reader23.Read())
				{

					strMsSQLDataChild = "SELECT * FROM T_ACTIVITY_DEPENDENT_COMPONENTS WHERE ACTIVITY_ID = " + reader23["ACTIVITY_ID"].ToString();
					var reader23Child = obj.sqlServerDataReaderOld(strMsSQLDataChild);

					while (reader23Child.Read())
					{
						//strMsSQLDataGrandChild = "SELECT *  T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION  WHERE Activity_Dependent_UI_Components_ID = " + reader23Child["Activity_Dependent_UI_Components_ID"].ToString();
						string reader23GrandChildCmdTxt = "SELECT *  T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION  WHERE Activity_Dependent_UI_Components_ID = " + reader23Child["Activity_Dependent_UI_Components_ID"].ToString();
						var reader23GrandChild = obj.sqlServerDataReaderOld(reader23GrandChildCmdTxt);


						while (reader23GrandChild.Read())
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

							for (int j = 0; j < reader23GrandChild.FieldCount; j++)
							{
								columnTypes.TryGetValue(reader23GrandChild.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + reader23GrandChild[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

						}
					}
				}

				#endregion
				#region T_DYNAMIC ROUTING
				columnTypes = getColumnTypesDictionary_v3("T_DYNAMIC_ROUTING", obj);
				List<long> selectedActivities = getActivities_v2(processId, obj);
				string reader24CmdTxt = "SELECT * FROM T_DYNAMIC_ROUTING";
				var reader24 = obj.sqlServerDataReaderOld(reader24CmdTxt);

				while (reader24.Read())
				{
					if ((selectedActivities.FindIndex(a => a == Convert.ToInt64(reader24["from_activity_id"])) > 0)
						 || selectedActivities.FindIndex(a => a == Convert.ToInt64(reader24["to_activity_id"])) > 0)
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

						for (int j = 0; j < reader24.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader24.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader24[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);


					}

				}

				#endregion
				#region T_CALCFIELD_FORMULA_STEPS___T_CALCFIELD_OPERAND
				// load process fields to list   
				fieldsForProcess_v2 = getProcessFields_v2(processId, obj);
				columnTypes = getColumnTypesDictionary_v3("T_CALCFIELD_FORMULA_STEPS", obj);
				string reader25CmdTxt = "SELECT * FROM T_CALCFIELD_FORMULA_STEPS";
				var reader25 = obj.sqlServerDataReaderOld(reader25CmdTxt);

				while (reader25.Read())
				{
					if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader25["FIELD_REF"].ToString())) > 0)
					{
						if (operands_v2.FindIndex(a => a == Convert.ToInt64(reader25["CALCFIELD_OPERAND1_REF"])) == -1)
						{
							operands_v2.Add(Convert.ToInt64(reader25["CALCFIELD_OPERAND1_REF"]));
						}
						if (operands_v2.FindIndex(a => a == Convert.ToInt64(reader25["CALCFIELD_OPERAND2_REF"])) == -1)
						{
							operands_v2.Add(Convert.ToInt64(reader25["CALCFIELD_OPERAND2_REF"]));
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

						for (int j = 0; j < reader25.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader25.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader25[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}

				string reader26CmdTxt = "SELECT * FROM T_CALCFIELD_OPERAND";
				var reader26 = obj.sqlServerDataReaderOld(reader26CmdTxt);

				while (reader26.Read())
				{
					if (operands_v2.FindIndex(a => a == Convert.ToInt64(reader26["CALCFIELD_OPERAND_ID"])) > 0)
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

						for (int j = 0; j < reader26.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader26.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader26[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

					}
				}
				#endregion
				#region T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS", obj);
				// transfer structure info
				string reader27CmdTxt = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS";
				var reader27 = obj.sqlServerDataReaderOld(reader27CmdTxt);

				while (reader27.Read())
				{
					if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader27["FIELD_ID"].ToString())) > 0)
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
						for (int j = 0; j < reader27.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader27.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader27[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_USER_DEFINED_TABLE
				columnTypes = getColumnTypesDictionary_v3("T_USER_DEFINED_TABLE", obj);
				string reader28CmdTxt = "SELECT * FROM T_USER_DEFINED_TABLE";
				var reader28 = obj.sqlServerDataReaderOld(reader28CmdTxt);

				while (reader28.Read())
				{
					if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader28["FIELD_ID"].ToString())) > 0)
					{
						udts_v2.Add(Convert.ToInt64(reader28["USER_DEFINED_TABLE_ID"]));
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
						for (int j = 0; j < reader28.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader28.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":

									break;
								default:
									{
										strSQLiteValues += "'" + reader28[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_FORMULA_STEPS
				columnTypes = getColumnTypesDictionary_v3("T_FORMULA_STEPS", obj);
				string reader29CmdTxt = "SELECT * FROM T_FORMULA_STEPS";
				var reader29 = obj.sqlServerDataReaderOld(reader29CmdTxt);

				while (reader29.Read())
				{
					if (udts_v2.FindIndex(a => a == Convert.ToInt64(reader29["USER_DEFINED_TABLE_REF"].ToString())) > 0)
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
						for (int j = 0; j < reader29.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader29.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":

									break;
								default:
									{
										strSQLiteValues += "'" + reader29[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_OPERAND
				columnTypes = getColumnTypesDictionary_v3("T_OPERAND", obj);
				string reader30CmdTxt = "SELECT * FROM T_OPERAND";
				var reader30 = obj.sqlServerDataReaderOld(reader30CmdTxt);

				while (reader30.Read())
				{
					if (reader30["OPERAND_USER_DEFINED_TABLE_REF"].ToString() != "")
					{
						if (udts_v2.FindIndex(a => a == Convert.ToInt64(reader30["OPERAND_USER_DEFINED_TABLE_REF"].ToString())) > 0)
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
							for (int j = 0; j < reader30.FieldCount; j++)
							{
								columnTypes.TryGetValue(reader30.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":

										break;
									default:
										{
											strSQLiteValues += "'" + reader30[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
						}
					}

				}

				#endregion
				#region T_PROCFIELD_PARTICIPANT
				columnTypes = getColumnTypesDictionary_v3("T_PROCFIELD_PARTICIPANT", obj);
				string reader31CmdTxt = "SELECT * FROM T_PROCFIELD_PARTICIPANT";
				var reader31 = obj.sqlServerDataReaderOld(reader31CmdTxt);

				while (reader31.Read())
				{
					if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader31["FIELD_ID"].ToString())) > 0)
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
						for (int j = 0; j < reader31.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader31.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader31[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_PROCFIELD_WORD_MERGE
				columnTypes = getColumnTypesDictionary_v3("T_PROCFIELD_WORD_MERGE", obj);
				string reader32CmdTxt = "SELECT * FROM T_PROCFIELD_WORD_MERGE";
				var reader32 = obj.sqlServerDataReaderOld(reader32CmdTxt);

				while (reader32.Read())
				{
					if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader32["FIELD_ID"].ToString())) > 0)
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
						for (int j = 0; j < reader32.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader32.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":
									strSQLiteValues += "'" + Convert.ToBase64String(((byte[])reader32[j])) + "',";
									break;
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader32[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
					}
				}
				#endregion
				#region T_PROCFIELD_WORD_MERGE_FIELD
				columnTypes = getColumnTypesDictionary_v3("T_PROCFIELD_WORD_MERGE_FIELD", obj);
				string reader33CmdTxt = "SELECT * FROM T_PROCFIELD_WORD_MERGE_FIELD";
				var reader33 = obj.sqlServerDataReaderOld(reader33CmdTxt);

				while (reader33.Read())
				{
					if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader33["FIELD_ID"].ToString())) > 0)
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
						for (int j = 0; j < reader33.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader33.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":

									break;
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader33[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

					}
				}
				#endregion
				#region T_AUTOMATIC_PROCESS
				columnTypes = getColumnTypesDictionary_v3("T_AUTOMATIC_PROCESS", obj);
				string reader34CmdTxt = "SELECT * FROM T_AUTOMATIC_PROCESS";
				var reader34 = obj.sqlServerDataReaderOld(reader34CmdTxt);

				while (reader34.Read())
				{
					if (activities_v2.FindIndex(a => a == Convert.ToInt64(reader34["Activity_Id"].ToString())) > 0)
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
						for (int j = 0; j < reader34.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader34.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":

									break;
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader34[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

					}
				}
				#endregion

				#region T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY
			/*	columnTypes = getColumnTypesDictionary_v3("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY", obj);
				string reader35CmdTxt = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY";
				var reader35 = obj.sqlServerDataReaderOld(reader35CmdTxt);

				while (reader35.Read())
				{
					if (activities_v2.FindIndex(a => a == Convert.ToInt64(reader35["Activity_Id"].ToString())) > 0)
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
						for (int j = 0; j < reader35.FieldCount; j++)
						{
							columnTypes.TryGetValue(reader35.GetName(j), out currType);
							switch (currType)
							{
								case "binary":
								case "varbinary":

									break;
								case "image":
									break;
								default:
									{
										strSQLiteValues += "'" + reader35[j].ToString().Replace("'", "''") + "',";
										break;
									}
							}
						}
						strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
						obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);

					}
				}*/
				#endregion
				#region T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY
				columnTypes = getColumnTypesDictionary_v3("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY", obj);
				for (var i = 0; i < activities_v2.Count; i++)
				{
					string reader36CmdTxt = "SELECT * FROM T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY WHERE Activity_ID=" + activities_v2[i].ToString();
					var reader36 = obj.sqlServerDataReaderOld(reader36CmdTxt);
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
					while (reader36.Read())
					{
						if (activities_v2.FindIndex(a => a == Convert.ToInt64(reader34["Activity_Id"].ToString())) > 0)
						{

							strSQLiteValues = "";
							for (int j = 0; j < reader36.FieldCount; j++)
							{
								columnTypes.TryGetValue(reader36.GetName(j), out currType);
								switch (currType)
								{
									case "binary":
									case "varbinary":
									case "image":
										break;
									default:
										{
											strSQLiteValues += "'" + reader36[j].ToString().Replace("'", "''") + "',";
											break;
										}
								}
							}
							strSQLiteValues = strSQLiteValues.Substring(0, strSQLiteValues.Length - 1) + ")";
							obj.executeQueriesInDbFile(strSqLiteSQL + strSQLiteValues);
						}
					}
				}



			}
			#endregion

			catch (Exception ex)
			{

				res = FillServiceCallResult_v2(ex);

			}

			return res;
		}

		public ServiceCallResult FillServiceCallResult_v2(Exception ex)
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

		public ServiceCallResult getSqlitePath_v2(Int64 process_Id , ConnectionManagerST obj)
		{
			string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + "\\";
			string strSQL = "SELECT Name FROM T_PROCESS WHERE Process_Id = @param";
			string processName = "";
			var reader = obj.sqlOldServerDataReaderWithSingleParameter(strSQL, process_Id.ToString());
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
				try
				{
					reader.Read();
					processName = reader["Name"].ToString();
					reader.Close();
				}
				catch (Exception ex)
				{
					res = FillServiceCallResult_v2(ex);
				}
				//res.Description = 
				processName = processName.Replace(" ", "_");
				fileName = fileName + processName + ".db";
				res.Description = fileName;
			
			return res;
		}

		public ServiceCallResult createDatabaseAndTables_v2(int processId, ConnectionManagerST obj)
		{
			SQLiteConnection connSQLite = new SQLiteConnection();
			string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root");
			if (!Directory.Exists(ConfigurationManager.AppSettings.Get("sqlite_databases_root")))
			{
				Directory.CreateDirectory(ConfigurationManager.AppSettings.Get("sqlite_databases_root"));
			}

			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };

			string strSQL = "SELECT Process_Id FROM T_PROCESS WHERE Process_Id = @param";
			string processName = "";
			var reader = obj.sqlOldServerDataReaderWithSingleParameter(strSQL, processId.ToString());
			try
			{
				while (reader.Read())
				{
					processName = reader["Process_Id"].ToString();
				}
			}
			catch (Exception ex)
			{
				res = FillServiceCallResult_v2(ex);
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

			}
			catch (Exception ex)
			{
				res = FillServiceCallResult_v2(ex);
			}
			return res;
		}

		public void FillProcesses_v2(Int64 processId, ConnectionManagerST obj)
		{
			fieldsForProcess_v2 = getProcessFields_v2(processId, obj);
			string strMsSQL = "";
			SqlCommand cmdMsSql;
			SqlDataReader readerMsSql;
			SqlCommand cmdMsSqlChild;
			SqlDataReader readerMsSqlChild;
			SqlCommand cmdMsSqlGrandChild;
			SqlDataReader readerMsSqlGrandChild;
			string strMSSqlChild;
			processes_v2.Add(new ProcessListItem { ProcessId = processId, Processed = false, ReasonType = ProcessReasonType.MainProcess });
			// -----  ProcessReasonType.SubProcess

			string cmdTxt = "SELECT * FROM T_ACTIVITY_FIELDS";
			var reader = obj.sqlServerDataReaderOld(cmdTxt);
			while (reader.Read())
			{
				if ((fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader["Field_Id"].ToString())) > -1) &&
						Convert.ToInt64(reader["Field_Type"].ToString()) > 1)
				{
					if ((reader["sub_process_id"] != null) && (reader["sub_process_id"].ToString() != ""))
					{
						if (!IsProcessInList(Convert.ToInt64(reader["sub_process_id"].ToString())))
						{
							processes_v2.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(reader["sub_process_id"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.SubProcess
							});
						}
					}
					if ((reader["parent_process_id"] != null) && (reader["parent_process_id"].ToString() != ""))
					{
						if (!IsProcessInList(Convert.ToInt64(reader["parent_process_id"].ToString())))
						{
							processes_v2.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(reader["parent_process_id"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.ParentProcess
							});
						}
					}
				}
			}

			// -----  ProcessReasonType.Report ----------------------------------------

			string cmdTxt2 = "SELECT * FROM T_REPORT_FIELD";
			var reader2 = obj.sqlServerDataReaderOld(cmdTxt2);
			while (reader2.Read())
			{
				if (fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader2["FIELD_ID"].ToString())) > 0)
				{
					if (!IsReportInList(Convert.ToInt64(reader2["REPORT_ID"].ToString())))
					{
						reports_v2.Add(new ReportListItem { ReportId = Convert.ToInt64(reader2["REPORT_ID"].ToString()), Processed = false, });
					}
					reportFields_v2.Add(Convert.ToInt64(reader2["REPORT_FIELD_ID"].ToString()));
					if (reader2["UDT_FIELD_ID"].ToString() != "")
					{
						udtReportFields_v2.Add(Convert.ToInt64(reader2["UDT_FIELD_ID"].ToString()));
					}
				}
			}
			string cmdTxt3 = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
			var reader3 = obj.sqlServerDataReaderOld(cmdTxt3);

			while (reader3.Read())
			{
				if (reportFields_v2.FindIndex(a => a == Convert.ToInt64(reader3["Report_Field_ID"].ToString())) > 0)
				{
					if (reader3["Referenced_Process_ID"] != null)
					{
						if (!IsProcessInList(Convert.ToInt64(reader3["Referenced_Process_ID"].ToString())))
						{
							processes_v2.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(reader3["Referenced_Process_ID"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.Report
							});
						}
					}
				}
			}
			string cmdTxt4 = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
			var reader4 = obj.sqlServerDataReaderOld(cmdTxt4);

			while (reader4.Read())
			{
				if (reportFields_v2.FindIndex(a => a == Convert.ToInt64(reader4["Report_Field_ID"].ToString())) > 0)
				{
					if (reader4["Referenced_Process_ID"] != null)
					{
						if (!IsProcessInList(Convert.ToInt64(reader4["Referenced_Process_ID"].ToString())))
						{
							processes_v2.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(reader4["Referenced_Process_ID"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.Report
							});
						}
					}
				}
			}



			// T_AUTOMATIC_PROCESS

			strMsSQL = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
			string cmdTxt5 = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
			var reader5 = obj.sqlServerDataReaderOld(cmdTxt5);

			while (reader5.Read())
			{
				strMSSqlChild = "SELECT T_PROCESS.Process_Alias_Id, T_PROCESS.Process_ID, T_AUTOMATIC_PROCESS.Activity_ID,T_PROCESS.Version_Status" +
							" FROM  T_PROCESS INNER JOIN T_AUTOMATIC_PROCESS ON T_PROCESS.Process_Alias_Id = T_AUTOMATIC_PROCESS.Process_Alias_ID_To_Start " +
										"WHERE Version_Status = 2 AND Activity_Id = " + Convert.ToInt64(reader5["activity_id"].ToString()).ToString();


				var reader5Child = obj.sqlServerDataReaderOld(strMSSqlChild);
				while (reader5Child.Read())
				{
					if (!IsProcessInList(Convert.ToInt64(reader5Child["process_id"].ToString())))
					{
						processes_v2.Add(new ProcessListItem
						{
							ProcessId = Convert.ToInt64(reader5Child["process_id"].ToString()),
							Processed = false,
							ReasonType = ProcessReasonType.AutomaticProcess
						});

					}
				}
			}



		}

		public bool CheckIfProcessExistInDatabase_v2(Int64 process_Id , ConnectionManagerST obj)
		{
			string strSQL = "SELECT  count(*) from T_PROCESS where Process_ID = @param";
			var reader = obj.sqlServerDataReaderWithSingleParameter(strSQL, process_Id.ToString());
			bool processIdFound = false;
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			try
			{
				reader.Read();
				if ((int)reader[0] == 1)
				{
					processIdFound = true;
				}
				reader.Close();

			}
			catch (Exception ex)
			{
				res = FillServiceCallResult_v2(ex);
			}
			
		return processIdFound;

		}

		private ServiceCallResult GenerateSqliteTableCreationScript_v2(string CWPTableName , ConnectionManagerST obj)
		{
			string ret = "";
			string strSQLServer;
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };

			try
			{
					strSQLServer = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + CWPTableName + "'";
					var reader = obj.sqlServerDataReader(strSQLServer);
					ret = "CREATE TABLE " + CWPTableName + "(";
					while (reader.Read())
					{
						switch (reader["DATA_TYPE"].ToString())
						{
							case "binary":
							case "varbinary":
							case "image":
								ret += reader["COLUMN_NAME"].ToString() + " BLOB, ";
								break;
							default:
								ret += reader["COLUMN_NAME"].ToString() + " NVARCHAR, ";
								break;
						}
					}
					ret = ret.Substring(0, ret.Length - 2); // cut ","
					ret += ")";
					res.Code = 0;
					res.Description = ret;
				
			}
			catch (Exception ex)
			{
				res = FillServiceCallResult_v2(ex);

			}
			return res;
		}

		public ServiceCallResult addTablesAndInfos(ConnectionManagerST obj)
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };

			string strSql = "create table table_information(";
			strSql += "TABLE_NAME VARCHAR(100),COLUMN_NAME VARCHAR(100), COLUMN_DEFAULT VARCHAR(10) NULL,";
			strSql += "IS_NULLABLE  VARCHAR(10) ,DATA_TYPE VARCHAR(30),";
			strSql += "CHARACTER_MAXIMUM_LENGTH INTEGER NULL, NUMERIC_PRECISION INT NULL";
			strSql += ")";
			try
			{
				obj.executeQueriesInDbFile(strSql);
			}
			catch (Exception ex)
			{
				res = FillServiceCallResult_v2(ex);
			}


			res = new ServiceCallResult { Code = 0, Description = "OK" };

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


			string strSqLiteSQL = "";
			string strMsSQL = "";
			string CommandText = "";
			Dictionary<string, string> columnTypes;
			for (int i = 0; i < tablenames.Length; i++)
			{

				CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + tablenames[i] + "'";
				var readerMsSql = obj.sqlServerDataReader(CommandText);
				columnTypes = getColumnTypesDictionary_v2(tablenames[i], obj);
				ServiceCallResult resGen;
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

					obj.executeQueriesInDbFile(strSqLiteSQL);
				}
				// create SQLite table  
				resGen = GenerateSqliteTableCreationScript_v2(tablenames[i] , obj);
				if (resGen.Code == 0)
				{
					strSqLiteSQL = resGen.Description;
					obj.executeQueriesInDbFile(strSqLiteSQL);

				}
				else
				{
					return res;
				}
			}

			return res;
		}
	}
}
