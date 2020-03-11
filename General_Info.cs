using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Process_Export_Import
{
    public class General_Info
    {
        public List<long> process_design_ids = new List<long>();

        public List<int> subProcess_id_with_existing_desgn = new List<int>();

        public List<int> distinct_field_condition_group_id = new List<int>();

        public List<string> info_list = new List<string>();

        public List<KeyValuePair<int, bool>> field_condition_group_id_list = new List<KeyValuePair<int, bool>>();

        public  List<long> used_field_for_user_defined_table = new List<long>();

        public  List<long> used_field_for_subprocess = new List<long>();

        public  List<long> used_field_for_procfield_word_merge = new List<long>();

        public  List<long> used_field_for_word_merge = new List<long>();

        public List<long> field_for_processes_uniq_list = new List<long>();

        public List<long> system_interface_id_list = new List<long>();
    }
}