namespace _440DocumentManagement.Models
{
	public class UserDevice : BaseModel
	{
		// Required
		public string device_name { get; set; }
		public string device_type { get; set; }
		public string user_id { get; set; }
		public string physical_device_id { get; set; }

		// Optional
		public string device_description { get; set; }
		public string device_last_update_datetime { get; set; }
		public string device_local_root_path { get; set; }
		public string device_night_end_time { get; set; }
		public string device_night_start_time { get; set; }
		public int device_update_count { get; set; } = 0;
		public string user_device_id { get; set; }
		public string status { get; set; }
	}
}
