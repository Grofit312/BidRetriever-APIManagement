namespace _440DocumentManagement.Models
{
	public class UserPublishedCurrentPlansGetRequest
	{
		// Optional
		public string folder_id { get; set; }
		public string project_id { get; set; }

		// Required
		public string user_device_id { get; set; }
	}
}
