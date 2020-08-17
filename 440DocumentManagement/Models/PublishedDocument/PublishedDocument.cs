namespace _440DocumentManagement.Models
{
	public class PublishedDocument : BaseModel
	{
		// Required
		public string doc_id { get; set; }
		public string file_id { get; set; }
		public string project_id { get; set; }
		public string customer_id { get; set; }
		public string customer_name { get; set; }
		public string destination_name { get; set; }
		public string destination_url { get; set; }
		public string destination_username { get; set; }
		public string destination_folder_path { get; set; }
		public string destination_file_name { get; set; }
		public string publish_datetime { get; set; }
		public string publish_status { get; set; }
		public string destination_sys_type_id { get; set; }
		public string file_original_filename { get; set; }
		public string bucket_name { get; set; }

		// Optional
		public string doc_publish_id { get; set; }
		public string project_name { get; set; }
		public string submission_id { get; set; }
		public string submission_datetime { get; set; }
		public string submitter_id { get; set; }
		public string destination_id { get; set; }
		public string destination_file_size { get; set; }
		public string destination_transfer_time { get; set; }
		public string destination_sys_type_name { get; set; }
		public string doc_name { get; set; }
		public string doc_number { get; set; }
		public string doc_revision { get; set; }
		public string doc_discipline { get; set; }
		public string status { get; set; }
	}
}
