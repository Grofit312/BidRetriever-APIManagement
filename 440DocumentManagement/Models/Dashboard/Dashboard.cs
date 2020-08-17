using _440DocumentManagement.Models;

namespace SDAPI.Models.Dashboard
{
	public class Dashboard : BaseModel
	{
		// Required
		public string customer_id { get; set; }
		public string dashboard_create_datetime { get; set; }
		public string dashboard_create_userid { get; set; }
		public string dashboard_edit_datetime { get; set; }
		public string dashboard_edit_userid { get; set; }
		public string dashboard_file_bucketname { get; set; }
		public string dashboard_file_key { get; set; }
		public string dashboard_id { get; set; }
		public string dashboard_start_datetime { get; set; }
		public string dashboard_status { get; set; }
		public string dashboard_template_id { get; set; }
		public string dashboard_type { get; set; }
		public int dashboard_version_number { get; set; }

		// Optional
		public string dashboard_end_datetime { get; set; }
		public string dashboard_name { get; set; }
		public string device_id { get; set; }
	}
}
