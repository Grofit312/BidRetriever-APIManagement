namespace _440DocumentManagement.Models
{
	public class AppTransactionLog
	{

		// Optional

		public string log_id { get; set; }
		public string submission_id { get; set; }
		public string routine_name { get; set; }
		public string routine_version { get; set; }
		public string function_name { get; set; }
		public string user_id { get; set; }
		public string customer_id { get; set; }
		public string project_id { get; set; }
		public string document_id { get; set; }
		public string file_id { get; set; }
		public string notification_id { get; set; }
		public string operation_name { get; set; }
		public string operation_datetime { get; set; }
		public string operation_status { get; set; }
		public string operation_status_desc { get; set; }
		public string operation_data { get; set; }
		public string device_id { get; set; }
		public string transaction_level { get; set; }
	}
}
