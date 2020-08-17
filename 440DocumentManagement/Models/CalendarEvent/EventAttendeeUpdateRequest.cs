namespace _440DocumentManagement.Models
{
	public class EventAttendeeUpdateRequest
	{
		// search parameter
		public string search_event_attendee_id { get; set; }

		// update parameters
		public string event_attendee_comment { get; set; }
		public string event_attendee_optional { get; set; }
		public string event_attendee_status { get; set; }
		public string calendar_event_id { get; set; }
		public string event_attendee_user_id { get; set; }
		public string status { get; set; }
	}
}
