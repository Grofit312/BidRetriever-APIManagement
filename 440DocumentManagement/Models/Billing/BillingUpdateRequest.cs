namespace _440DocumentManagement.Models
{
	public class BillingUpdateRequest : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string source_token { get; set; }
	}
}
