namespace _440DocumentManagement.Models
{
	public class BillingItem
	{
		public string customer_id { get; set; }
		public string customer_billing_id { get; set; }
		public string user_email { get; set; }
		public string billable_month { get; set; }
		public string project_name { get; set; }
		public int total_new_documents { get; set; }
		public string project_size { get; set; }
		public double project_cost { get; set; }
	}
}
