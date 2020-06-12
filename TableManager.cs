using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class TableManager
    {

        public List<string> getFirstRoundInsertTables()
        {

            List<string> firstRoundInsertTables = new List<string>()
                {
                "T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE",
                "T_ACTIVITY_DEPENDENT_COMPONENTS",
                "T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION",
                "T_ACTIVITY_FIELDS_FOR_ESIGNING",
                "T_ACTIVITY_FIELDS_UI_PARAMETERS",
        //       "T_ACTIVITY_FINISH_STEP_MODE",
                "T_ACTIVITY_OWNER_BY_CONDITION_CONDITION",
                "T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP",
                "T_ACTIVITY_UI_COMPONENT",
                "T_FIELD_DATE_CONSTRAINT",
                "T_FIELD_EXTENSION_NUMBER",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS",
                "T_FIELD_TO_FIELD_DEPENDENCY",
                "T_FIELD_TO_FIELD_DEPENDENCY_TYPE",
                "T_FIELD_VALUE",
                "T_FIELD_VALUE_TRANSLATION",
                "T_PROC_DESIGN_DRAW",
                "T_PROC_DESIGN_DRAW_PART",
                "T_PROC_DESIGN_DRAW_PART_DETAIL",
                "T_PROC_DESIGN_DRAW_PART_TYPE",
                "T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE",
                "T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE",
                "T_REPORT_FIELD_UDT_COLUMNS",
                "T_REPORT_FILTER",
                "T_REPORT_GROUP",  
                "T_REPORT_REFERENCED_FIELD_LOCATION",
                "T_REPORT_TYPE"
                };

            return firstRoundInsertTables;
        }
        public List<string> getAllTablesNameInDbFile()
        {

            List<string> allTables = new List<string>()
                {
                "T_PROCESS" ,
                "T_PROCESS_DESIGN" ,
                "T_PROC_DESIGN_DRAW" ,
                "T_PROC_DESIGN_DRAW_PART" ,
                "T_PROC_DESIGN_DRAW_PART_TYPE" ,
                "T_ACTIVITY_DESIGN" ,    
                "T_FIELD_CONDITION_GROUP" ,
                "T_FIELD_DATE_TYPE" ,
                "T_FIELD" ,
                "T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE" ,
                "T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE" ,
                "T_FIELD_TEXT_FORMAT_TYPE" ,
                "T_FIELD_TO_FIELD_DEPENDENCY_TYPE" ,
                "T_FIELD_TYPE" ,
                "T_FILE_FIELD_TYPE" ,
                "T_ACTIVITY" ,
                "T_ACTIVITY_FIELDS_UI_PARAMETERS" ,
                "T_NOTIFICATION" ,
                "T_DEPARTMENT" ,
                "T_CALCULATED_FIELD_RESULT_TYPE_ID" ,
                "T_CATEGORY" ,
                "T_PROCESS_OWNER" ,
                "T_PROCESS_READER" ,
                "T_ROLE" ,
                "T_NOTIFICATION_TRIGGER",
                "T_ROUTING_DESIGN" ,
                "T_ROUTING" ,
                "T_REPORT_GROUP" ,
                "T_REPORT_GROUP_ADMINISTRATOR" ,
                "T_PROC_DESIGN_DRAW_PART_DETAIL" ,
                "T_ROUTING_CONDITION" ,
                "T_ROUTING_CONDITION_GROUP" ,
                "T_FIELD_CONDITION" ,
                "T_FIELD_DATE_CONSTRAINT" ,
                "T_FIELD_EXTENSION_NUMBER" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS" ,
                "T_FIELD_LABEL_TRANSLATION" ,
                "T_FIELD_VALUE" ,              
                "T_ACTIVITY_FIELDS" ,
                "T_ACTIVITY_FIELDS_FOR_ESIGNING" ,
                "T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION" ,
                "T_ACTIVITY_DEPENDENT_COMPONENTS" ,
                "T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION" ,
                "T_DYNAMIC_ROUTING" ,
                "T_CALCFIELD_FORMULA_STEPS" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS" ,
                "T_USER_DEFINED_TABLE" ,
                "T_FORMULA_STEPS" ,
                "T_OPERAND" ,
                "T_PROCFIELD_PARTICIPANT" ,
                "T_PROCFIELD_WORD_MERGE" ,
                "T_REPORT_FIELD" ,
                "T_REPORT" ,
                "T_REPORT_2_FIELD_COND_GROUP" ,
                "T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE" ,
                "T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE" ,
                "T_REPORT_EDIT_OWNER" ,
                "T_REPORT_FIELD_UDT_COLUMNS" ,
                "T_REPORT_FILTER" ,
                "T_REPORT_REFERENCED_FIELD_LOCATION" ,
                "T_SUBPROCESS" ,
                "T_ACTIVITY_OWNER_BY_CONDITION" ,
                "T_ACTIVITY_OWNER_BY_COND_PARTICIPANT" ,
                "T_ACTIVITY_OWNER_BY_CONDITION_CONDITION" ,
                "T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP" ,
                "T_ACTIVITY_PARTICIPANT" ,
                "T_ACTIVITY_UI_COMPONENT" ,
                "T_AUTOMATIC_PROCESS" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY" ,
                "T_FIELD_TO_FIELD_DEPENDENCY" ,
                "T_FIELD_VALUE_TRANSLATION" ,
                "T_CHART_TYPE" ,
                "T_CHART_FIELD_TYPE" ,
                "T_LANGUAGE" ,
                "T_REPORT_TYPE" ,
                "T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE" ,
                "T_ACTIVITY_FINISH_STEP_MODE" ,
                "T_ACTIVITY_PARTICIPANT_TYPE" ,
                "T_SYSTEM_INTERFACE",
                "T_CALCULATED_FIELD_CONSTANT_TYPE" ,
                "T_COMPARE_OPERATION" ,
                "T_SYSTEM_INTERFACE_TYPE",
                "T_PROCFIELD_WORD_MERGE_FIELD" ,
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA",
                 };

            return allTables;
        }
        public List<string> getSecondRoundInsertTables()
        {
            List<string> secondRoundTables = (getAllTablesNameInDbFile()).Except(getFirstRoundInsertTables()).ToList();
            return secondRoundTables;
        }
        public List<string> secondRoundInsertTablesWithoutIdentityProprty()
        {
            List<string> secondRoundInsertTablesWithoutIdentityProprty = new List<string>()
            {
                "T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE",
                "T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE",
                "T_FIELD_TYPE",
                "T_FILE_FIELD_TYPE",
                "T_CALCULATED_FIELD_RESULT_TYPE_ID",
                "T_ACTIVITY_FIELDS",
                "T_ACTIVITY_PARTICIPANT",
                "T_ACTIVITY_PARTICIPANT_TYPE",
                "T_CHART_TYPE",
                "T_CHART_TYPE",
                "T_PROCESS_OWNER",
                "T_CHART_FIELD_TYPE",
                "T_CALCULATED_FIELD_CONSTANT_TYPE",
                "T_COMPARE_OPERATION",
                "T_PROCESS_READER",
            };
            return secondRoundInsertTablesWithoutIdentityProprty;
        }
        public List<string> listOfTablesThatDontNeedToBeInsert()
        {

            List<string> noInsertTables = new List<string>()
                {
                "T_PROCESS_OWNER",
                "T_DB_CONNECTION"
                };
            return noInsertTables;
        }
        public List<string> listOfTablesWhereIdentityInsertNeeded()
        {

            List<string> listOfTablesWhereIdentityInsertNeeded = new List<string>()
                {
                "T_FIELD_DATE_CONSTRAINT",
                "T_ACTIVITY_FIELDS_UI_PARAMETERS",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS",
                "T_FIELD_TO_FIELD_DEPENDENCY",
                "T_FIELD_VALUE",
                "T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION",
                "T_FIELD_VALUE_TRANSLATION",
                "T_PROC_DESIGN_DRAW",
                "T_PROC_DESIGN_DRAW_PART",
                "T_PROC_DESIGN_DRAW_PART_DETAIL",
                "T_REPORT_GROUP",
                "T_FIELD_EXTENSION_NUMBER"
                };
            return listOfTablesWhereIdentityInsertNeeded;
        }   
        public bool tableInDBFileWithoutRow(ConnectionManagerST obj, string tableName)
        {
            bool tableWithoutRow = true;
            try
            {
             
                var reader = obj.sqLiteDataReader("SELECT count(*) from " + tableName);

                if (reader.Read())
                {
                    if (reader[0].ToString() != "0")
                    {
                        tableWithoutRow = false;

                    }

                }
               
            }
            catch (Exception ex)
            {
                throw ex;

            }

            return tableWithoutRow;
        }
        public List<string> firstRoundTablesWithContent(ConnectionManagerST obj)
        {
            List<string> allFirstRoundTables = getFirstRoundInsertTables();
            List<string> firstRoundTablesWithContent = new List<string>();
            List<string> firstRoundTablesWithOutContent = new List<string>();
            foreach (string tableName in allFirstRoundTables)
            {
                firstRoundTablesWithOutContent.Add(tableInDBFileWithoutRow(obj, tableName).ToString());

            }
            firstRoundTablesWithContent.AddRange(allFirstRoundTables.Except(firstRoundTablesWithOutContent).ToList());

            return firstRoundTablesWithContent;
        }
        public List<string> twoDimensionaltypeTables()
        {
            List<string> twoDimensionaltypeTables = new List<string>()
            {
                "T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE", 
                "T_ACTIVITY_PARTICIPANT_TYPE",
                "T_CALCULATED_FIELD_CONSTANT_TYPE",
                "T_CALCULATED_FIELD_RESULT_TYPE_ID",
                "T_CATEGORY",
                "T_CHART_FIELD_TYPE",
                "T_CHART_TYPE",
                "T_COMPARE_OPERATION",
                "T_FIELD_DATE_TYPE",
                "T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE",
                "T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE",
                "T_FIELD_TO_FIELD_DEPENDENCY_TYPE",
                "T_FILE_FIELD_TYPE",
                "T_PROC_DESIGN_DRAW_PART_TYPE",
                "T_REPORT_TYPE",
            };

            return twoDimensionaltypeTables;
        }
        public List<KeyValuePair<string, string>> getTableNameWitkFkIdNameList()
        {
            List<KeyValuePair<string, string>> tableNameWitkFkIdNameList = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("T_PROCESS","Process_ID" ),
                new KeyValuePair<string, string>("T_PROCESS_DESIGN","Process_Design_ID" ),
                new KeyValuePair<string, string>("T_PROC_DESIGN_DRAW","Proc_Design_Draw_ID" ),
                new KeyValuePair<string, string>("T_PROC_DESIGN_DRAW_PART","Proc_Design_Draw_Part_ID" ),
                new KeyValuePair<string, string>("T_PROC_DESIGN_DRAW_PART_TYPE","Proc_Design_Draw_Part_Type_ID" ),
                new KeyValuePair<string, string>("T_ROUTING","Routing_ID" ),
                new KeyValuePair<string, string>("T_FIELD","Field_ID" ),
                new KeyValuePair<string, string>("T_FIELD_CONDITION_GROUP","Field_Condition_Group_ID" ),
                new KeyValuePair<string, string>("T_FIELD_DATE_TYPE","Date_Field_Type_ID" ),
                new KeyValuePair<string, string>("T_FIELD_DOCUMENT_REFERENCE_IMPORT_TYPE","Field_Document_Reference_Import_Ttype_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_CONDITION_OPERATOR","Field_Group_To_Field_Group_Condition_Operator_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY","Field_Group_To_Field_Group_Dependency_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_MODE","Field_Group_To_Field_Group_Dependency_Mode_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_TYPE","Field_Group_To_Field_Group_Dependency_Type_ID" ),
                new KeyValuePair<string, string>("T_FIELD_TEXT_FORMAT_TYPE","Field_Text_Format_Type_ID" ),
                new KeyValuePair<string, string>("T_FIELD_TO_FIELD_DEPENDENCY_TYPE","Field_To_Field_Dependency_Type_ID" ),
                new KeyValuePair<string, string>("T_FIELD_TYPE","Field_Type_ID" ),
                new KeyValuePair<string, string>("T_FILE_FIELD_TYPE","File_Field_Type_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY","Activity_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_FIELDS_UI_PARAMETERS","Activity_Fields_UI_Paramaters_ID" ),
                new KeyValuePair<string, string>("T_NOTIFICATION","Notification_ID" ),
                new KeyValuePair<string, string>("T_DEPARTMENT","Department_ID" ),
                new KeyValuePair<string, string>("T_DEPARTMENT_MEMBERS","Department_ID" ),
                new KeyValuePair<string, string>("T_CALCULATED_FIELD_RESULT_TYPE_ID","Calculated_Field_Result_Type_ID" ),
                new KeyValuePair<string, string>("T_CATEGORY","Category_ID" ),
                new KeyValuePair<string, string>("T_PROCESS_OWNER","Process_ID" ),
                new KeyValuePair<string, string>("T_PROCESS_READER","Process_ID" ),
                new KeyValuePair<string, string>("T_ROLE","Role_ID" ),
                new KeyValuePair<string, string>("T_ROLE_MEMBERS","Role_ID" ),
                new KeyValuePair<string, string>("T_REPORT_GROUP","Report_Group_ID" ),
                new KeyValuePair<string, string>("T_REPORT_GROUP_ADMINISTRATOR","Report_Group_Administrator_ID" ),
                new KeyValuePair<string, string>("T_REPORT_OWNERS","Report_ID" ),
                new KeyValuePair<string, string>("T_PROC_DESIGN_DRAW_PART_DETAIL","Proc_Design_Draw_Part_Detail_ID" ),
                new KeyValuePair<string, string>("T_ROUTING_CONDITION","Routing_Condition_ID" ),
                new KeyValuePair<string, string>("T_ROUTING_CONDITION_GROUP","Routing_Condition_Group_ID" ),
                new KeyValuePair<string, string>("T_ROUTING_DESIGN","Routing_Design_ID" ),
                new KeyValuePair<string, string>("T_FIELD_CONDITION","Field_Condition_ID" ),
                new KeyValuePair<string, string>("T_FIELD_DATE_CONSTRAINT","Field_Date_Constraint_ID" ),
                new KeyValuePair<string, string>("T_FIELD_EXTENSION_NUMBER","Field_Extension_Number_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS","Field_Group_To_Field_Group_Dependent_Fields_ID" ),
                new KeyValuePair<string, string>("T_FIELD_LABEL_TRANSLATION","Field_Label_Translation_ID" ),
                new KeyValuePair<string, string>("T_FIELD_VALUE","Field_Value_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_DESIGN","Activity_Design_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_FIELDS","Activity_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_FIELDS_FOR_ESIGNING","Activity_Fields_For_ESigning" ),
                new KeyValuePair<string, string>("T_ACTIVITY_BEFORE_ESCALATION_NOTIFICATION","Activity_Before_Escalation_Notification_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_DEPENDENT_COMPONENTS","Activity_Dependent_UI_Components_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION","Activity_Dependent_UI_Component_Translation_ID" ),
                new KeyValuePair<string, string>("T_DYNAMIC_ROUTING","Dynamic_Routing_ID" ),
                new KeyValuePair<string, string>("T_CALCFIELD_FORMULA_STEPS","CALCFIELD_FORMULA_STEPS_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS","FIELD_GROUP_TO_FIELD_GROUP_T_ACTIVITY_FIELDS_ID" ),
                new KeyValuePair<string, string>("T_USER_DEFINED_TABLE","USER_DEFINED_TABLE_ID" ),
                new KeyValuePair<string, string>("T_FORMULA_STEPS","FORMULA_STEPS_ID" ),
                new KeyValuePair<string, string>("T_CALCFIELD_OPERAND","CALCFIELD_OPERAND_ID" ),
                new KeyValuePair<string, string>("T_OPERAND","OPERAND_ID" ),
                new KeyValuePair<string, string>("T_PROCFIELD_PARTICIPANT","Procfield_Participant_ID" ),
                new KeyValuePair<string, string>("T_PROCFIELD_WORD_MERGE","Procfield_Word_Merge_ID" ),
                new KeyValuePair<string, string>("T_PROCFIELD_WORD_MERGE_FIELD","Procfield_Word_Merge_Field_ID" ),
                new KeyValuePair<string, string>("T_REPORT_FIELD","Report_Field_ID" ),
                new KeyValuePair<string, string>("T_REPORT","Report_ID" ),
                new KeyValuePair<string, string>("T_REPORT_2_FIELD_COND_GROUP","Report_2_Field_Cond_Group_ID" ),
                new KeyValuePair<string, string>("T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE","Report_Calculated_Field_Formula_Tree_Node_ID" ),
                new KeyValuePair<string, string>("T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE","Report_Calculated_Field_Formula_Tree_Node_Value_ID" ),
                new KeyValuePair<string, string>("T_REPORT_EDIT_OWNER","Report_ID" ),
                new KeyValuePair<string, string>("T_REPORT_FIELD_UDT_COLUMNS","Report_Field_UDT_Columns_ID" ),
                new KeyValuePair<string, string>("T_REPORT_FILTER","REPORT_ID" ),
                new KeyValuePair<string, string>("T_REPORT_REFERENCED_FIELD_LOCATION","Report_Referenced_Field_Location_ID" ),
                new KeyValuePair<string, string>("T_SUBPROCESS","Subprocess_ID" ),
                new KeyValuePair<string, string>("T_SYSTEM_INTERFACE","System_Interface_ID" ),
                new KeyValuePair<string, string>("T_SYSTEM_INTERFACE_TRIGGER","System_Interface_Trigger_ID" ),
                new KeyValuePair<string, string>("T_SYSTEM_INTERFACE_TYPE","System_Interface_Type_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_OWNER_BY_CONDITION","Activity_Owner_By_Condition_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_OWNER_BY_COND_PARTICIPANT","Act_Owner_By_Cond_Participant_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_OWNER_BY_CONDITION_CONDITION","Activity_Owner_By_Condition_Condition_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP","Activity_Owner_By_Condition_Condition_Group_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_PARTICIPANT","Activity_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_UI_COMPONENT","Activity_UI_Component_ID" ),
                new KeyValuePair<string, string>("T_AUTOMATIC_PROCESS","Automatic_Process_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY","Field_Group_To_Field_Group_Dependency_Activation_Activity_ID" ),
                new KeyValuePair<string, string>("T_FIELD_TO_FIELD_DEPENDENCY","Field_To_Field_Dependency_ID" ),
                new KeyValuePair<string, string>("T_FIELD_VALUE_TRANSLATION","Field_Value_Translation_ID" ),
                new KeyValuePair<string, string>("T_CHART_TYPE","Chart_Type_ID" ),
                new KeyValuePair<string, string>("T_CHART_FIELD_TYPE","Chart_Type_Field_ID" ),
                new KeyValuePair<string, string>("T_LANGUAGE","Language_ID" ),
                new KeyValuePair<string, string>("T_NOTIFICATION_TRIGGER","Notification_Trigger_ID" ),
                new KeyValuePair<string, string>("T_NOTIFICATION_TYPE","NOTIFICATION_TYPE_ID" ),
                new KeyValuePair<string, string>("T_NOTIFICATION_RECIPIENT_TYPE","Recipient_Type_ID" ),
                new KeyValuePair<string, string>("T_REPORT_TYPE","Report_Type_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE","Activity_Before_Finish_Check_Query_Type_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_FINISH_STEP_MODE","Activity_Finish_Step_Mode_ID" ),
                new KeyValuePair<string, string>("T_ACTIVITY_PARTICIPANT_TYPE","Activity_Participant_Type_ID" ),
                new KeyValuePair<string, string>("T_NOTIFICATION_ADDRESS","Notification_ID" ),
                new KeyValuePair<string, string>("T_CALCULATED_FIELD_CONSTANT_TYPE","Calculated_Field_Constant_Type_ID" ),
                new KeyValuePair<string, string>("T_COMPARE_OPERATION","Compare_Operation_ID" ),
                new KeyValuePair<string, string>("T_DB_CONNECTION","DB_Connection_ID" ),
                new KeyValuePair<string, string>("T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA","Field_Group_To_Field_Group_Dependency_Condition_Formula_ID" ),
            };

            return tableNameWitkFkIdNameList;

        }
    }

        //  public List<string> getAllTableWhereIdExits(ConnectionManagerST obj)
        // {
        //  List<string> tables
        //select table_name from table_information where column_name like '%Process_Id%';
        // }
    }
