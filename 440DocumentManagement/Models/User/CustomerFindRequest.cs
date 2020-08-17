namespace _440DocumentManagement.Models
{
	public class CustomerFindRequest
	{
		public string company_type { get; set; }
		public string customer_domain { get; set; }
		public string customer_name { get; set; }
		public string customer_service_area { get; set; }
		public string record_source { get; set; }
		public string customer_state { get; set; }
		public string customer_zip { get; set; }
		public string status { get; set; }

		public string detail_level { get; set; } = "basic";
	}
}
