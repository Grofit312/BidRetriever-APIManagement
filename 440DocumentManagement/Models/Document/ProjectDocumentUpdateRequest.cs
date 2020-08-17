namespace _440DocumentManagement.Models
{
	public class ProjectDocumentUpdateRequest
	{
		// Search Parameter
		public string search_project_document_id { get; set; }

		// Update Parameters
		public string doc_name { get; set; }
		public string doc_name_abbrv { get; set; }
		public string doc_number { get; set; }
		public string doc_version { get; set; }
		public string doc_discipline { get; set; }
		public string doc_desc { get; set; }
		public string doc_type { get; set; }
		public string status { get; set; }
		public string process_status { get; set; }
		public string display_name { get; set; }
		public string doc_size { get; set; }
		public string doc_pagenumber { get; set; }
		public int? doc_sequence { get; set; }
		public string doc_subproject { get; set; }

		// Update Admin Parameters
		public string doc_revision { get; set; }
		public string doc_next_rev { get; set; }
	}
}
