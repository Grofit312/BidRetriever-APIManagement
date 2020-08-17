namespace _440DocumentManagement.Models.DestinationSystem
{
	public class CustomerDestinationUpdateRequest
	{
		// Search parameter
		public string search_destination_id { get; set; }

		// Update parameters
		public string destination_access_token { get; set; }
		public string destination_name { get; set; }
		public string destination_password { get; set; }
		public string destination_root_path { get; set; }
		public string destination_type_id { get; set; }
		public string destination_url { get; set; }
		public string destination_username { get; set; }
		public string status { get; set; }

		// Update parameters (Admin)
		public string customer_id { get; set; }
		public int total_access_count { get; set; }
	}
}
