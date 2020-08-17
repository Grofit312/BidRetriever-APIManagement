namespace _440DocumentManagement.Models
{
	public class SourceSystemTypeUpdateRequest
	{
		// Search Parameter
		public string search_source_type_id { get; set; }

		// Update Parameters
		public string source_type_name { get; set; }
		public string source_type_desc { get; set; }
		public string source_type_domain { get; set; }
		public string source_type_url { get; set; }
		public string status { get; set; }

		public int total_access_count { get; set; }
		public string last_synch_datetime { get; set; }
	}
}
