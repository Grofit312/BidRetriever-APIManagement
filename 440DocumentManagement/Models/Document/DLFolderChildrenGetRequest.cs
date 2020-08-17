namespace _440DocumentManagement.Models
{
	public class DLFolderChildrenGetRequest
	{
		public string project_id { get; set; }
		public string folder_id { get; set; }
		public string folder_type { get; set; }
		public string submission_id { get; set; }
		public string detail_level { get; set; }
	}
}
