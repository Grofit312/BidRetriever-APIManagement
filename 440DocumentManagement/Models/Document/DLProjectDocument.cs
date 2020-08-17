namespace _440DocumentManagement.Models
{
	public class DLProjectDocument : BaseModel
	{
		// Required

		public string project_id { get; set; }
		public string submission_id { get; set; }
		public string submission_datetime { get; set; }

		// Optional
		public string doc_type { get; set; }
		public string doc_id { get; set; }
		public string doc_parent_id { get; set; }
		public string doc_name { get; set; }
		public string doc_name_abbrv { get; set; }
		public string doc_number { get; set; }
		public string doc_revision { get; set; }
		public string project_doc_original_filename { get; set; }
		public string status { get; set; }
		public string process_status { get; set; }
		public string display_name { get; set; }
		public string doc_size { get; set; }
	}
}
