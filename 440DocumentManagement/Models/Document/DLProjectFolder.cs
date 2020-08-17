namespace _440DocumentManagement.Models
{
	public class DLProjectFolder : BaseModel
	{
		// Required

		public string project_id { get; set; }
		public string folder_name { get; set; }
		public string folder_type { get; set; }


		// Optional

		public string folder_id { get; set; }
		public string parent_folder_id { get; set; }
		public string status { get; set; }
	}
}
