namespace _440DocumentManagement.Models
{
	public class DLDocumentFile : BaseModel
	{
		// Required
		public string file_id { get; set; }
		public string doc_id { get; set; }


		// Optional
		public string doc_file_id { get; set; }
	}
}
