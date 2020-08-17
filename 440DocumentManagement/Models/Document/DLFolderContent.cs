namespace _440DocumentManagement.Models
{
	public class DLFolderContent : BaseModel
	{
		public string project_id { get; set; }
		public string folder_id { get; set; }
		public string folder_type { get; set; }
		public string doc_id { get; set; }
		public string file_id { get; set; }
		public string folder_path { get; set; }
		public string status { get; set; }
		public string folder_content_id { get; set; }
		public string folder_original_filename { get; set; }
		public string submission_id { get; set; }
	}
}
