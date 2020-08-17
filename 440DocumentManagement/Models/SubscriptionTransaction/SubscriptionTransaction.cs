namespace _440DocumentManagement.Models
{
	public class SubscriptionTransaction : BaseModel
	{
		// Required
		public string subscription_id { get; set; }
		public string transaction_datetime { get; set; }
		public string transaction_type { get; set; }
		public double transaction_amount { get; set; }
		public string transaction_status { get; set; }

		// Optional
		public string transaction_id { get; set; }
	}
}
