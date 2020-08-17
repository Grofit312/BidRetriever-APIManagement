using _440DocumentManagement.Models;

namespace SDAPI.Models.Dashboard
{
	public class Device : BaseModel
	{
		// Required
		public string device_mac_address { get; set; }
		public string device_serial_number { get; set; }
		public string device_type_id { get; set; }

		// Optional
		public string customer_id { get; set; }
		public string device_name { get; set; }
		public string device_night_end_time { get; set; }
		public string device_night_start_time { get; set; }
		public string device_status { get; set; }
		public string device_timezone { get; set; }
		public long device_update_frequency { get; set; } = 0;
		public string device_wireless_password { get; set; }
		public string device_wireless_ssid { get; set; }
		public string user_email { get; set; }
	}
}
