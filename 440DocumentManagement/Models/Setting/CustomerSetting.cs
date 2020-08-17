namespace _440DocumentManagement.Models
{
	public class CustomerSetting : BaseModel
	{
		// required
		public string setting_id { get; set; }
		public string setting_name { get; set; }
		public string setting_value { get; set; }
		public string setting_value_data_type { get; set; }

		// optional
		public string customer_id { get; set; }
		public string setting_desc { get; set; }
		public string setting_help_link { get; set; }
		public string setting_group { get; set; }
		public string setting_sequence { get; set; }
		public string setting_tooltiptext { get; set; }
		public string status { get; set; }
	}
}
