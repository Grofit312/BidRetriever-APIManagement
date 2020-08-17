namespace _440DocumentManagement.Models
{
	public class PublishedDocumentGetRequest
	{
		public string project_id { get; set; }
		public string doc_id { get; set; }
		public string submission_id { get; set; }
		public string file_id { get; set; }
		public string customer_id { get; set; }
		public string detail_level { get; set; }
	}
}
