namespace _440DocumentManagement.Models
{
	public class BillingRegisterRequest : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string user_email { get; set; }
		public string source_token { get; set; }
	}
}
