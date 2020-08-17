namespace _440DocumentManagement.Models
{
	public class CalendarEventFindRequest
	{
		public string calendar_event_id { get; set; }
		public string company_id { get; set; }
		public string company_office_id { get; set; }
		public string user_id { get; set; }
		public string project_id { get; set; }
		public string type { get; set; }
		public string start_datetime { get; set; }
		public string end_datetime { get; set; }
		public string status { get; set; }
	}
}
