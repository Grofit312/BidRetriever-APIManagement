namespace _440DocumentManagement.Models
{
	public class ProjectSubmissionFindRequest
	{
		// Optional parameters
		public string project_id { get; set; }
		public string submission_process_status { get; set; }
		public string user_id { get; set; }
		public string customer_id { get; set; }
		public string office_id { get; set; }
		public string detail_level { get; set; }
	}
}
