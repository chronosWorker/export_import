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



		private List<ProcessListItem> processes_v2 = new List<ProcessListItem>();

		private List<TableNameAndCondition> tables_v2 = new List<TableNameAndCondition>();

		private List<long> fieldsForProcess_v2 = new List<long>();

		private List<long> operands_v2 = new List<long>();

		private List<long> udts_v2 = new List<long>();

		private List<ReportListItem> reports_v2 = new List<ReportListItem>();

		private List<long> t_report_calculated_field_formula_tree_nodes_v2 = new List<long>();

		private List<long> reportFields_v2 = new List<long>();

		private List<long> udtReportFields_v2 = new List<long>();

		private List<long> activities_v2 = new List<long>();

		private List<long> activityOwnerByCondition_v2 = new List<long>();

		public Export()
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			

		}


		private Int64 getProcessDesignIdFromProcess_v2(Int64 processId, ConnectionManagerST obj)
		{
			Int64 ret = 0;
			string strSQLServer;
			strSQLServer = "SELECT PROCESS_DESIGN_ID FROM T_PROCESS WHERE PROCESS_ID =" + processId.ToString();
			var reader = obj.sqlServerDataReader(strSQLServer);
			reader.Read();
			ret = Convert.ToInt64(reader["PROCESS_DESIGN_ID"]);

			return ret;
		}

		private Int64 getProcessDesignDrawId_v2(Int64 processDesignId, ConnectionManagerST obj)
		{
			Int64 ret = 0;
			string strSQLServer;
			strSQLServer = "SELECT PROC_DESIGN_DRAW_ID FROM T_PROC_DESIGN_DRAW WHERE PROCESS_DESIGN_ID =" + processDesignId.ToString();
			var reader = obj.sqlServerDataReader(strSQLServer);
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

		public void FillProcesses_v2(Int64 processId, ConnectionManagerST obj)
		{
			fieldsForProcess_v2 = getProcessFields_v2(processId, obj);

			string strMsSQL = "";
			string strMSSqlChild;
			processes_v2.Add(new ProcessListItem { ProcessId = processId, Processed = false, ReasonType = ProcessReasonType.MainProcess });
			// -----  ProcessReasonType.SubProcess

			string msSqlCommandText1 = "SELECT * FROM T_ACTIVITY_FIELDS";
			var reader1 = obj.sqlServerDataReader(msSqlCommandText1);
			while (reader1.Read())
			{
				if ((fieldsForProcess_v2.FindIndex(a => a == Convert.ToInt64(reader1["Field_Id"].ToString())) > -1) &&
					  Convert.ToInt64(reader1["Field_Type"].ToString()) > 1)
				{
					if ((reader1["sub_process_id"] != null) && (reader1["sub_process_id"].ToString() != ""))
					{
						if (!IsProcessInList(Convert.ToInt64(reader1["sub_process_id"].ToString())))
						{
							processes_v2.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(reader1["sub_process_id"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.SubProcess
							});
						}
					}
					if ((reader1["parent_process_id"] != null) && (reader1["parent_process_id"].ToString() != ""))
					{
						if (!IsProcessInList(Convert.ToInt64(reader1["parent_process_id"].ToString())))
						{
							processes_v2.Add(new ProcessListItem
							{
								ProcessId = Convert.ToInt64(reader1["parent_process_id"].ToString()),
								Processed = false,
								ReasonType = ProcessReasonType.ParentProcess
							});
						}
					}
				}
			}

			// -----  ProcessReasonType.Report ----------------------------------------
			string msSqlCommandText2 = "SELECT * FROM T_REPORT_FIELD";
			var reader2 = obj.sqlServerDataReader(msSqlCommandText2);
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
			string msSqlCommandText3 = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
			var reader3 = obj.sqlServerDataReader(msSqlCommandText3);

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
			string msSqlCommandText4 = "SELECT * FROM T_REPORT_REFERENCED_FIELD_LOCATION";
			var reader4 = obj.sqlServerDataReader(msSqlCommandText4);
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
			string msSqlCommandText5 = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
			var reader5 = obj.sqlServerDataReader(msSqlCommandText5);
			strMsSQL = "SELECT * FROM T_ACTIVITY WHERE process_id = " + processId.ToString();
			while (reader5.Read())
			{
				strMSSqlChild = "SELECT T_PROCESS.Process_Alias_Id, T_PROCESS.Process_ID, T_AUTOMATIC_PROCESS.Activity_ID,T_PROCESS.Version_Status" +
						   " FROM  T_PROCESS INNER JOIN T_AUTOMATIC_PROCESS ON T_PROCESS.Process_Alias_Id = T_AUTOMATIC_PROCESS.Process_Alias_ID_To_Start " +
									  "WHERE Version_Status = 2 AND Activity_Id = " + Convert.ToInt64(reader5["activity_id"].ToString()).ToString();


				var reader6 = obj.sqlServerDataReader(strMSSqlChild);
				while (reader6.Read())
				{
					if (!IsProcessInList(Convert.ToInt64(reader6["process_id"].ToString())))
					{
						processes_v2.Add(new ProcessListItem
						{
							ProcessId = Convert.ToInt64(reader6["process_id"].ToString()),
							Processed = false,
							ReasonType = ProcessReasonType.AutomaticProcess
						});

					}
				}
			}


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

		public ServiceCallResult createDatabaseAndTables_v2(int processId, ConnectionManagerST obj)
		{

			string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root");
			if (!Directory.Exists(ConfigurationManager.AppSettings.Get("sqlite_databases_root")))
			{
				Directory.CreateDirectory(ConfigurationManager.AppSettings.Get("sqlite_databases_root"));
			}

			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };

			string strSQL = "SELECT Process_Id FROM T_PROCESS WHERE Process_Id = @param";
			string processName = "";
			var reader = obj.sqlServerDataReaderWithSingleParameter(strSQL, processId.ToString());
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

	}
}