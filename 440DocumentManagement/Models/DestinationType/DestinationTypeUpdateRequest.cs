namespace _440DocumentManagement.Models.DestinationType
{
	public class DestinationTypeUpdateRequest : BaseModel
	{
		// Search parameter
		public string search_destination_type_id { get; set; }

		// Update parameters
		public string destination_type_name { get; set; }
		public string destination_type_domain { get; set; }
		public string destination_type_desc { get; set; }
		public string status { get; set; }
	}
}
