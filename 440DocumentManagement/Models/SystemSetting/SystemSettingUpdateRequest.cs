namespace _440DocumentManagement.Models
{
	public class SystemSettingUpdateRequest
	{
		public string search_system_setting_id { get; set; }

		// Update Parameters
		public string setting_name { get; set; }
		public string setting_value { get; set; }
		public string setting_value_data_type { get; set; }
		public string setting_tooltiptext { get; set; }
		public string setting_desc { get; set; }
		public string setting_group { get; set; }
		public int setting_sequence { get; set; }
		public string setting_help_link { get; set; }
		public string setting_environment { get; set; }
		public string status { get; set; }
	}
}
