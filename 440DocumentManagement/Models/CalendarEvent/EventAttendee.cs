namespace _440DocumentManagement.Models
{
	public class EventAttendee : BaseModel
	{
		// Required
		public string calendar_event_id { get; set; }
		public string event_attendee_user_id { get; set; }

		// Optional
		public string event_attendee_comment { get; set; }
		public string event_attendee_id { get; set; }
		public string event_attendee_optional { get; set; }
		public string event_attendee_status { get; set; }
        public string event_attendee_type { get; set; }
		public string status { get; set; }
	}
}
