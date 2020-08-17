namespace _440DocumentManagement.Models
{
	public class CustomerGetRequest : BaseModel
	{

		// Optional
		public string customer_domain { get; set; }
		public string customer_id { get; set; }
		public string customer_crm_id { get; set; }
		public string detail_level { get; set; }
	}
}
