namespace _440DocumentManagement.Models.DestinationType
{
	public class DestinationTypeGetRequest : BaseModel
	{
		// Required
		public string destination_type_id { get; set; }

		// Optional
		public string detail_level { get; set; }
	}
}
