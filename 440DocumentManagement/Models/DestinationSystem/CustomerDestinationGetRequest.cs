namespace _440DocumentManagement.Models.DestinationSystem
{
	public class CustomerDestinationGetRequest : BaseModel
	{
		// Required
		public string destination_id { get; set; }

		// Optional
		public string detail_level { get; set; }
	}
}
