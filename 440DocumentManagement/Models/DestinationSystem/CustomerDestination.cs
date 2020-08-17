namespace _440DocumentManagement.Models.DestinationSystem
{
	public class CustomerDestination : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string destination_type_id { get; set; }
		public string destination_url { get; set; }

		// Optional
		public string destination_access_token { get; set; }
		public string destination_id { get; set; }
		public string destination_name { get; set; }
		public string destination_password { get; set; }
		public string destination_root_path { get; set; }
		public string destination_type_name { get; set; }
		public string destination_username { get; set; }
		public string status { get; set; }
	}
}
