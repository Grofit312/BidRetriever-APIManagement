namespace _440DocumentManagement.Models
{
	public class ProjectSettingsGetRequest : BaseModel
	{
		// Required
		public string project_id { get; set; }

		// Optional
		public string project_setting_id { get; set; }
		public string setting_name { get; set; }
		public string detail_level { get; set; }
	}
}
