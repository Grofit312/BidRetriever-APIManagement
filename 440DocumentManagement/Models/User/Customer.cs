namespace _440DocumentManagement.Models
{
	public class Customer : BaseModel
	{

		// Required
		public string customer_name { get; set; }


		// Optional
		public string customer_id { get; set; }
		public string customer_admin_user_id { get; set; }
		public string customer_email { get; set; }
		public string customer_photo_id { get; set; }
		public string customer_duns_number { get; set; }
		public string customer_phone { get; set; }
		public string customer_address1 { get; set; }
		public string customer_address2 { get; set; }
		public string customer_city { get; set; }
		public string customer_state { get; set; }
		public string customer_zip { get; set; }
		public string customer_country { get; set; }
		public string customer_service_area { get; set; }
		public string customer_crm_id { get; set; }
		public string customer_timezone { get; set; }
		public string status { get; set; }

		public string company_type { get; set; }
		public string company_website { get; set; }
		public string customer_domain { get; set; }
		public string record_source { get; set; }
	}
}
