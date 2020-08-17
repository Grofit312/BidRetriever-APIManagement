namespace _440DocumentManagement.Models
{
	public class UserSettingsGetRequest : BaseModel
	{
		// Required
		public string user_id { get; set; }

		// Optional
		public string user_setting_id { get; set; }
		public string setting_name { get; set; }
		public string detail_level { get; set; }
	}
}
