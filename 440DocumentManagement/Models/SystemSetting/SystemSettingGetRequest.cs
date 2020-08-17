namespace _440DocumentManagement.Models
{
	public class SystemSettingGetRequest : BaseModel
	{
		public string system_setting_id { get; set; }
		public string setting_name { get; set; }
		public string detail_level { get; set; }
	}
}
