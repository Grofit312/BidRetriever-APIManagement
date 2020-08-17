namespace _440DocumentManagement.Models
{
	public class DuplicateNameFindRequest : BaseModel
	{
		public string original_filename { get; set; }
		public string folder_path { get; set; } = "";
		public string project_id { get; set; }
	}
}
