namespace _440DocumentManagement.Models
{
	public class CustomerSourceSystem : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string customer_source_sys_name { get; set; }
		public string source_sys_type_id { get; set; }


		// Optional
		public string customer_source_sys_id { get; set; }
		public string system_url { get; set; }
		public string source_sys_url { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public string access_token { get; set; }
		public string status { get; set; }
	}
}
