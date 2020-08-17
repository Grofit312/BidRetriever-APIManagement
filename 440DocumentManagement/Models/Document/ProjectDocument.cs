namespace _440DocumentManagement.Models
{
	public class ProjectDocument : BaseModel
	{
		// Required
		public string file_id { get; set; }
		public string project_id { get; set; }
		public string file_size { get; set; }
		public string file_original_filename { get; set; }
		public string status { get; set; }
		public string submission_id { get; set; }
		public string submission_datetime { get; set; }

		// Optional
		public string doc_type { get; set; }
		public string doc_id { get; set; }
		public string folder_path { get; set; }
		public string doc_number { get; set; }
		public string doc_parent_id { get; set; }
		public string bucket_name { get; set; }
		public string doc_name { get; set; }
		public string doc_name_abbrv { get; set; }
		public string doc_version { get; set; }
		public string doc_revision { get; set; }
		public string doc_next_rev { get; set; }
		public string doc_description { get; set; }
		public string doc_discipline { get; set; }
		public string create_datetime { get; set; }
		public string create_user_id { get; set; }
		public string edit_user_id { get; set; }
		public string edit_datetime { get; set; }
		public string process_status { get; set; }
		public string display_name { get; set; }
		public string doc_size { get; set; }

		public string file_original_application { get; set; }
		public string file_original_author { get; set; }
		public string file_original_create_datetime { get; set; }
		public string file_original_modified_datetime { get; set; }
		public string file_original_document_title { get; set; }
		public string file_original_pdf_version { get; set; }
		public string parent_original_create_datetime { get; set; }
		public string parent_original_modified_datetime { get; set; }
		public string file_original_document_bookmark { get; set; }
	}
}
