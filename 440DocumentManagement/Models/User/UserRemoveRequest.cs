namespace _440DocumentManagement.Models
{
	public class UserRemoveRequest : BaseModel
	{
		public string customer_id { get; set; }
		public string user_id { get; set; }
	}
}
