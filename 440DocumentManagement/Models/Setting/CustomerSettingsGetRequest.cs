namespace _440DocumentManagement.Models
{
	public class CustomerSettingsGetRequest : BaseModel
	{
		// required
		public string customer_id { get; set; }

		// optional
		public string setting_id { get; set; }
		public string setting_name { get; set; }
		public string detail_level { get; set; }
	}
}
