namespace _440DocumentManagement.Models
{
	public class UserLoginRequest : BaseModel
	{
		public string user_email { get; set; }
		public string user_password { get; set; }
	}
}
