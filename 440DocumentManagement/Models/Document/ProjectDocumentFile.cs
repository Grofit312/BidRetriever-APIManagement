namespace _440DocumentManagement.Models
{
	public class ProjectDocumentFile : BaseModel
	{
		// Required

		public string doc_id { get; set; }
		public string file_id { get; set; }
		public string file_type { get; set; }
		public string file_size { get; set; }
		public string file_original_filename { get; set; }

		// Optional
		public string status { get; set; }
		public string bucket_name { get; set; }

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
