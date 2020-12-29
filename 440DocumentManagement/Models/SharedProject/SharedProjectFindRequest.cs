namespace _440DocumentManagement.Models
{
	public class SharedProjectFindRequest
	{
		public string detail_level { get; set; } = "basic";
		public string shared_project_id { get; set; }
		public string is_public { get; set; }
		public string status { get; set; } = "active";
		public string share_company_id { get; set; }
		public string share_office_id { get; set; }
		public string share_user_email { get; set; }
		public string share_user_id { get; set; }
		public string share_source_company_id { get; set; }
		public string share_source_office_id { get; set; }
        public string share_source_user_email { get; set; }
        public string share_source_user_id { get; set; }
		public string project_id { get; set; }
	}
}
