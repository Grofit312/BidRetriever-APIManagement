namespace _440DocumentManagement.Models
{
	public class DLFile : BaseModel
	{
		// Required

		public string file_id { get; set; }
		public string file_type { get; set; }
		public string file_size { get; set; }
		public string file_key { get; set; }

		// Optional
		public string file_original_filename { get; set; }
		public string bucket_name { get; set; }
		public string standard_doc_number { get; set; }
		public string status { get; set; }

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
