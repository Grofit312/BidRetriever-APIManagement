namespace _440DocumentManagement.Models
{
	public class SubmitDropBoxFilesRequest : BaseModel
	{
		// Required
		public string submission_id { get; set; }
		public string submitter_email { get; set; }
		public string dropbox_url { get; set; }

		// Optional
		public string submission_datetime { get; set; }
		public string source_system { get; set; }
		public string project_id { get; set; }
		public string project_name { get; set; }

	}

	public class SubmitBoxFilesRequest : BaseModel
	{
		// Required
		public string submission_id { get; set; }
		public string submitter_email { get; set; }
		public string box_url { get; set; }

		// Optional
		public string submission_datetime { get; set; }
		public string source_system { get; set; }
		public string project_id { get; set; }
		public string project_name { get; set; }

		// maybe temporary
		public string access_token { get; set; }
	}
}
