namespace SDAPI.Models.Dashboard
{
	public class DashboardGetRequest
	{
		// Required
		public string customer_id { get; set; }

		// Optional
		public string device_id { get; set; }
		public bool new_dashboards { get; set; } = true;
	}
}
