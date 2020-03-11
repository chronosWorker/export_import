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
         //       "T_ACTIVITY_FINISH_STEP_MODE" ,
                "T_ACTIVITY_PARTICIPANT_TYPE" ,
                "T_SYSTEM_INTERFACE",
                "T_CALCULATED_FIELD_CONSTANT_TYPE" ,
                "T_COMPARE_OPERATION" ,
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

        //  public List<string> getAllTableWhereIdExits(ConnectionManagerST obj)
        // {
        //  List<string> tables
        //select table_name from table_information where column_name like '%Process_Id%';
        // }
    }
}