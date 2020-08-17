namespace _440DocumentManagement.Models
{
	public class UserAddCompanyRequest : BaseModel
	{
		public string user_id { get; set; }
		public string customer_id { get; set; }
	}
}
