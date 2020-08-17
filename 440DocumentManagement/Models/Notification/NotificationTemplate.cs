namespace _440DocumentManagement.Models.Notification
{
	public class NotificationTemplate : BaseModel
	{
		// Required
		public string notification_type { get; set; }
		public string template_from_address { get; set; }
		public string template_html { get; set; }
		public string template_name { get; set; }
		public string template_subject_line { get; set; }

		// Optional
		public string notification_template_id { get; set; }
		public string status { get; set; }
		public string template_desc { get; set; }
		public string template_from_name { get; set; }
	}
}
