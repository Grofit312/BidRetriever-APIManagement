namespace _440DocumentManagement.Models.Document
{
	public class ProjectDocumentFindRequest
	{
		// Required
		public string project_id { get; set; }

		// Optional
		public string submission_id { get; set; }
		public string customer_id { get; set; }
		public string doc_number { get; set; }
		public string doc_parent_id { get; set; }
		public bool latest_rev_only { get; set; }
		public string doc_type { get; set; }
		public string detail_level { get; set; }
		public string doc_size { get; set; }
	}
}
