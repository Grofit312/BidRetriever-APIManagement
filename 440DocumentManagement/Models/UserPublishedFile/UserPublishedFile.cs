namespace _440DocumentManagement.Models
{
	public class UserPublishedFile : BaseModel
	{
		// Required
		public string folder_content_id { get; set; }
		public string publish_datetime { get; set; }
		public string publish_status { get; set; }
		public string published_file_hash { get; set; }
		public string published_filename { get; set; }
		public string user_device_id { get; set; }

		// Optional
		public string user_published_file_id { get; set; }
	}
}
