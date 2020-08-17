namespace _440DocumentManagement.Models
{
	public class DLFolderContentDeleteRequest : BaseModel
	{
		public string project_id { get; set; }
		public string doc_id { get; set; }
		public string folder_type { get; set; }
		public string folder_path { get; set; }

		// Optional
		public string folder_content_id { get; set; }
	}
}
