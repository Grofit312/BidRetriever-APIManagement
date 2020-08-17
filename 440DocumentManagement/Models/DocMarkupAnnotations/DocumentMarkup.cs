using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.DocMarkupAnnotations
{
    public class DocumentMarkup :BaseModel
    {
        public string markup_id { get; set; }
        public string author_userid { get; set; }
        public string author_displayname { get; set; }
        public DateTime? create_datetime { get; set; }
        public DateTime? edit_datetime { get; set; }
        public string create_userid { get; set; }
        public string author_companyname { get; set; }
        public string doc_id { get; set; }
        public string edit_userid { get; set; }
        public string markup_name { get; set; }
        public string markup_description { get; set; }
        public string status { get; set; }
        public string parent_markup_id { get; set; }
        public string file_id { get; set; }
    }
}
