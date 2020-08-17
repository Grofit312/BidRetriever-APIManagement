namespace _440DocumentManagement.Models
{
	public class ProjectSettingUpdateRequest : BaseModel
	{
		// Search Param
		public string search_project_setting_id { get; set; }

		// Update Params
		public string setting_desc { get; set; }
		public string setting_environment { get; set; }
		public string setting_group { get; set; }
		public string setting_help_link { get; set; }
		public string setting_name { get; set; }
		public string setting_sequence { get; set; }
		public string setting_tooltiptext { get; set; }
		public string setting_value { get; set; }
		public string setting_value_data_type { get; set; }
		public string status { get; set; }
	}
}
