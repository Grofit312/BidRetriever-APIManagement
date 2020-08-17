namespace _440DocumentManagement.Models
{
	public class UserNotificationFindRequest : BaseModel
	{
		public string customer_id { get; set; }
		public string project_id { get; set; }
		public string notification_send_datetime { get; set; }
		public string submission_id { get; set; }
		public string user_id { get; set; }
	}
}
