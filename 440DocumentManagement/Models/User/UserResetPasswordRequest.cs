namespace _440DocumentManagement.Models
{
	public class UserResetPasswordRequest : BaseModel
	{
		public string token { get; set; }
		public string user_password { get; set; }
	}
}
