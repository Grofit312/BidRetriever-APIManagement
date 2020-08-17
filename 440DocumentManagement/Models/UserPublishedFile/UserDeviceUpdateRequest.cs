namespace _440DocumentManagement.Models
{
	public class UserDeviceUpdateRequest
	{
		public string search_user_device_id { get; set; }

		// Update Parameters
		public string device_name { get; set; }
		public string device_description { get; set; }
		public string device_type { get; set; }
		public string device_last_update_datetime { get; set; }
		public string device_local_root_path { get; set; }
		public string device_night_start_time { get; set; }
		public string device_night_end_time { get; set; }
		public int device_update_count { get; set; } = -1;
		public int device_last_seq_num { get; set; } = -1;
		public string physical_device_id { get; set; }
		public string user_id { get; set; }
		public string status { get; set; }
	}
}
