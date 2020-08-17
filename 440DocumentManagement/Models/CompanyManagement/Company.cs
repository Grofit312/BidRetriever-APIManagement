using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.CompanyManagement
{
	public class Company : BaseModel
	{
		public string company_id { get; set; }
		public string company_address1 { get; set; }
		public string company_address2 { get; set; }
		public string company_crm_id { get; set; }
		public string company_admin_user_id { get; set; }
		public string company_city { get; set; }
		public string company_country { get; set; }
		public string company_domain { get; set; }
		public string company_duns_number { get; set; }
		public string company_email { get; set; }
		public int? company_employee_number { get; set; }
		public string customer_id { get; set; }
		public string company_name { get; set; }
		public string company_phone { get; set; }
		public string company_photo_id { get; set; }
		public string company_record_source { get; set; }
		public string company_service_area { get; set; }
		public string company_state { get; set; }
		public string company_timezone { get; set; }
		public string company_type { get; set; }
		public string company_website { get; set; }
		public string company_zip { get; set; }
		public string company_status { get; set; }
		public string customer_office_id { get; set; }
		public string user_id { get; set; }
		public string company_logo_id { get; set; }
		public string company_revenue { get; set; }
	}
}
