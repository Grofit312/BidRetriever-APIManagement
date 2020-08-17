namespace _440DocumentManagement.Models.Notification
{
	public class NotificationUpdateRequest
	{
		// Search
		public string search_notification_template_id { get; set; }

		// Update Parameters
		public string notification_type { get; set; }
		public string status { get; set; }
		public string template_desc { get; set; }
		public string template_from_address { get; set; }
		public string template_from_name { get; set; }
		public string template_html { get; set; }
		public string template_name { get; set; }
		public string template_subject_line { get; set; }

		public string notification_template_id { get; set; }
	}
}
