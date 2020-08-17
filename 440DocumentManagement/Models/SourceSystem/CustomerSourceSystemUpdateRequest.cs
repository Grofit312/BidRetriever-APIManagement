namespace _440DocumentManagement.Models
{
	public class CustomerSourceSystemUpdateRequest : BaseModel
	{
		// Search parameter
		public string search_customer_source_sys_id { get; set; }

		// Update parameters (All Optional)
		public string customer_source_sys_name { get; set; }
		public string source_sys_type_id { get; set; }
		public string system_url { get; set; }
		public string source_sys_url { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public string access_token { get; set; }
		public string status { get; set; }

		/// Admin Only
		public string customer_id { get; set; }
		public int total_access_count { get; set; }
	}
}
