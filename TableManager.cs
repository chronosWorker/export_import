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
                " T_ACTIVITY_BEFORE_FINISH_CHECK_QUERY_TYPE ",
                " T_ACTIVITY_DEPENDENT_COMPONENTS ",
                " T_ACTIVITY_DEPENDENT_COMPONENT_TRANSLATION ",
                " T_ACTIVITY_FIELDS_FOR_ESIGNING ",
                " T_ACTIVITY_FIELDS_UI_PARAMETERS ",
                " T_ACTIVITY_FINISH_STEP_MODE ",
                " T_ACTIVITY_OWNER_BY_CONDITION_CONDITION ",
                " T_ACTIVITY_OWNER_BY_CONDITION_CONDITION_GROUP ",
                " T_ACTIVITY_UI_COMPONENT ",
                " T_CALCULATED_FIELD_CONSTANT_TYPE ",
                " T_FIELD_DATE_CONSTRAINT ",
                " T_FIELD_EXTENSION_NUMBER 1 ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS ",
                " T_FIELD_TO_FIELD_DEPENDENCY ",
                " T_FIELD_TO_FIELD_DEPENDENCY_TYPE ",
                " T_FIELD_VALUE ",
                " T_FIELD_VALUE_TRANSLATION ",
                " T_GENERAL_DATA_PROTECTION_FIELD_TYPE ",
                " T_NOTIFICATION_RECIPIENT ",
                " T_NOTIFICATION_TYPE ",
                " T_PROC_DESIGN_DRAW ",
                " T_PROC_DESIGN_DRAW_PART ",
                " T_PROC_DESIGN_DRAW_PART_DETAIL ",
                " T_PROC_DESIGN_DRAW_PART_TYPE ",
                " T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE ",
                " T_REPORT_CALCULATED_FIELD_FORMULA_TREE_NODE_VALUE ",
                " T_REPORT_FIELD_UDT_COLUMNS ",
                " T_REPORT_FILTER ",
                " T_REPORT_GROUP ",
                " T_REPORT_REFERENCED_FIELD_LOCATION ",
                " T_REPORT_TYPE ",
                " T_SUBPROCESS_TYPE "
                };

            return firstRoundInsertTables;
        }

        public List<string> tablesInDBFileWithoutRow(ConnectionManagerST obj)
        {
            List<string> tablesWithoutRows = new List<string>();
            try
            {
                foreach (string tableName in getFirstRoundInsertTables())
                {
                    var reader =  obj.sqLiteDataReader(" Select count(*) from " + tableName + " where 1 = 1;");
                    {
                        if (reader.Read())
                        {

                            tablesWithoutRows.Add(tableName);
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

            return tablesWithoutRows;
        }
    }
}