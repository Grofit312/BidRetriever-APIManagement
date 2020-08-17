namespace _440DocumentManagement.Models
{
	public class UserActivity : BaseModel
	{
		// Required
		public string activity_name { get; set; }
		public string application_name { get; set; }

		// Optional
		public string activity_data { get; set; }
		public string activity_datetime { get; set; }
		public string activity_level { get; set; }
		public string customer_id { get; set; }
		public string document_id { get; set; }
		public string file_id { get; set; }
		public string notification_id { get; set; }
		public string project_id { get; set; }
		public string user_activity_id { get; set; }
		public string user_id { get; set; }
	}
}
