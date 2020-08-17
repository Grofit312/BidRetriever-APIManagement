namespace _440DocumentManagement.Models
{
	public class FileUpdateRequest
	{
		// Search Parameter
		public string search_file_id { get; set; }

		// Update Parameters
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
