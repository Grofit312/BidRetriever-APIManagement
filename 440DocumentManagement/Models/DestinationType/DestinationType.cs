namespace _440DocumentManagement.Models.DestinationType
{
	public class DestinationType : BaseModel
	{
		// Required
		public string destination_type_name { get; set; }

		// Optional
		public string destination_type_desc { get; set; }
		public string destination_type_domain { get; set; }
		public string destination_type_id { get; set; }
		public string status { get; set; }
	}
}
