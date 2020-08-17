namespace _440DocumentManagement.Models.DestinationSystem
{
	public class CustomerDestinationFindRequest : BaseModel
	{
		// Required
		public string customer_id { get; set; }

		// Optional
		public string detail_level { get; set; }
	}
}
