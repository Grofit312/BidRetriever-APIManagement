namespace _440DocumentManagement.Models
{
	public class DocumentLink : BaseModel
	{
		// Required
		public string primary_doc_id { get; set; }
		public string linked_doc_id { get; set; }

		// Optional
		public string document_link_id { get; set; }
		public string link_name { get; set; }
		public string return_link_name { get; set; }
		public string status { get; set; }
	}
}
