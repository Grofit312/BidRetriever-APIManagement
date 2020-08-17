namespace _440DocumentManagement.Models
{
	public class UserGetRequest : BaseModel
	{
		// Optional

		public string user_id { get; set; }
		public string user_email { get; set; }
		public string user_crm_id { get; set; }
		public string detail_level { get; set; }
	}
}
