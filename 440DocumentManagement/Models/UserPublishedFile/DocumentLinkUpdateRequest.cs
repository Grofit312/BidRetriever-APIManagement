namespace _440DocumentManagement.Models
{
	public class DocumentLinkUpdateRequest
	{
		public string search_document_link_id { get; set; }

		// Update Parameters
		public string link_name { get; set; }
		public string status { get; set; }
	}
}
