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

			[DataMember]
			public string ResultString { get; set; }

			[DataMember]
			public int ResultInt { get; set; }

			[DataMember]
			public List<string> subProcessIds { get; set; }

			public static implicit operator ServiceCallResult(string v)
			{
				throw new NotImplementedException();
			}
		}
		
		[OperationContract]
		public ServiceCallResult Export_Process(int processId)
		{
			List<string> exportInfo = new List<string>();
			var connectionManager = new ConnectionManagerST();
			var ExportManager = new Export();
			var General_Info = new General_Info();
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + processId.ToString() + ".db; Version=3;";
			connectionManager.openSqlServerConnection();
			connectionManager.openOldSqlServerConnection();

			res = ExportManager.getSqlitePath_v2(processId, connectionManager);
			if (res.Code != 0)
			{
				return res;
			}
			res = ExportManager.createDatabaseAndTables_v2(processId, connectionManager);
			connectionManager.openSqLiteConnection(sqliteSource);
		//	connectionManager.setPasswordOnDbFile();
			ExportManager.addTablesAndInfos(connectionManager);
			if (res.Code != 0)
			{
				return res;
			}
			try
			{
			
				string fileName = ConfigurationManager.AppSettings.Get("sqlite_databases_root") + processId.ToString() + ".db";

				if (File.Exists(fileName))
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}

				res = ExportSubProcesses(connectionManager, processId);

				if (res.Code != 0)
				{
					return res;
				}
			}
			catch (Exception e)
			{
				res = ExportManager.FillServiceCallResult_v2(e);
				exportInfo.Add(e.ToString());


			}

			connectionManager.closeSqLiteConnection();
			connectionManager.closeSqlServerConnection();
			connectionManager.closeOldSqlServerConnection();

			return res;	
		}
		public ServiceCallResult ExportSubProcesses(ConnectionManagerST connectionManager, int processId )
		{
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			res.Description += processId.ToString();
			Export export = new Export();
			List<int> subporcessIds = export.checkIfSubProcessExist(connectionManager, processId);
			bool docRefExits = export.checkIfThereIsDocRefInFieldsForProcess(connectionManager, processId);
			if (docRefExits)
			{
				List<int> docRefProcessIdList = export.docRefProcessIdList(connectionManager, processId);
				subporcessIds.AddRange(docRefProcessIdList);
			}
			export.processes_v2 = new List<ProcessListItem>();
			export.FillProcesses_v2(processId, connectionManager);
			export.fieldsForProcess_v2 = new List<long>();
			subporcessIds.ForEach(e => res.ResultString += " id : " + e.ToString() + " ;");
			res = export.TransferProcess_v2(processId, connectionManager);
			if (subporcessIds.Count == 0 || res.Code != 0 )
			{
				return res;
			}
			else
			{
				res = export.updateMainProcessIdForSubprocesses(connectionManager, subporcessIds, processId);
				foreach (int subProcessId in subporcessIds)
				{
					res = ExportSubProcesses(connectionManager, subProcessId);
					if (res.Code != 0)
					{
						throw new Exception();
					}

				}
			}

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
		//	string sqliteSource = @"Data Source=C:\inetpub\wwwroot\csf_test_site\temp\" + fileName + "; Version=3;Password=!#zSnP+n%m!8k@(/;";
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
					general.updateNullableEmptyFieldsToNull(connectionManager);
					general.deleteMultipleOccurenceIdValueFromTable(connectionManager , tableInfo);
					

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

					FkManager subProcessId = new FkManager("T_SUBPROCESS", "Subprocess_ID");
					subProcessId.changeAllIdInDbFileToFitSqlServer(connectionManager);
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

					FkManager activityFinishStepModedId = new FkManager("T_ACTIVITY_FINISH_STEP_MODE", "Activity_Finish_Step_Mode_ID");
					activityFinishStepModedId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityUiComponentdId = new FkManager("T_ACTIVITY_UI_COMPONENT", "Activity_UI_Component_ID");
					activityUiComponentdId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityOwnerByConditionId = new FkManager("T_ACTIVITY_OWNER_BY_CONDITION", "Activity_Owner_By_Condition_ID");
					activityOwnerByConditionId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityParticipantTypeId = new FkManager("T_ACTIVITY_PARTICIPANT_TYPE", "Activity_Participant_Type_ID");
					activityParticipantTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityFieldUIParametersId = new FkManager("T_ACTIVITY_FIELDS_UI_PARAMETERS", "Activity_Fields_UI_Paramaters_ID");
					activityFieldUIParametersId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityOwnerByCondParticipantId = new FkManager("T_ACTIVITY_OWNER_BY_COND_PARTICIPANT", "Act_Owner_By_Cond_Participant_ID");
					activityOwnerByCondParticipantId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager activityBeforeEscalationNotificationId = new FkManager("T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION", "Activity_Before_Escalation_Notification_ID");
					activityBeforeEscalationNotificationId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					#endregion
					#region fields
					//-----------FIELD---------------------------------------------------------
					//-------------------------------------------------------------------------			

					FkManager fieldTypeId = new FkManager("T_FIELD_TYPE", "Field_Type_ID");
					fieldTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);
							   
					FkManager fileFieldTypeId = new FkManager("T_FILE_FIELD_TYPE", "File_Field_Type_ID");
					fileFieldTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager filedExtensionNumberId = new FkManager("T_FIELD_EXTENSION_NUMBER" , "Field_Extension_Number_ID");
					filedExtensionNumberId.changeAllIdInDbFileToFitSqlServer(connectionManager);

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

				//	FkManager fieldDateTypeId = new FkManager("T_FIELD_DATE_TYPE", "Date_Field_Type_ID");
				//	fieldDateTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager calcFieldConstantTypeId = new FkManager("T_CALCULATED_FIELD_CONSTANT_TYPE", "Calculated_Field_Constant_Type_ID");
					calcFieldConstantTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager calcFieldResultTypeId = new FkManager("T_CALCULATED_FIELD_RESULT_TYPE_ID", "Calculated_Field_Result_Type_ID");
					calcFieldResultTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager fieldGroupToFieldGroupDependentFieldsId = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS", "Field_Group_To_Field_Group_Dependent_Fields_ID");
					fieldGroupToFieldGroupDependentFieldsId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager fieldToFieldDependecyId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY", "Field_To_Field_Dependency_ID");
					fieldToFieldDependecyId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager dependentFieldId = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY", "Dependent_Field_ID");
					dependentFieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

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

					FkManager procDesignDrawPartTypeId = new FkManager("T_PROC_DESIGN_DRAW_PART_TYPE", "Proc_Design_Draw_Part_Type_ID");
					procDesignDrawPartTypeId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager procWordMergedId = new FkManager("T_PROCFIELD_WORD_MERGE" , "Procfield_Word_Merge_ID");
					procWordMergedId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager procWordMergeFieldId = new FkManager("T_PROCFIELD_WORD_MERGE_FIELD" , "Procfield_Word_Merge_Field_ID");
					procWordMergeFieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					#endregion
					#region TypeTables
					//-----------TYPE TABLES--------------------------------------------------------
					//-----------------------------------------------------------------------------
					FkManager fieldGroupToFieldGroupDepModeType = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE", "Field_Group_To_Field_Group_Dependency_Mode_Name");
					fieldGroupToFieldGroupDepModeType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldTypeType =  new FkManager("T_FIELD_TYPE" , "Name");
					fieldTypeType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldTextFormatType = new FkManager("T_FIELD_TEXT_FORMAT_TYPE", "Name");
					fieldTextFormatType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

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

					FkManager fieldDateType = new FkManager("T_FIELD_DATE_TYPE", "Date_Field_Type_Value");
					fieldDateType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldDocReferenceImportType = new FkManager("T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE", "Name");
					fieldDocReferenceImportType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldGroupToFieldGroupDepType = new FkManager("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE", "Field_Group_To_Field_Group_Dependency_Type_Name");
					fieldGroupToFieldGroupDepType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fileFieldType = new FkManager("T_FILE_FIELD_TYPE", "Name");
					fileFieldType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager procDesignDrawPartType = new FkManager("T_PROC_DESIGN_DRAW_PART_TYPE", "Name");
					procDesignDrawPartType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldToFieldDependencyType = new FkManager("T_FIELD_TO_FIELD_DEPENDENCY_TYPE", "Field_To_Field_Dependency_Name");
					fieldToFieldDependencyType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager systemInterfaceType = new FkManager("T_SYSTEM_INTERFACE_TYPE", "Description");
					systemInterfaceType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);

					FkManager fieldConditionGroupType = new FkManager("T_FIELD_CONDITION_GROUP", "Name");
					fieldConditionGroupType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);


					FkManager roleType = new FkManager("T_ROLE", "Name");
					roleType.deleteUnnecessaryRecordsFromTypeTables(connectionManager);
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

					FkManager systemInterfaceId = new FkManager("T_SYSTEM_INTERFACE", "System_Interface_ID");
					systemInterfaceId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					FkManager systemInterfaceTriggerId = new FkManager("T_SYSTEM_INTERFACE_TRIGGER", "System_Interface_Trigger_ID");
					systemInterfaceTriggerId.changeAllIdInDbFileToFitSqlServer(connectionManager);
					
					FkManager fieldId = new FkManager("T_FIELD", "Field_ID");
				   fieldId.changeAllIdInDbFileToFitSqlServer(connectionManager);

					#endregion
					#region generateResponseObject
					int mainProcessId = general.getMainProcessId(connectionManager);
					insertResultInfo.Add("Main ID  " + mainProcessId.ToString());
				//	general.updateMainProcessIdForSubprocesses(connectionManager, mainProcessId);
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

					List<int> FieldIdsForDocRefList = new List<int>();
					List<KeyValuePair<int, int>> FieldValueAndIdList = new List<KeyValuePair<int, int>>();
					FieldIdsForDocRefList = general.selectdFieldIdsForDocRefList(connectionManager);
					FieldValueAndIdList = general.selectFieldValueAndFieldId(connectionManager, FieldIdsForDocRefList);
					general.updateDocRefValues(connectionManager, FieldValueAndIdList);


               
                 //   string insertText2 = " SET IDENTITY_INSERT T_PROCFIELD_WORD_MERGE ON; insert into T_PROCFIELD_WORD_MERGE(Procfield_Word_Merge_ID,Field_ID,Content,At_Start,File_Name,Convert_Final_Merged_File_To_PDF,Electronically_Sign_When_Mandatory_In_Activity)Values(405,72241,0x504B03041400060008000000210009248782810100008E050000130008025B436F6E74656E745F54797065735D2E786D6C20A2040228A000020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000B4944D4F83401086EF26FE07B25703DB7A30C694F6A0F5A84DACF1BC2E43D9C87E6467FBF5EF1D4A4BAAA1A55ABD90C032EFFBCC0B3383D14A97D1023C2A6B52D64F7A2C02236DA6CC2C65AFD3C7F896451884C944690DA46C0DC846C3CB8BC174ED0023AA3698B2220477C739CA02B4C0C43A3074925BAF45A05B3FE34EC80F31037EDDEBDD70694D0013E25069B0E1E00172312F43345ED1E39AC443892CBAAF5FACBC52269C2B95148148F9C264DF5CE2AD4342959B77B0500EAF0883F15687EAE4B0C1B6EE99A2F12A8368227C78129A30F8D2FA8C6756CE35F5901C9769E1B479AE2434F5959AF356022265AECBA439D142991DFF410E0CEB12F0EF296ADD13EDDF5428C6790E923E76771E1AE3AAE9A4B6D8ABED76831028A4534CBEFE827157E8B855EE4458C2FBCBBF51EC897782E4341A53F15EC20989FF308C46BA1322D0BC03DF5CFB67736C648E59D2644CBC7548FBC3FFA2EDDD82A8AA631A39073E28685644DB88358EB47BCEEE0FAAED9641D6E2CD37DB74F8090000FFFF0300504B0304140006000800000021001E911AB7F30000004E0200000B0008025F72656C732F2E72656C7320A2040228A0000200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000008C92DB4A03410C86EF05DF61C87D37DB0A22D2D9DE48A17722EB038499EC01770ECCA4DABEBDA320BA50DB5EE6F4E7CB4FD69B839BD43BA73C06AF6159D5A0D89B6047DF6B786DB78B075059C85B9A82670D47CEB0696E6FD62F3C9194A13C8C31ABA2E2B38641243E226633B0A35C85C8BE54BA901C4909538F91CC1BF58CABBABEC7F457039A99A6DA590D6967EF40B5C758365FD60E5D371A7E0A66EFD8CB8915C807616FD92E622A6C49C6728D6A29F52C1A6C30CF259D9162AC0A36E069A2D5F544FF5F8B8E852C09A10989CFF37C759C035A5E0F74D9A279C7AF3B1F21592C167D7BFB4383B32F683E010000FFFF0300504B0304140006000800000021007C3B973922010000B90300001C000801776F72642F5F72656C732F646F63756D656E742E786D6C2E72656C7320A2040128A0000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000AC934D4F84301086EF26FE07D2BB14565D8DD9B21735D9ABAEF1DC2D5368849674C60FFEBD15B30ACAA2072E4D669ABECFD349BB5ABFD555F4021E8DB382A571C222B0CAE5C616823D6C6F4F2E5984246D2E2B6741B01690ADB3E3A3D51D5492C2212C4D835148B1285849D45C718EAA845A62EC1AB061473B5F4B0AA52F7823D5932C802F9264C97D3F836583CC68930BE637F9298BB66D13C87F673BAD8D826BA79E6BB03482E00844E1661832A52F8004DB77E2E0C9F8B8C2E280426D9477E834C5CAD5FC93FE41BD185E8C23B515E0A3A1F2466B50D4C7FFDC9AF2480F788C8CF91FA3E8C8BD4174F5147E39279EC20B816F7A57F26E4DA71CCEE774D0CED256EEAA9EC7576B4AE26C4E8957D8DDFF7A95BDE65E840F3E5CF60E0000FFFF0300504B03041400060008000000210010BC9D77E9020000DD09000011000000776F72642F646F63756D656E742E786D6CCC56DD6ED33014BE47E21DA25C22AD49F6D39568ED04B4AB2631A8D6718784DCC449AD263E96ED36746FC173F0087035C47B711C276969A7696B35C44D12FF9CEF7CE7B3FDC567E75FF3CC5950A918F0AE1BB47CD7A13C8298F1B4EB7EBAB938E8B88ED284C724034EBBEE922AF7BCF7F2C55911C610CD73CAB583105C858588BAEE546B117A9E8AA63427AA95B348828244B722C83D48121651AF00197B877EE0975F42424495C27CEF085F10E55670F9361A08CA31570232275AB540A65E4EE46C2E0E105D10CD262C637A89D87EBB8681AE3B973CAC081D34844C48680955AF3A426E55714F5E1BD9AF1428337A9266C801B89A32B12A6357342C715A535A3C54C422CFEA7985088EB7F235253F660DFA9214B8142BC02DB87BC4886D509E591DCCFAAE56751331F01F2AA65A1103D170780C85BF73D64C72C27803B39B34EBE2E289D8677F0F25CC454347B0FDD02EF9ACC13207F309CCFC7679F2D64B534F02D83ABAE32911D475F228BC4C394832C99051111C3B6647BA3D348B09C44BF3164E11A2D9C4D75DD7F7FB4781DF3975EBAE3E4DC83CD366E4A27D7872E29791D2849526132A41220416922A2A17D4ED7D20E952DDFEFA99FDFE163A671ECEEB992786E0536CE5AB501FCE9764F198E522A3388D71A5D10F9CABC1F57070713978DF771C9B739E81E37C7E558D7CBCBE7A73E3ACE8CA51C99AC34802249E2565FB74EFEE7B0371F7639374937DD71230D986CA8FAADAAADC1B57626ED27A262DC7B77BE95886FF1F1AEE27FB7049259D85FF48759B6DF7ED6BE39F4177B375270033F3571F6B22B5398231FA81B1084E723CFB5F86F0964433B7D4BB9E3BE07133D31EB672BF2A1AE991C491C670D66C45A4E35B1C2AF0D213BC36F785229CE277BB73D4B1E022BD22265883C0FEE3C0FA144BA768507573025A43BE6A6734591B9D521253B48F531F2F51459800E8B5663AD765D3B7E922C8144EAA2CCE849425E2256B2899292F639C8E988E90E551BB0C428FB325965E611D16FBEA7B59EF0F000000FFFF0300504B0304140006000800000021009940B5E1BD0600005C1A000015000000776F72642F7468656D652F7468656D65312E786D6CEC594D8B1B3718BE17FA1F86B93BFE9AF1C7126FB0C776D2663709B19392A3D6963DCA6A466624EFC68440494EBD140A69E9A181DE7A28A581061A7AE98F59D8D0A63FA2AF34E3B164CBD90FF6104A772F1ECDF3BE7AF4BED2F34AA3EB379E44D439C209272C6EB9E56B25D7C1F1888D493C6DB90F86FD42C375B840F1185116E396BBC0DCBDB1FBE927D7D18E0871841DB08FF90E6AB9A110B39D62918FA019F16B6C86637837614984043C26D3E23841C7E037A2C54AA9542B4688C4AE13A308DCDE9D4CC8083B43E9D2DD5D3AEF51788C05970D239A0CA46B6C5828ECF8B02C117CC1039A384788B65CE867CC8E87F889701D8AB880172DB7A4FEDCE2EEF522DAC98CA8D862ABD9F5D55F6697198C0F2BAACF647A9077EA79BE576BE7FE15808A4D5CAFDEABF56AB93F0540A3118C34E5A2FBF43BCD4ED7CFB01A28FD69F1DDAD77AB6503AFF9AF6E706EFBF2DFC02B50EADFDBC0F7FB0144D1C02B508AF737F09E57AF049E8157A0145FDBC0D74BEDAE5737F00A1452121F6EA04B7EAD1A2C479B43268CDEB2C29BBED7AF5732E72B14CC867C76C92E262C16DBE65A841EB3A40F0009A44890D8118B199EA011CCE20051729010678F4C4321BB413B1869EFD3A611DF68923D3A7C94909968B99FCF10AC8B95D7D3B76F4F9EBF3979FEFBC98B1727CF7FD5BD1B76B7503CD5EDDEFFF4CD3FAFBE74FEFEEDC7F72FBF4DBB5EC7731DFFEE97AFDEFDF1E787DCC362D2687DF7FADD9BD7A7DF7FFDD7CF2F2DDEDB093AD0E1431261EEDCC1C7CE7D16C10055744C3EF820B998C5304444B768C7538E62247BB1F8EF89D040DF59208A2CB80E36E3F8300131B1016FCE1F1B8407613217C4E2F1761819C07DC6688725D628DC967D69611ECEE3A9BDF364AEE3EE237464EB3B40B191E5DE7C062A4A6C2E83101B34EF51140B34C531168E7CC70E31B68CEE1121465CF7C928619C4D84F388381D44AC211992036336AD8C6E9108F2B2B011847C1BB1D97FE87418B58DBA8B8F4C24AC0D442DE487981A61BC89E6024536974314513DE07B48843692834532D2713D2E20D3534C99D31B63CE6D36771318AF96F4DB2024F6B4EFD345642213410E6D3EF710633AB2CB0E831045331B7640E250C77EC60F618A22E71E1336F83E3357887C863CA0786BBA1F126CA4FB6C3578001AAA535A4D10F9669E587279133363FE0E167482B0921A907843B923129F29E3690F5723E02093A73FBCB270BE1AD1B63B36227E7A31B96E27C4BA5E6EAD89F436DCBA34072C19938F5F99BB681EDFC3B01836CBD3FFC2FCBF30BBFF7961DEB69EAF5E8E570A0CE22C3782E9965B6DC0A3ADFBEF09A174201614EF71B505E75077C67D689476EAEC89F3F3D82C849F72254307066E9A2065E3244C7C41443808D10CB6EF65573A99F2CCF5943B33C6E1D8A89AADBE259ECEA37D364E8F9DE5B23C62A6E2C19158B597FCBC1D8E0C2245D7EAABA354EE5EB19DAA23EF9280B4BD0809AD339344D542A2BE6C944152076C089A85841AD995B0685A5834A4FB65AA365800B53C2BB03172603BD5727D0F4CC008CE4D88E2B1CC539AEA65765532AF32D3DB8269CC80127CDBC866C02AD34DC975EBF0E4E8D2A9768E4C1B24B4E96692509151358C87688CB3D9295BCF43E3A2B96EAE526AD093A1C862A1D1A8373EC4E2B2B906BB756DA0B1AE1434768E5B6EADEAC39419A159CB9DC0F11D7E4633983B5C6E68119DC237B09148D2057F196599255C74110FD3802BD149D5202202270E2551CB95C3CFD34063A5218A5BB90282F0D1926B82AC7C6CE420E96692F1648247424FBBD622239D3E82C2A75A617DABCC2F0F96966C0EE91E84E363E780CE93FB08A6985F2FCB008E09876F3CE5349A63029F2573215BCDBFB5C294C9AEFE5D50CDA1B41DD15988B28AA28B790A57529ED3514F790CB4A76CCC10502D2459213C98CA02AB07D5A8A679D548396CADBA671BC9C869A2B9AA9986AAC8AA695731A3876519588BE5E58ABCC66A196228977A854FA57B5D729B4BAD5BDB27E45502029EC7CF5275CF5110346AABCE0C6A92F1A60C4BCDCE5ACDDAB11CE019D4CE532434D5AF2DDDAEC52DAF11D6EEA0F152951FECD6672D344D96FB4A1569757FA15F31B083C7201E5DF8983BA782AB54C2054282604334507B92543660893C11D9D2805FCE3C212DF769C96F7B41C50F0AA586DF2B7855AF5468F8ED6AA1EDFBD572CF2F97BA9DCA33282C228CCA7E7A77D2870F4D7491DDA0A8F68D5B9468F92DEDDA884545A66E498A8AB8BA452957B25B14750BD372ADD7290E01F5795AABF49BD566A7566856DBFD82D7ED340ACDA0D629746B41BDDBEF067EA3D97FE63A470AECB5AB8157EB350AB5721014BC5A498EA3D12CD4BD4AA5EDD5DB8D9ED77E96ED672004A98E644181382B82BBFF020000FFFF0300504B030414000600080000002100DE0D573C88030000B708000011000000776F72642F73657474696E67732E786D6CB4565B6FDB36147E1FB0FF20E8798E24C74E03354EB125F0DA225E872A7DD91B251D4B4478034959717F7D0F4931AA912C2856ECC9D4B97CE7F21D1EFAEADD2367C901B4A1526CD2E22C4F13108D6CA9E836E997FBEDE2324D8C25A2254C0AD8A44730E9BBEB5F7FB91A4B03D6A2994910429892379BB4B7569559669A1E383167528140E55E6A4E2C7EEA2EE3443F0C6AD148AE88A53565D41EB3659E5FA4138CDCA48316E504B1E0B4D1D2C8BD752EA5DCEF6903D34FF4D03F123778DECA66E020AC8F986960988314A6A7CA4434FE5FD1B0C43E821C5E2BE2C059B41B8BFC35CBA9DC51EAF6C9E347D2730E4ACB068C4182380BE57242C5134CB17A06F4D4EA336C751662670E0ADD8BDC9FE6CC0D7BE6FF02DB81C53B5A6BA203CD38002E0BDE941F3A2135A9190ED558ACD26B9CA8AF52F2642C15E80649C271CCF334730A2C46EE2B4B2CA0DA2860CCCF67C38020D858769A709CAC4D1A24DEA7853D1998BD277565A542A303C19CDFE49701B23FAA1E84E7FF1F9CECA85F2DD741DFF44493C682AE146930DA8D14564B16ED5AF997B43738C51A9B3C79F89976E986E9AEC2FD400F41385619A4D3CCEF640B2EF341D3678DFC57229C83AF02FBE56B7C3990C4FBAC690B583A83CA1E196C31F98A7E85DF45FB713096E22DF295FF4406AF25807DC5C89FF0F6DF1F156C81D801DBF43F05F34C6C19553BAAB5D41F448BB3F3B3C1B248A2A31397636BE2E1B39436D290E7DB8BE57A3D0DA9339B35B7E7457EF92674E95433FB6094099B976E7DFCADAFAFC2C91196F040F60DE1B5A624D9B90583B4F3B2D60F7F5011F535E08285EF35D55047E56211148613C6B638D151E193E6654B8DBA85BD87653BA2BB1977B2D02F4AF1767D7CC272B715F49F5A0E2A441B3551818818AE58AD263C2AEC1DE5516E86BA8A5E0297C477AA41B49F0EDA0166737BC6D2E2DBE207FA8E882EF6BB1F16EFBF3853E48DE9CABD3FB0234AE1C54593BA2B3629A35D6F0B378416BF5A7C87FC47DD2D27DDD2EBF0CBE9FC07695C65683D1D9C4138A2D5749865E751763ECB70CB06BBD52C5B47D97A965D4419BE836389BB0934AEB8075C0DF1E8E47BC9981CA17D1F859BF4992834C1F44401F2EA36208EAE2CBD605A89263994F088FB155A6AF17957B4E5E4D1ADDBE585739FAC1939CAC19ED83A9D335627D2A42596A0BBA7EAC419A9C37D7D9ACB58B6D0501CC7EAC8EB79A1FE161267D4D80A14EE5E2B3596ECD7DD5B8F3CFFE3B8FE060000FFFF0300504B03041400060008000000210017A0164E02010000AC01000014000000776F72642F77656253657474696E67732E786D6C8CD0C14A03311006E0BBE03B2CB9B7D99522B274B72052F12282FA006976761BCC64C24C6AAC4F6FDAAA205E7ACB24998F997FB9FA405FBD038BA3D0A9665EAB0A82A5C185A953AF2FEBD98DAA249930184F013AB50751ABFEF26299DB0C9B6748A9FC94AA28415AB49DDAA6145BADC56E018DCC2942288F23319A544A9E341A7EDBC599258C26B98DF32EEDF5555D5FAB6F86CF51681C9D853BB23B84908EFD9AC11791826C5D941F2D9FA365E22132591029FBA03F79685CF8659AC53F089D65121AD3BC2CA34F13E90355DA9BFA7842AF2AB4EDC31488CDC6970473B3507D898F6272E83E614D7CCB9405581FAE8DF7949F1EEF4BA1FF64DC7F010000FFFF0300504B03041400060008000000210017B6F063C30700005E3D00001A000000776F72642F7374796C657357697468456666656374732E786D6CB49B6D53DB3810C7DFDFCC7D078FDF4348A070CD34ED50E803336D8F3630F75AB115A2C1B67C7E20709FFE5692AD183BB67763F715C4B1F6B7AB5DFD57A5D2BB0FCF61E03CF12415325AB8D3E313D7E191277D113D2CDCFBBBCF477FB94E9AB1C867818CF8C27DE1A9FBE1FD9F7FBCDBCED3EC25E0A90306A274BE8DBD85BBC9B2783E99A4DE86872C3D0E8597C854AEB3634F8613B95E0B8F4FB632F127B393E989FE2D4EA4C7D31468572C7A62A95B980B9BD664CC2360AD6512B22C3D96C9C32464C9631E1F81F5986562250291BD80ED93F3D28C5CB87912CD0B878EAC436AC8DC3854FC2847248D28F670CDC86BE9E5218F324D9C243C001F64946E44BC0BE3506B10E2A674E9A92B88A73028DFDBC6D3B306CF868CC9C175C2B6908A9DC186B93D93E19B416160E641E57797D5BAC5E949573045469409EB03C685D7CCD2939089C89A396C6AAA930BEB61487D7F49641E5B776231CCDA4DF4686DA96549F0ECE45CAFBC6A6829C94063E92E372CE6AE137AF39B8748266C158047DBE999A32AD27D0F52E14BEF9AAF591E64A9FA98DC26C5C7E293FEF1594659EA6CE72CF584B80309012BA100835F2FA354B8F00D676976990AB6F7CB8D7A6BEF375E9A55AC7D14BE70278A98FE07369F58B07067B3F2C995F2E0D5B380450FE5B34D7EF4F5BEEAC9C2E5D1D1FD523D5A81DD85CB92A3E5A53236D161963F2BE1C6AF82874FDA959879B0F2C00C5B671C44687A0E4ABC9D07426577F6E66DF9E157AE2697E5992C20DA00C0AA66E1636DC6419B40A99646B1E15BBEFE26BD47EE2F33F862E16A163CBCBFB94D844C404617EE5BCD84874B1E8AAFC2F7B96A10CA0FF562B4113EFF67C3A3FB94FBBBE73F3F6B792E2C7A328F3270FFFC42574190FA9F9E3D1E2B990433115319FEA1068086413A2A1CED502E76DE980735AA7EF86F899C9A1CEEA56C38532DCDD1FE778274D4F960D04C45540D40DB25F97A3ADCC4D970136F869B80763C742E2E869B808DCC502F4C6D54AA129FD44C7AA6F8AA3571FAB6A364D5884615F58E68144DEF88468DF48E689444EF884605F48E6824BC774423BFBD231AE9EC1CE1312D5CF52A3AD5B3815AD877220BB81ADF2940D38152776D1AAD73CB12F690B078E3A8C65A77BB4B2C97F92AC3B9AAE5F470B15C668954DBCD9E1981EEAC96EEC19AFC298C372C15B02BEF030D9CFA3BB5F571BE2402B6AF3D2828B5BD31E98DC9DE16761B308F6F64E0F3C4B9E3CF26A38D39691FFF433A4BB3CBE8756E605ABF89874DE6C0AE50B5DC5E98D9E834B3DB1E89B1FF4DA47A0E3A17D3794B287DC651393C6FA9CB76E3DFB92FF2B09C1AC46EE4DCE83921CD358476B17B8ACEA8955820540230219876410F41DB47F86F9A0BDDBECA31C67FD38A0EB48FF0DF34AE03EDEBFAE8CE2F5969AEE1CF2A0E6A795DB40866FB0AB892814CD67950AE815E79B820AF608BC085405EC4D63E4A242EC82BF8957C3A979E07FF72C3D42939173B1D2550C8E93014BDD8F0B190935293BD29212272826AAC1981354C6B0920B2E8FEE24F42FD1198DA0CB44ADBBD66EF723E6D9901D80CA1F6D03F7399F5EFA1672D9A87A5DC44F0E792943B38DA69CBCAC3D2AAFD94524CC31A1FA1988675400268582B24805AEAA3BD6FD99E88870C6F8E041659966D17D30B18ADCC176465B6205A0B18A96F22F65F2DABB7BD169A7D13412127A8D937111472766ABDCCF64D046BB4BE8960B5748DF61C5535951214B96F564156BC11118D23DE08D038E28D008D23DE08D070F1EE878C27DE0816591BACA656C51B01D2AF50FED4624155F14680C8DA60D4AEF89B51D9F7B495EE7FDC8E20DE080A39414DF14650C8D969136F044BBF42A9841ACB4A1D82358E782340E3883702348E782340E3883702348E782340C3C5BB1F329E782358646DB09A5A156F04882C0F1654156F0448BF42D186BDE2AD57FD6F176F04859CA0A6782328E4ECD404D56E52112C72826A2C2BDE08967E85520C054B173725A871C41B11D138E28D008D23DE08D038E28D000D17EF7EC878E28D6091B5C16A6A55BC1120B23C585055BC1120B236EC156FBD187FBB782328E40435C51B412167A726A856E7102C72826A2C2BDE0896AE97C1E28D00E9570E0551221A47BC11118D23DE08D038E28D000D17EF7EC878E28D6091B5C16A6A55BC1120B23C585055BC1120B236EC156FBD467EBB782328E40435C51B412167A726A856BC112C72826A2C2B7508D638E28D00E9C21C2CDE08907EE500905E4594348D23DE8888C6116F0468B878F743C6136F048BAC0D5653ABE28D0091E5C182AAE28D0091B5419DB385F3A2E8E3A9D39622C09E33284F35A081B3962461814580BFF89A2770AB90F79F0E19082C2324105BCA031BE247291F1DDCC1EED3960241A3C42A10521FE97ED1A7742A17114E2F3A6E12DCFD7DE57C3517601AE37449BD3E7903B787AAD785F4F524757108FCCC5E62B8B2139727CB9535B80CA4EE75155780F49DD01BB810545CEB5183D53D1F78515FAA2A1EEBFFB72DA8F03B10F5C026CADB00CB831B511DA8E2C0BB3D83A48FBBD7C12DA7E2B523BB2B19A59BC5E9F8DD1ECABCF7EA8C66A7DF993A09DEE1B33E29DE39478E7EC564B5E9205CCED22EF57908295B05E68A19FC7213F910E1B6B89D6592E93F33630ABEBFE241F09DE90B69998CDB5F0DF83A33DF4E4F7407AC995AC92C9361FBF8441F10D79EEC3300E55075C67C5441B4D74994872B9E14C7CD5B4B52750E7D13ED75499AB3AE2DA5809DE99D6FE56FE9FBFF010000FFFF0300504B030414000600080000002100F04B4F424B0100007702000011000801646F6350726F70732F636F72652E786D6C20A2040128A00001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000008C92514FC3201485DF4DFC0F0DEF2D749B539BB6CBD4ECC92526ABD1F846E06E23164A00D7EDDF4BDBB576D1071F2FE7F0DD732FA48BA32C8303182B2A95A138222800C52A2ED42E43AFC52ABC43817554715A560A3274028B16F9F555CA74C22A032FA6D2609C001B7892B209D319DA3BA7138C2DDB83A436F20EE5C56D652475BE343BAC29FBA43BC01342E65882A39C3A8A1B60A807223A23391B90FACB942D80330C254850CEE2388AF18FD78191F6CF0BAD32724AE14EDACF748E3B6673D68983FB68C560ACEB3AAAA76D0C9F3FC6EFEBE74D3B6A2854B32B06284F394B9801EA2A9317CBF57293E2D149B3BD925AB7F68BDE0AE00FA7DEF45B68BC060EA279A13C4EF1B8F45DDAA1BA56C0031F33E986EA95B7E9E353B142F984C4B7219985645E907932BB4F08F968325DDC6F627707F29CECDFC49BF892D803F236F1E557C9BF010000FFFF0300504B030414000600080000002100E908A5CE430700006D3A00000F000000776F72642F7374796C65732E786D6CB49B6D739B3810C7DFDFCC7D0786F7A9633B8DAF99BA9D34692F99697B699DCCBDC6201B4D0171209AA49FFE56CB430818D80DF455CC83F6B7D2AEFE2B3BD2DBF70F6160FD14492A55B4B6E7AF8E6D4B44AEF264B45FDB77B79F8EFEB2AD543B91E7042A126BFB51A4F6FB777FFEF1F6FE2CD58F81482D3010A567A1BBB67DADE3B3D92C757D113AE92B158B081EEE54123A1A2E93FD2C74921F597CE4AA3076B4DCCA40EAC7D9E2F8F8D42ECC24142B6AB793AEB8546E168A4863FB592202B0A8A2D497715A5ABBA758BB57891727CA15690A9D0E83DC5EE8C8A832333F69190AA59BA854EDF42BE8CC2CF768664C41F3F9317E0A03DB0ADDB3EB7DA412671BC0E0DDCF4FEC7730729E722FC5CEC9029D9ACBE426292E8B2BFCF349453AB5EECF9CD495F21686140C84126C5D9D47A9B4E18970527D9E4AE7E043DFBC75F0899BEA9AB50FD293F6CC10D35F60F3A713ACEDC5A2BC73613C78762F70A27D79CFCF8EAEEEEA9EAC6D111DDD6DCCAD2DD85DDB4E72B43937C666D8CDF26FADBBF1B3CEC315BA123B2E0403CC383B2D2029E6A79098F767813439B878FDA6BCF89E99717532AD0A081A0058DD2C5C36461C720532679327303C15BBCFCAFD21BC8D86076B1B5970F3EEFA26912A81245DDB6F90093737229457D2F384992FC60FF362E44B4FFCEB8BE82E15DED3FD6F9F30F90B8BAECA220DEE9FAE300B82D4FBF8E08AD8A42D98891C13E1AFA601240E84A3C6418732F9E44D7EA341C59BFF95C8791EC383145F3866865BE87F2F087B9D8D062D4C8FEA1D40BB2C5F97E34D9C8C37F17ABC0910BBB163B11A6F02747DAC17796ED4B2921E54ADDC3CF9EA39B17CD393B2A6452B8B065BB49266B0452B47065BB45262B0452B03065BB4023ED8A215DFC116AD70F6B6701D14AE66162D71344813FB56EA4098F6BD02341F29759779A1B56E9CC4D9274EEC5BA6B036DDEE13CB4DB6D53457514E5F2E961B9DA8683F3822509DCDD47DB1267F0C63DF4925AC9206867E3172E86FCDAAC7FA3B91DE200A52ED609F706172B084DD048E2B7C157822B16EC5431ED1D69874B7FFAAAC4DBECA18746E64583FCBBDAFAD8D8F257710962F74DAD1EDEE496EFFB34C710C7A27D3694757868C936278DA9197DDC6BF084F6661393484D5C869AEE78C303710E862FF109D7033B140980050BA90970B7E17D03EC1FFBCB8F0ED9B1853FCCF4BD10BED13FCCF0BD70BED637EF4C797AD3497F0A5D5224DAF55876076CF800B15A8649705E51C189487157B0657085A17D893B8B24F1289157B063F934FEBDC75E19B1B254FD9B178D25106851D8E9C82938DDE1776501AB23767F4881DA0066BC1608DD35A06882DBADFC54F697E13E3160354E96AAD39389D971D23008B21D21AFA5BA6F4F01A7AD1A17954CA75043F97A4C2A2D1961D338F4AABD7534E328D2B7C8C641A570119A071A59001EAC88FEEBA55D5443A647C7164B0D8B25C55319CC064655EB195B902F14AC0447593B0FEEA98BDDDB9D0AE9B040A3B40EDBA49A0B0A3D3A86555DD24B026AB9B045647D5E88E515D53399D62D7CD3AA8126F428FA6116F02681AF12680A6116F0268BC780F43A6136F028BAD0D95A6D6C59B00C257383FB554A0BA7813406C6DC8D5AEF8CDA8AC7B68A5FFCBED04E24DA0B003D4166F02851D9D2EF126B0F0154E26345895D41158D3883701348D781340D3883701348D781340D3883701345EBC8721D3893781C5D6864A53EBE24D00B1E5A102D5C59B00C25738DA7050BC71D6FF76F12650D8016A8B3781C28E4E4350AB452A81C50E50835589378185AF7092A1606172733A358D78137A348D781340D3883701348D781340E3C57B18329D7813586C6DA834B52EDE04105B1E2A505DBC0920B6361C146F9C8CBF5DBC09147680DAE24DA0B0A3D310D44AE7082C76801AAC4ABC092CCC97D1E24D00E12B2F05717A348D78137A348D781340D3883701345EBC8721D3893781C5D6864A53EBE24D00B1E5A102D5C59B00626BC341F1C639F2DBC59B406107A82DDE040A3B3A0D41ADC49BC06207A8C1AAA48EC09A46BC09204CCCD1E24D00E12B2F00E12CE284691AF126F4681AF12680C68BF730643AF126B0D8DA50696A5DBC0920B63C54A0BA7813406C6D30FB6C61BF28797BEABC2309A8FB0CCA5D0D64E0A223485460D1C1EF6227123864258677878C04963D64103BD283DAC50F4AFDB0681BBB971D094246C96D20156EE97EC45D3AB58308CB55CF4982DB7F2EACABFC004CAB1DA6D4F39D37707AA87E5C088F27998343E0A77E8CE1C84E5CEE2C37D6E0309039D7551C01C22372D77020A838D6631A9B733EF0221EAA2A6EE3FF6D0B2A7C0622366CA35C1F582E9C88EA41151BDEAB3D48B8DDBD09EED8158F8E3C1DC928DD2C76C73FADA1F2F79EEDD1ECF55B9B9DE03D3EE34EF1DE31B2F0953CAA6D07E17016BA34E421846C1BE447CCE0C375E4410FE19020FED72C0FA6F7E0E4A6E0F98508822F0E1E48D32AEE7E35103B9D3F9D1F63056C98DA2AAD55D8DD3EC10DE2E8C92103900E7567F24BD389EE3C89B2702B1238E1D533E65F95A91C7812ED794AE67B5D3B52813AD24FBE959FD277FF030000FFFF0300504B030414000600080000002100A361E28CC8010000A804000012000000776F72642F666F6E745461626C652E786D6CBC92DD6AE3301085EF17FA0E46F78D65C7FD33754A361BC3C2B217A57D0045916D51FD188D12376FDFB1E4E4A2A190B2B03608FB8CF479E6F83C3EBD6B95EC8503694D45B219258930DC6EA5692BF2FA525FDF93043C335BA6AC11153908204F8BAB1F8F43D958E321C1F3064ACD2BD279DF97690ABC139AC1CCF6C260B1B14E338FAFAE4D35736FBBFE9A5BDD332F3752497F48734A6FC9847197506CD3482E7E59BED3C2F8703E754221D11AE8640F47DA70096DB06EDB3BCB0500CEAC55E46926CD09931567202DB9B3601B3FC361D2D8513AA2F07846C3935624D1BCFCDD1AEBD846A177435690C5645C3294866914574CC98D93A1D03363416458DB3355119AD39ADEE03ADE059D8F2B494702EF9803E18F1BD7EB28374C4B7538AA30488058E8A5E7DD51DF3327C7866209648B851D6C6845D619A534AF6B1295AC22050ACBD549C9B1A9783D4C7BE6270593838D054ED8923D040E2AC8994ED1F19B698CCE99132F520B48FE8A2179B69A992F1CC9E92D3A71837E8CCECCBFE5880BDCE0E0A58E60E3F9F2343F4EB242E5EEBEC8A6F9BFE548E45CEEC8948DE48F6C3BFF851F75C8C5FF4AC872FC91F9FA5342727AF7F3CC8F9007CCD5BF24648A0A2C3E000000FFFF0300504B0304140006000800000021003BA8275374010000C502000010000801646F6350726F70732F6170702E786D6C20A2040128A00001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009C52CB4EC33010BC23F10F51EEAD930A28421B23D40A71E0253585B3E56C120BC7B66C17D1BF67D3D034881B39EDCCAE27B363C3ED57A7934FF4415953A4F93C4B1334D256CA3445BA2DEF67D76912A23095D0D66091EE31A4B7FCFC0C5EBD75E8A3C29090840945DAC6E86E180BB2C54E8439B50D756AEB3B1109FA86D9BA5612D756EE3A34912DB2EC8AE1574453613573A3603A28DE7CC6FF8A5656F6FEC25BB977649843899DD322227FEEED68602301A58D4297AA43BE207A04F02A1A0C3C073614F06E7D15F88286860A56ADF04246CA8EE7974B60130C77CE692545A454F99392DE065BC7E4E5B07FD29F07361D01CA648372E755DCF30CD814C2A3328391A120635E345EB8F6C7DD88602385C6152DCE6BA103023B11B0B29D1366CFC9E7B122BD8FB075A55DF7C9FC1CF94D4E967C57B1DD38217B2FCBEBE9BA930E6C2814ACC8FF51EF44C0035D86D7FD4F292AD360759CF9DBE8037C1B5E25CF2FE6197D87C48E1CDDCAF85CF837000000FFFF0300504B01022D001400060008000000210009248782810100008E0500001300000000000000000000000000000000005B436F6E74656E745F54797065735D2E786D6C504B01022D00140006000800000021001E911AB7F30000004E0200000B00000000000000000000000000BA0300005F72656C732F2E72656C73504B01022D00140006000800000021007C3B973922010000B90300001C00000000000000000000000000DE060000776F72642F5F72656C732F646F63756D656E742E786D6C2E72656C73504B01022D001400060008000000210010BC9D77E9020000DD090000110000000000000000000000000042090000776F72642F646F63756D656E742E786D6C504B01022D00140006000800000021009940B5E1BD0600005C1A000015000000000000000000000000005A0C0000776F72642F7468656D652F7468656D65312E786D6C504B01022D0014000600080000002100DE0D573C88030000B708000011000000000000000000000000004A130000776F72642F73657474696E67732E786D6C504B01022D001400060008000000210017A0164E02010000AC010000140000000000000000000000000001170000776F72642F77656253657474696E67732E786D6C504B01022D001400060008000000210017B6F063C30700005E3D00001A0000000000000000000000000035180000776F72642F7374796C657357697468456666656374732E786D6C504B01022D0014000600080000002100F04B4F424B01000077020000110000000000000000000000000030200000646F6350726F70732F636F72652E786D6C504B01022D0014000600080000002100E908A5CE430700006D3A00000F00000000000000000000000000B2220000776F72642F7374796C65732E786D6C504B01022D0014000600080000002100A361E28CC8010000A80400001200000000000000000000000000222A0000776F72642F666F6E745461626C652E786D6C504B01022D00140006000800000021003BA8275374010000C502000010000000000000000000000000001A2C0000646F6350726F70732F6170702E786D6C504B0506000000000C000C0009030000C42E00000000,  'False',  '224_Költöztetés igénybejelento wf template.doc.doc',  'False',  'False'); ";
                    //       connectionManager.executeQueriesInDbFile(insertText);    connectionManager.executeQueriesInDbFile(insertText);
                 //   connectionManager.executeQueriesInSqlServer(insertText2);
                    
					foreach (string tableName in tableInfo.getFirstRoundInsertTables())
					{

						if (tableName != "T_DB_CONNECTION" && tableName != "T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION" && tableName !=  "T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR" && tableName != "T_CATEGORY" && tableName != "T_ACTIVITY_FINISH_STEP_MODE" && tableName != "T_ACTIVITY_UI_COMPONENT" && tableName != "T_FIELD_VALUE_TRANSLATION")
						{

							if (!(tableInfo.tableInDBFileWithoutRow(connectionManager, tableName)))
							{

								if (listOfTablesWhereIdentityInsertNeeded.Contains(tableName))
								{
									insertValuesFromDbFileToSqlServer(tableName, true, connectionManager, ref insertResultInfo);
								}
								else
								{
									insertValuesFromDbFileToSqlServer(tableName, false, connectionManager, ref insertResultInfo);
								}

							}
						}

					}
                    
					foreach (string tableName in tableInfo.getSecondRoundInsertTables())
					{
						//insertResultInfo.Add("Aktuális tábla : " + tableName + ";");

						if (tableName != "T_DB_CONNECTION" && tableName != "T_DEPARTMENT" && tableName != "T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR"  &&  tableName != "T_USER_DEFINED_TABLE" && tableName != "T_ACTIVITY_UI_COMPONENT"  && tableName != "T_LANGUAGE" && tableName != "T_CATEGORY" && tableName != "T_ACTIVITY_FINISH_STEP_MODE"   && tableName != "T_OPERATION" && tableName != "T_FIELD_VALUE_TRANSLATION")
						{
                           

                                if (!(tableInfo.tableInDBFileWithoutRow(connectionManager, tableName)))
							    {
								    if (secondRoundInsertTablesWithoutIdentityProprty.Contains(tableName))
								    {
									
									//    insertValuesFromDbFileToSqlServer(tableName, false, connectionManager);

								    }
								    else
								    {
                                        if (tableName == "T_PROCFIELD_WORD_MERGE")
                                        {
                                           insertValuesFromDbFileToSqlServer(tableName, true, connectionManager , ref insertResultInfo);
                                        }
                                        else
                                        {
                                            insertValuesFromDbFileToSqlServer(tableName, true, connectionManager, ref insertResultInfo);						

                                        }
								    }

							    }
                            }
						

					}

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
        /*
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

								insertValuesFromDbFileToSqlServer(tableName, true, connectionManager, ref insertResultInfo);
							}
							else
							{

								insertValuesFromDbFileToSqlServer(tableName, false, connectionManager, ref insertResultInfo);

							}

						}
					}

				}

				foreach (string tableName in tableInfo.getSecondRoundInsertTables())
				{

					if (tableName != "T_DB_CONNECTION" && tableName != "T_DEPARTMENT" && tableName != "T_LANGUAGE" && tableName != "T_CATEGORY"  && tableName != "T__OPERATION")
					{

						if (!(tableInfo.tableInDBFileWithoutRow(connectionManager, tableName)))
						{
							if (secondRoundInsertTablesWithoutIdentityProprty.Contains(tableName))
							{

								insertValuesFromDbFileToSqlServer(tableName, false, connectionManager, ref insertResultInfo);

							}
							else
							{
                             
								insertValuesFromDbFileToSqlServer(tableName, true, connectionManager, ref insertResultInfo);
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
        */
		public List<string> insertValuesFromDbFileToSqlServer(string tableName, bool needToSetIdentityInsertOn, ConnectionManagerST obj, ref List<string> insertresultInfo)
		{
		//	List<string> insertresultInfo = new List<string>();
			List<string> values = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			string commandText = "INSERT INTO " + tableName + "  ( ";
            int paramIndex = 0;
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
                                object currentObjectRecord = reader[columnTypes.ElementAt(index).Key.ToString()];
							    string currentRecordSingleQuoteFormalized = currentRecord.Replace("'", "''");
                             
						  
							    switch (columnTypes.ElementAt(index).Value)
							    {

								    case "varbinary":
                                    case "varbinary(max)":
                                    case "BLOB":
                                    case "blob":
                                    paramIndex++;
                                    commandText += " @parameter"+ paramIndex.ToString();
                                    parameters.Add(new SqlParameter("parameter" + paramIndex.ToString(), currentObjectRecord));
                                    break;
                                case "bit":
								    case "binary":
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
                    if (parameters.Count > 0)
                    {
                        obj.executeQueriesInSqlServerWithParameters("SET IDENTITY_INSERT " + tableName + " ON ; " + commandText + " ; SET IDENTITY_INSERT " + tableName + " OFF ;", parameters.ToArray());
                    }
                    else
                    {
					    obj.executeQueriesInSqlServer("SET IDENTITY_INSERT " + tableName + " ON ; " + commandText + " ; SET IDENTITY_INSERT " + tableName + " OFF ;");

                    }
				}
				else
				{
					obj.executeQueriesInSqlServer(commandText);
				}
			}
			catch (Exception e)
			{
				insertresultInfo.Add(e.ToString() + e.StackTrace + (e.InnerException == null ? "" : e.InnerException.ToString() + e.InnerException.StackTrace ));
				throw new Exception();

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
		public ServiceCallResult Export_Process_For_SubProcesses(int subProcessId , ConnectionManagerST connectionManager , General_Info gen_inf) { 

			var ExportManager = new Export(); 
			ServiceCallResult res = new ServiceCallResult { Code = 0, Description = "OK" };
			try
			{
				ExportManager.processes_v2 = new List<ProcessListItem>();
				ExportManager.fieldsForProcess_v2 = new List<long>();
				res = ExportManager.TransferProcess_v2(subProcessId, connectionManager, true, true);
			}
			catch (Exception e)
			{
					res = ExportManager.FillServiceCallResult_v2(e);
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
