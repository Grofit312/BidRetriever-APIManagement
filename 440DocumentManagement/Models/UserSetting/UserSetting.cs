namespace _440DocumentManagement.Models
{
	public class UserSetting : BaseModel
	{
		// Required
		public string user_id { get; set; }
		public string setting_name { get; set; }
		public string setting_value { get; set; }
		public string setting_value_data_type { get; set; }

		// Optional
		public string user_device_id { get; set; }
		public string user_setting_id { get; set; }
		public string setting_desc { get; set; }
		public string setting_help_link { get; set; }
		public string setting_group { get; set; }
		public string setting_sequence { get; set; }
		public string setting_tooltiptext { get; set; }
		public string status { get; set; }
	}
}
