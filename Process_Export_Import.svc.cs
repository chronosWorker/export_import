using System;
using System.Web;
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
using Newtonsoft.Json;

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

			private string result;

			public static implicit operator ServiceCallResult(string v)
			{
				throw new NotImplementedException();
			}
		}
		
		[OperationContract]
		public ServiceCallResult Export_Process(int processId)
		{

			var connectionManager = new ConnectionManagerST();
			string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + processId.ToString() + ".db; Version=3;";
			connectionManager.openSqlServerConnection();
			connectionManager.openOldSqlServerConnection();
			var ExportManager = new Export();
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			res = ExportManager.getSqlitePath_v2(processId, connectionManager);
			res = ExportManager.createDatabaseAndTables_v2(processId, connectionManager);
			connectionManager.openSqLiteConnection(sqliteSource);
		//	connectionManager.setPasswordOnDbFile();
			ExportManager.addTablesAndInfos(connectionManager);
			bool processIdExistInLocalDb = ExportManager.CheckIfProcessExistInDatabase_v2(processId , connectionManager);
			if (res.Code != 0)
			{
				return res;
			}
			try
			{
				ExportManager.processes_v2 = new List<ProcessListItem>();
				ExportManager.FillProcesses_v2(processId, connectionManager);
				ExportManager.fieldsForProcess_v2 = new List<long>();

				if (ExportManager.processes_v2.Count > 0)
				{
					for (int i = 0; i < ExportManager.processes_v2.Count; i++)
					{
						res = ExportManager.TransferProcess_v2(ExportManager.processes_v2[i].ProcessId , connectionManager);
					}
				}

				if (processIdExistInLocalDb)
				{
					
					res.Description = "notification ID: " + ExportManager.notification_id.Count.ToString() ;

				}
			
				string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + processId.ToString() + ".db";
				if (File.Exists(fileName))
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}
				//
				List<int> subporcessIds = ExportManager.checkIfSubProcessExist(connectionManager, processId);
				if (subporcessIds.Count >= 1)
				{
					foreach (int subProcessId in subporcessIds)
					{
						Export_Process_For_SubProcesses(subProcessId , connectionManager);
					}
				}

			}
			catch (Exception e)
			{
				res = ExportManager.FillServiceCallResult_v2(e);

			}
			connectionManager.closeSqLiteConnection();
			connectionManager.closeSqlServerConnection();
			connectionManager.closeOldSqlServerConnection();
			return res;	
		}



		[OperationContract]
		public List<string> Check_Import(string fileName , int personId)
		{

			List<string> insertResultInfo = new List<string>();
			List<string> firstRoundTablesWithContent = new List<string>();
			var connectionManager = new ConnectionManagerST();
			TableManager tableInfo = new TableManager();
			List<string> listOfTablesWhereIdentityInsertNeeded = tableInfo.listOfTablesWhereIdentityInsertNeeded();
			List<string> secondRoundInsertTablesWithoutIdentityProprty = tableInfo.secondRoundInsertTablesWithoutIdentityProprty();
			ResponseJson response = new ResponseJson();
			//string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + fileName + "; Version=3;Password=!#zSnP+n%m!8k@(/;";
			string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + fileName + "; Version=3;";
			connectionManager.openSqLiteConnection(sqliteSource);
			connectionManager.openSqlServerConnection();
			//bool isTheDBStructuresAreTheSame = verifyThatDBStructuresAreTheSame(checkingDBStructureDifferences(connectionManager));
			if (true)
			{
				try
				{
					#region general
					FkManager general = new FkManager("T_PROCESS", "Process_ID");
					general.changeProcessName(connectionManager);
					general.changeProcessDesignName(connectionManager);
					general.updateProcessDesignDrawCreationDateToToday(connectionManager);
					general.setImportPersonToProcessOwner(connectionManager, personId);
					#endregion
					#region process
					//-----------PROCESS-------------------------------------------------------
					//-------------------------------------------------------------------------

					FkManager processId = new FkManager("T_PROCESS", "Process_ID");
					processId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager processAliasId = new FkManager("T_PROCESS", "Process_Alias_ID");
					processAliasId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager automaticProcessId = new FkManager("T_AUTOMATIC_PROCESS", "Automatic_Process_ID");
					automaticProcessId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager processDesignId = new FkManager("T_PROCESS_DESIGN", "Process_Design_ID");
					processDesignId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					#endregion
					#region routing
					//-----------ROUTING-------------------------------------------------------
					//-------------------------------------------------------------------------
					FkManager routingConditionId = new FkManager("T_ROUTING_CONDITION", "Routing_Condition_ID");
					routingConditionId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager routingConditionGroupId = new FkManager("T_ROUTING_CONDITION_GROUP", "Routing_Condition_Group_ID");
					routingConditionGroupId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager routingId = new FkManager("T_ROUTING", "Routing_ID");
					routingId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager routingDesignId = new FkManager("T_ROUTING_DESIGN", "Routing_Design_ID");
					routingDesignId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityDesigndId = new FkManager("T_ACTIVITY_DESIGN", "Activity_Design_ID");
					activityDesigndId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					// FkManager routingDesignFromActivityId = new FkManager("T_ROUTING_DESIGN", "From_Activity_Design_ID");
					//	routingDesignFromActivityId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					//FkManager routingDesignToActivityId = new FkManager("T_ROUTING_DESIGN", "To_Activity_Design_ID");
					//routingDesignToActivityId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					#endregion
					#region activityk
					//-----------ACTIVITY------------------------------------------------------
					//-------------------------------------------------------------------------

					FkManager activityBeforeFinishCheckQueryTypeIdId = new FkManager("T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE", "Activity_Before_Finish_Check_Query_Type_ID");
					activityBeforeFinishCheckQueryTypeIdId.changeAllIdInDbFileToFitSqlServer(connectionManager);

			//		FkManager activityFinishStepModedId = new FkManager("T_ACTIVITY_FINISH_STEP_MODE", "Activity_Finish_Step_Mode_ID");
			//		activityFinishStepModedId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityUiComponentdId = new FkManager("T_ACTIVITY_UI_COMPONENT", "Activity_UI_Component_ID");
					activityUiComponentdId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityOwnerByConditionId = new FkManager("T_ACTIVITY_OWNER_BY_CONDITION", "Activity_Owner_By_Condition_ID");
					activityOwnerByConditionId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityParticipantTypeId = new FkManager("T_ACTIVITY_PARTICIPANT_TYPE", "Activity_Participant_Type_ID");
					activityParticipantTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityFieldUIParametersId = new FkManager("T_ACTIVITY_FIELDS_UI_PARAMETERS", "Activity_Fields_UI_Paramaters_ID");
					activityFieldUIParametersId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					#endregion
					#region fields
					//-----------FIELD---------------------------------------------------------
					//-------------------------------------------------------------------------
							

							   FkManager fieldTypeId = new FkManager("T_FIELD_TYPE", "Field_Type_ID");
							   fieldTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fileFieldTypeId = new FkManager("T_FILE_FIELD_TYPE", "File_Field_Type_ID");
							   fileFieldTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldTextFormatTypeId = new FkManager("T_FIELD_TEXT_FORMAT_TYPE", "Field_Text_Format_Type_ID");
							   fieldTextFormatTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupDependencyId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY", "Field_Group_To_Field_Group_Dependency_ID");
							   fieldGroupToFieldGroupDependencyId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupDependencyTypeId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE", "Field_Group_To_Field_Group_Dependency_Type_ID");
							   fieldGroupToFieldGroupDependencyTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupDependencyModeId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE", "Field_Group_To_Field_Group_Dependency_Mode_ID");
							   fieldGroupToFieldGroupDependencyModeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupDependencyActivationActivityId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY", "Field_Group_To_Field_Group_Dependency_Activation_Activity_ID");
							   fieldGroupToFieldGroupDependencyActivationActivityId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupDepCondFormula = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA", "Field_Group_To_Field_Group_Dependency_Condition_Formula_ID");
							   fieldGroupToFieldGroupDepCondFormula.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupConditionOperatorId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR", "Field_Group_To_Field_Group_Condition_Operator_ID");
							   fieldGroupToFieldGroupConditionOperatorId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldConditionId = new FkManager("T_FIELD_CONDITION", "Field_Condition_ID");
							   fieldConditionId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldConditionGroupId = new FkManager("T_FIELD_CONDITION_GROUP", "Field_Condition_Group_ID");
							   fieldConditionGroupId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldDateConstraitId = new FkManager("T_FIELD_DATE_CONSTRAINT", "Field_Date_Constraint_ID");
							   fieldDateConstraitId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldDateTypeId = new FkManager("T_FIELD_DATE_TYPE", "Date_Field_Type_ID");
							   fieldDateTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager calcFieldConstantTypeId = new FkManager("T_CALCULATED_FIELD_CONSTANT_TYPE", "Calculated_Field_Constant_Type_ID");
							   calcFieldConstantTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager calcFieldResultTypeId = new FkManager("T_CALCULATED_FIELD_RESULT_TYPE_ID", "Calculated_Field_Result_Type_ID");
							   calcFieldResultTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldGroupToFieldGroupDependentFieldsId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS", "Field_Group_To_Field_Group_Dependent_Fields_ID");
							   fieldGroupToFieldGroupDependentFieldsId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldToFieldDependecyId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY", "Field_To_Field_Dependency_ID");
							   fieldToFieldDependecyId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						//	   FkManager dependentFieldId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY", "Dependent_Field_ID");
						//	   dependentFieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager compareOperationId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY", "Compare_Operation_Id");
							   compareOperationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager dependencyActivationId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY", "Dependency_Activation_Activity_ID");
							   dependencyActivationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   

								FkManager fieldToFieldDependecyTypeId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY_TYPE", "Field_To_Field_Dependency_Type_ID");
								fieldToFieldDependecyTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldValueId = new FkManager("T_FIELD_VALUE", "Field_Value_ID");
							   fieldValueId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldValueTranslationId = new FkManager("T_FIELD_VALUE_TRANSLATION", "Field_Value_Translation_ID");
							   fieldValueTranslationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldLabelTranslationId = new FkManager("T_FIELD_LABEL_TRANSLATION", "Field_Label_Translation_ID");
							   fieldLabelTranslationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

							   FkManager fieldDocumentReferenceImportTypeId = new FkManager("T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE", "Field_Document_Reference_Import_Ttype_ID");
							   fieldDocumentReferenceImportTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					#endregion
					#region reports
					FkManager activityId = new FkManager("T_ACTIVITY", "Activity_ID");
					activityId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					//-----------REPORTOK------------------------------------------------------
					//-------------------------------------------------------------------------
							FkManager reportId = new FkManager("T_REPORT", "Report_ID");
						   reportId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportFieldCondGroupId = new FkManager("T_REPORT_2_FIELD_COND_GROUP", "Report_2_Field_Cond_Group_ID");
						   reportFieldCondGroupId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportCalcFieldFormulaTreeNodeId = new FkManager("T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE", "Report_Calculated_Field_Formula_Tree_Node_ID");
						   reportCalcFieldFormulaTreeNodeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportCalcFieldFormulaTreeNodeValueId = new FkManager("T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE", "Report_Calculated_Field_Formula_Tree_Node_ID");
						   reportCalcFieldFormulaTreeNodeValueId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportFieldId = new FkManager("T_REPORT_FIELD", "Report_Field_ID");
						   reportFieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportFieldUDTColumnId = new FkManager("T_REPORT_FIELD_UDT_COLUMNS", "Report_Field_UDT_COLUMNS_ID");
						   reportFieldUDTColumnId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportGroupId = new FkManager("T_REPORT_GROUP", "Report_Group_ID");
						   reportGroupId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportGroupAdministratorId = new FkManager("T_REPORT_GROUP_ADMINISTRATOR", "Report_Group_Administrator_ID");
						   reportGroupAdministratorId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportRefFieldLocationId = new FkManager("T_REPORT_REFERENCED_FIELD_LOCATION", "Report_Referenced_Field_Location_ID");
						   reportRefFieldLocationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

						   FkManager reportTypenId = new FkManager("T_REPORT_TYPE", "Report_Type_ID");
						   reportTypenId.changeAllIdInDbFileToFitSqlServer(connectionManager);
		   #endregion
					#region rajz
					//-----------DRAW--------------------------------------------------------------
					//-----------------------------------------------------------------------------
					FkManager procDesignDrawId = new FkManager("T_PROC_DESIGN_DRAW", "Proc_Design_Draw_ID");
					procDesignDrawId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager procDesignDrawPartId = new FkManager("T_PROC_DESIGN_DRAW_PART", "Proc_Design_Draw_Part_ID");
					procDesignDrawPartId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager procDesignDrawPartDetailId = new FkManager("T_PROC_DESIGN_DRAW_PART_DETAIL", "Proc_Design_Draw_Part_Detail_ID");
					procDesignDrawPartDetailId.changeAllIdInDbFileToFitSqlServer(connectionManager);

			//	   FkManager procDesignDrawPartTypeId = new FkManager("T_PROC_DESIGN_DRAW_PART_TYPE", "Proc_Design_Draw_Part_Type_ID");
			//	   procDesignDrawPartTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager procWordMegeFieldId = new FkManager("T_PROCFIELD_WORD_MERGE_FIELD" , "Procfield_Word_Merge_Field_ID");
					procWordMegeFieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);
		#endregion
					#region TypeTables
					//-----------TYPE TABLES--------------------------------------------------------
					//-----------------------------------------------------------------------------
				
					FkManager categoryType = new FkManager("T_CATEGORY", "NAME");
					categoryType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager notificationType = new FkManager("T_NOTIFICATION_TYPE", "NOTIFICATION_TYPE");
					notificationType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager notificationRecipientType = new FkManager(" T_NOTIFICATION_RECIPIENT_TYPE", "Name");
					notificationRecipientType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager reportType = new FkManager("T_REPORT_TYPE", "Name");
					reportType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager activityBeforeFinishCheckQueryType = new FkManager("T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE", "Name");
					activityBeforeFinishCheckQueryType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager activityParticipantType = new FkManager("T_ACTIVITY_PARTICIPANT_TYPE", "Name");
					activityParticipantType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager calcFieldConstantType = new FkManager("T_CALCULATED_FIELD_CONSTANT_TYPE", "Name");
					calcFieldConstantType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager calcFieldResultType = new FkManager("T_CALCULATED_FIELD_RESULT_TYPE_ID", "Name");
					calcFieldResultType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager chartFieldType = new FkManager("T_CHART_FIELD_TYPE", "Name");
					chartFieldType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager chartType = new FkManager("T_CHART_TYPE", "Name");
					chartType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager compareType = new FkManager("T_COMPARE_OPERATION", "Name");
					compareType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldDateType = new FkManager("T_FIELD_DATE_TYPE", "Date_Field_Type_ID");
					fieldDateType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldDocReferenceImportType = new FkManager("T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE", "Name");
					fieldDocReferenceImportType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldGroupToFieldGroupDepType = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE", "Field_Group_To_Field_Group_Dependency_Type_Name");
					fieldGroupToFieldGroupDepType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fileFieldType = new FkManager("T_FILE_FIELD_TYPE", "Name");
					fileFieldType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager procDesignDrawPartType = new FkManager("T_PROC_DESIGN_DRAW_PART_TYPE", "Name");
					procDesignDrawPartType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);
					#endregion
					#region egyébTáblák
						//-----------ALL OTHER---------------------------------------------------------
						//-----------------------------------------------------------------------------

					FkManager roleId = new FkManager("T_ROLE", "Role_ID");
					roleId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager userDefinedTableId = new FkManager("T_USER_DEFINED_TABLE", "USER_DEFINED_TABLE_ID");
					userDefinedTableId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager chartTypeId = new FkManager("T_CHART_TYPE", "Chart_Type_ID");
					chartTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager chartTypeFiekldId = new FkManager("T_CHART_FIELD_TYPE", "Chart_Type_Field_ID");
					chartTypeFiekldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					//FkManager compareOperationId = new FkManager("T_COMPARE_OPERATION", "Compare_Operation_ID");
					//compareOperationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager notificationId = new FkManager("T_NOTIFICATION", "Notification_ID");
					notificationId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager formulaStepId = new FkManager("T_FORMULA_STEPS", "FORMULA_STEPS_ID");
					formulaStepId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager operandId = new FkManager("T_OPERAND", "OPERAND_ID");
					operandId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager notificationTriggerId = new FkManager("T_NOTIFICATION_TRIGGER", "Notification_Trigger_ID");
					notificationTriggerId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager fieldId = new FkManager("T_FIELD", "Field_ID");
					fieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					#endregion
					#region generateResponseObject
					response.ActivityParticipants = general.detectActivityParticipants(connectionManager);
					if (general.detectNotificationEmailAddress(connectionManager))
					{
						response.EmailAddressFound = true;
						response.NotificationAddresses = general.detectNotificationAddresses(connectionManager);
						general.deleteNotificationAddresses(connectionManager);
					}
						
					var result = JsonConvert.SerializeObject(response);
					insertResultInfo.Add(result);
					#endregion
				}
				catch (Exception ex)
				{
					insertResultInfo.Add(ex.Message.ToString() + ex.StackTrace.ToString());
				}
			}
			connectionManager.closeSqLiteConnection();
			connectionManager.closeSqlServerConnection();

			return insertResultInfo;

		}

		[OperationContract]
		public List<string> ImportData(string fileName , string emailAddress, List<KeyValuePair<string, int>> activityParticipants )
		{
			var connectionManager = new ConnectionManagerST();
			string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + fileName + "; Version=3;";
			connectionManager.openSqLiteConnection(sqliteSource);
			connectionManager.openSqlServerConnection();
			TableManager tableInfo = new TableManager();
			List<string> listOfTablesWhereIdentityInsertNeeded = tableInfo.listOfTablesWhereIdentityInsertNeeded();
			List<string> secondRoundInsertTablesWithoutIdentityProprty = tableInfo.secondRoundInsertTablesWithoutIdentityProprty();
			List<string> insertresultInfo = new List<string>();
			try
			{
			
				foreach (string tableName in tableInfo.getFirstRoundInsertTables())
				{

					if (tableName != "T_DB_CONNECTION" && tableName != "T_CATEGORY")
					{

						if (!(tableInfo.tableInDBFileWithoutRow(connectionManager, tableName)))
						{

							if (listOfTablesWhereIdentityInsertNeeded.Contains(tableName))
							{

								insertValuesFromDbFileToSqlServer(tableName, true, connectionManager);
							}
							else
							{

								insertValuesFromDbFileToSqlServer(tableName, false, connectionManager);

							}

						}
					}

				}

				foreach (string tableName in tableInfo.getSecondRoundInsertTables())
				{

					if (tableName != "T_DB_CONNECTION" && tableName != "T_DEPARTMENT" && tableName != "T_LANGUAGE" && tableName != "T_CATEGORY" && tableName != "T_PROCFIELD_WORD_MERGE" && tableName != "T__OPERATION")
					{

						if (!(tableInfo.tableInDBFileWithoutRow(connectionManager, tableName)))
						{
							if (secondRoundInsertTablesWithoutIdentityProprty.Contains(tableName))
							{

								insertValuesFromDbFileToSqlServer(tableName, false, connectionManager);

							}
							else
							{

								insertValuesFromDbFileToSqlServer(tableName, true, connectionManager);
							}

						}
					}

				}
			}
			catch (Exception ex)
			{
				insertresultInfo.Add(ex.Message.ToString() + ex.StackTrace.ToString());
			}

			return insertresultInfo;

		}

		public List<string> insertValuesFromDbFileToSqlServer(string tableName, bool needToSetIdentityInsertOn, ConnectionManagerST obj)
		{
			List<string> insertresultInfo = new List<string>();
			List<string> values = new List<string>();
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			string commandText = "INSERT INTO " + tableName + "  ( ";
			try
			{
				columnTypes = getColumnTypesDictionary_v3(tableName, obj);

				var reader = obj.sqLiteDataReader("SELECT * FROM " + tableName);
				int fieldCount = reader.FieldCount;
				//insertresultInfo.Add("Field Count :" + fieldCount);
				//insertresultInfo.Add("columnTypes.Count :" + columnTypes.Count);


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

				}

				commandText += ") Values ";
				while (reader.Read())
				{
					if (reader.GetValue(0).ToString() != "NULL" || reader.GetValue(0).ToString() != "" || reader.GetValue(0).GetType() != typeof(DBNull))
					{
						commandText += "( ";

						for (var index = 0; index < columnTypes.Count; index++)
						{
							string currentRecord = reader[columnTypes.ElementAt(index).Key.ToString()].ToString();
							string currentRecordSingleQuoteFormalized = currentRecord.Replace("'", "''");
						  
							switch (columnTypes.ElementAt(index).Value)
							{

								case "bit":
								case "binary":
								case "varbinary":
								case "image":
								case "DateTime":
								case "nvarchar":
								case "varchar":
								case "datetime":
									if (currentRecord.Contains("'"))
									{

										commandText += "'" + currentRecordSingleQuoteFormalized + "'";

									}
									else
									{
										commandText += "'" + currentRecord + "'";

									}
									break;
								default:
									if (currentRecord.Contains("'"))
									{

										commandText += (currentRecordSingleQuoteFormalized.GetType() == typeof(DBNull) || currentRecordSingleQuoteFormalized == "") ? "NULL" :
										currentRecordSingleQuoteFormalized;

									}
									else
									{
										commandText += (currentRecord.GetType() == typeof(DBNull) || currentRecord == "") ? "NULL" :
										currentRecord;

									}
									break;
							}
							if (index < columnTypes.Count - 1)
							{

								commandText += ",";
							}
						}

						commandText += ") ,";

					}
					else
					{
						insertresultInfo.Add(tableName + " has 0 rows");
					}
				}
				commandText = commandText.Substring(0, commandText.Length - 1);
				commandText += ";";
				insertresultInfo.Add("commandText: " + commandText);

				if (needToSetIdentityInsertOn)
				{
					obj.executeQueriesInSqlServer("SET IDENTITY_INSERT " + tableName + " ON ; " + commandText + " ; SET IDENTITY_INSERT " + tableName + " OFF ;");
				}
				else
				{
					obj.executeQueriesInSqlServer(commandText);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return insertresultInfo;
		}
	
		#region table_infos_+_smaller_help_functions

		private Dictionary<string, string> getColumnTypesDictionary_v3(string CWPTableName, ConnectionManagerST obj)
		{
			Dictionary<string, string> fields = new Dictionary<string, string>();
			string commandText = "SELECT * FROM table_information WHERE TABLE_NAME =  '" + CWPTableName + "'";
			var reader = obj.sqLiteDataReader(commandText);
			while (reader.Read())
			{
				fields.Add(reader["COLUMN_NAME"].ToString(), reader["DATA_TYPE"].ToString());
			}
			return fields;

		}
		public ServiceCallResult Export_Process_For_SubProcesses(int processId , ConnectionManagerST connectionManager) { 

			string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + processId.ToString() + ".db; Version=3;";
			var ExportManager = new Export();
		   
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			res = ExportManager.getSqlitePath_v2(processId, connectionManager);
			res = ExportManager.createDatabaseAndTables_v2(processId, connectionManager);
			connectionManager.openSqLiteConnection(sqliteSource);
		//	connectionManager.setPasswordOnDbFile();
			ExportManager.addTablesAndInfos(connectionManager);
			bool processIdExistInLocalDb = ExportManager.CheckIfProcessExistInDatabase_v2(processId, connectionManager);
			if (res.Code != 0)
			{
				return res;
			}
			try
			{
				ExportManager.processes_v2 = new List<ProcessListItem>();
				ExportManager.FillProcesses_v2(processId, connectionManager);
				ExportManager.fieldsForProcess_v2 = new List<long>();

				if (ExportManager.processes_v2.Count > 0)
				{
					for (int i = 0; i<ExportManager.processes_v2.Count; i++)
					{
						res = ExportManager.TransferProcess_v2(ExportManager.processes_v2[i].ProcessId , connectionManager);
					}
				}

				if (processIdExistInLocalDb)
				{
					
					res.Description = "notification ID: " + ExportManager.notification_id.Count.ToString() ;

				}
			}
			catch (Exception e)
			{
					res = ExportManager.FillServiceCallResult_v2(e);

			}
			
			string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + processId.ToString() + ".db";
			if (File.Exists(fileName))
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			return res;	
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


/*
  T_SUBPROCESS
  T_ACTIVITY_OWNER_BY_COND_PARTICIPANT
  T_ACTIVITY_OWNER_BY_CONDITION
  T_ACTIVITY_OWNER_BY_CONDITION_CONDITION
  T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP
  T_ACTIVITY_PARTICIPANT
  T_ACTIVITY_UI_COMPONENT
*/

/*  EZEK KIMARADTAK, KELLENEK !!!!
*  
*   documentum referenciált processek
*   subprocessek
*   a report is hivatkozhat másik processre  ** -- OK beleírva
*   
  T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY
  T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS
  T_FIELD_VALUE_TRANSLATION
*/


#endregion


	}
}
