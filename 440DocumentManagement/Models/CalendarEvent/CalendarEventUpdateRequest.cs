namespace _440DocumentManagement.Models
{
	public class CalendarEventUpdateRequest
	{
		// Search parameter
		public string search_calendar_event_id { get; set; }

		// Update parameters
		public string calendar_event_company_office_id { get; set; }
		public string calendar_event_color_id { get; set; }
		public string calendar_event_desc { get; set; }
		public string calendar_event_end_datetime { get; set; }
		public string calendar_event_name { get; set; }
		public string calendar_event_organizer_user_id { get; set; }
		public string calendar_event_organizer_company_id { get; set; }
		public string calendar_event_organizer_company_office_id { get; set; }
		public string calendar_event_source_user_id { get; set; }
		public string calendar_event_source_company_id { get; set; }
		public string calendar_event_source_company_office_id { get; set; }
		public string calendar_event_start_datetime { get; set; }
		public string calendar_event_status { get; set; }
		public string calendar_event_type { get; set; }
		public string calendar_event_location { get; set; }
		public string google_id { get; set; }
		public string icaluid { get; set; }
		public string outlook_id { get; set; }
		public string project_id { get; set; }
		public string status { get; set; }
	}
}
