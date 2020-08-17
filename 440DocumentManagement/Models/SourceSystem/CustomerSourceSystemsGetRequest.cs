namespace _440DocumentManagement.Models
{
	public class CustomerSourceSystemsGetRequest : BaseModel
	{
		// Required
		public string customer_id { get; set; }

		// Optional
		public string customer_source_sys_id { get; set; }
		public string detail_level { get; set; }
	}
}
