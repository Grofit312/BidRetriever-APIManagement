namespace _440DocumentManagement.Models
{
	public class SubscriptionUpdateRequest : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string core_product_id { get; set; }
		public string license_product_id { get; set; }
		public int license_count { get; set; }
	}
}
