﻿namespace _440DocumentManagement.Models
{
	public class DLProjectSubmission : BaseModel
	{
		// Required
		public string user_id { get; set; }
		public string submitter_email { get; set; }

		// Optional
		public string submission_id { get; set; }
		public string submission_name { get; set; }
		public string project_id { get; set; }
		public string project_name { get; set; }
		public string customer_id { get; set; }
		public string source_url { get; set; }
		public string source_sys_type_id { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public string inbound_email { get; set; }
		public string received_datetime { get; set; }
		public string project_number { get; set; }
		public string status { get; set; }
		public string user_timezone { get; set; }
		public string submission_process_status { get; set; }
		public string submission_process_message { get; set; }
		public string submission_email_file_bucket { get; set; }
		public string submission_email_file_key { get; set; }
		public string submission_type { get; set; }
	}
}
