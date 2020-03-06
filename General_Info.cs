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
    }
}