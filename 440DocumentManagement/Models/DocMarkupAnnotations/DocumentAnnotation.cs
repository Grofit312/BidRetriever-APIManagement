using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.DocMarkupAnnotations
{
    public class DocumentAnnotation : BaseModel
    {
        public string annotation_id { get; set; }
        public string annotation_type { get; set; }
        public DateTime? create_datetime { get; set; }
        public string create_userid { get; set; }
        public DateTime? edit_datetime { get; set; }
        public string edit_userid { get; set; }
        public string markup_id { get; set; }
        public string annotation_current_data { get; set; }
        public string annotation_status { get; set; }
        public string parent_annotation_id { get; set; }
    }
}