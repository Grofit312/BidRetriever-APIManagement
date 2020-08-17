namespace _440DocumentManagement.Models
{
	public class UserRegistrationRequest : BaseModel
	{
		// All required
		public string user_email { get; set; }
		public string user_password { get; set; }
		public string user_firstname { get; set; }
		public string user_lastname { get; set; }
		public string customer_name { get; set; }
	}
}
