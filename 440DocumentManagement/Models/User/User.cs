namespace _440DocumentManagement.Models
{
	public class User : BaseModel
	{
		// Required
		public string user_email { get; set; }


		// Optional
		public string user_id { get; set; }
		public string user_firstname { get; set; }
		public string user_lastname { get; set; }
		public string user_phone { get; set; }
		public string user_address1 { get; set; }
		public string user_address2 { get; set; }
		public string user_city { get; set; }
		public string user_state { get; set; }
		public string user_zip { get; set; }
		public string user_country { get; set; }
		public string user_crm_id { get; set; }
		public string user_username { get; set; }
		public string user_password { get; set; }
		public string status { get; set; }
		public string user_photo_id { get; set; }
		public string user_role { get; set; }
		public string customer_office_id { get; set; }
	}
}
