namespace _440DocumentManagement.Models
{
	public class CompanyOffice : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string company_office_name { get; set; }

		// Optional
		public string company_office_id { get; set; }
		public string company_office_admin_user_id { get; set; }
		public string company_office_address1 { get; set; }
		public string company_office_address2 { get; set; }
		public string company_office_city { get; set; }
		public string company_office_state { get; set; }
		public string company_office_zip { get; set; }
		public bool company_office_headoffice { get; set; }
		public string company_office_country { get; set; }
		public string company_office_timezone { get; set; }
		public string company_office_phone { get; set; }
		public string company_office_service_area { get; set; }
		public string status { get; set; }
	}
}
