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
                " T_FIELD_EXTENSION_NUMBER ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_ACTIVATION_ACTIVITY ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENCY_CONDITION_FORMULA ",
                " T_FIELD_GROUP_TO_FIELD_GROUP_DEPENDENT_FIELDS ",
                " T_FIELD_TO_FIELD_DEPENDENCY ",
                " T_FIELD_TO_FIELD_DEPENDENCY_TYPE ",
                " T_FIELD_VALUE ",
                " T_FIELD_VALUE_TRANSLATION ",
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
                " T_REPORT_TYPE "
                };

            return firstRoundInsertTables;
        }

        public List<string> tablesInDBFileWithoutRow(ConnectionManagerST obj, string tableName)
        {
            List<string> tablesWithoutRows = new List<string>();
            try
            {
             
                 var reader = obj.sqLiteDataReader("SELECT count(*) from" + tableName);

                while (reader.Read())
                {
                    if (reader[0].ToString() == "0")
                    {
                        tablesWithoutRows.Add(tableName);

                    }

                }
               
            }
            catch (Exception ex)
            {
                throw ex;

            }

            return tablesWithoutRows;
        }
        public List<string> firstRoundTablesWithContent(ConnectionManagerST obj)
        {
            List<string> allFirstRoundTables = getFirstRoundInsertTables();
            List<string> firstRoundTablesWithContent = new List<string>();
            List<string> firstRoundTablesWithOutContent = new List<string>();
            foreach (string tableName in allFirstRoundTables)
            {
                firstRoundTablesWithOutContent.AddRange(tablesInDBFileWithoutRow(obj, tableName));

            }
            firstRoundTablesWithContent.AddRange(allFirstRoundTables.Except(firstRoundTablesWithOutContent).ToList());

            return firstRoundTablesWithContent;
        }

      //  public List<string> getAllTableWhereIdExits(ConnectionManagerST obj)
       // {
            //  List<string> tables
            //select table_name from table_information where column_name like '%Process_Id%';
       // }
    }
}