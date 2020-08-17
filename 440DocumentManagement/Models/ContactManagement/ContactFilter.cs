using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.ContactManagement
{
    public class ContactFilter : BaseModel
{
        public string search_contact_id { get; set; }
        public string contact_email { get; set; }
        public string contact_firstname { get; set; }
        public string contact_lastname { get; set; }
        public string contact_phone { get; set; }
        public string contact_address1 { get; set; }
        public string contact_address2 { get; set; }
        public string contact_city { get; set; }
        public string contact_state { get; set; }
        public string contact_zip { get; set; }
        public string contact_country { get; set; }
        public string contact_crm_id { get; set; }
        public string contact_photo_id { get; set; }
        public string customer_id { get; set; }
        public string contact_status { get; set; }
    }
}
