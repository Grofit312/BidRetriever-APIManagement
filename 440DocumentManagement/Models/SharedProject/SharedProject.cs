namespace _440DocumentManagement.Models
{
	public class SharedProject : BaseModel
	{
		// Required
		public string project_id { get; set; }
		public string share_user_id { get; set; }
		public string share_source_company_id { get; set; }
		public string share_source_user_id { get; set; }

		// Optional
		public string shared_project_id { get; set; }
		public bool is_public { get; set; } = false;
		public string share_company_id { get; set; }
		public string share_office_id { get; set; }
		public string share_source_company_name { get; set; }
		public string share_source_user_displayname { get; set; }
		public string share_source_office_id { get; set; }
		public string status { get; set; }
	}
}
