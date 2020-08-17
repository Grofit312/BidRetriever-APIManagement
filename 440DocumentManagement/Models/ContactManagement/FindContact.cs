using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.ContactManagement
{
    public class FindContact : BaseModel
{
        public string company_id { get; set; }
        public string contact_lastname { get; set; }
        public string customer_id { get; set; }
        public string customer_office_id { get; set; }
        public string detail_level { get; set; }
        public string user_id { get; set; }
    }
}
