using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.SystemAttribute
{
    public class SystemAttribute : BaseModel
{
        public string system_attribute_datatype { get; set; }
        public string system_attribute_desc { get; set; }
        public string system_attribute_displayname { get; set; }
        public string system_attribute_id { get; set; }
        public string system_attribute_name { get; set; }
        public string system_attribute_status { get; set; }
        public string system_attribute_type { get; set; }
        public string customer_id { get; set; }
        public string default_alignment { get; set; }
        public string default_format { get; set; }
        public string default_heading { get; set; }
        public string default_width { get; set; }
    }
}
