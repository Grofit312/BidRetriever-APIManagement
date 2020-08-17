namespace _440DocumentManagement.Models
{
	public class UserNotification : BaseModel
	{
		// Required
		public string user_email { get; set; }

		// Optional
		public string create_user_id { get; set; }
		public string user_notification_id { get; set; }
		public string user_id { get; set; }
		public string notification_actual_from_address { get; set; }
		public string notification_actual_from_name { get; set; }
		public string notification_actual_html { get; set; }
		public string notification_actual_subject { get; set; }
		public string notification_name { get; set; }
		public string notification_send_datetime { get; set; }
		public string notification_template_id { get; set; }
		public string status { get; set; }
		public string project_id { get; set; }
		public string submission_id { get; set; }
		public string customer_id { get; set; }
		public string customer_name { get; set; }
	}
}
