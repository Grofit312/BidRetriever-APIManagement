namespace _440DocumentManagement.Models
{
	public class SharedProjectUpdateRequest
	{
		public string search_shared_project_id { get; set; }

		public bool is_public { get; set; }
		public string share_office_id { get; set; }
		public string share_user_id { get; set; }
		public string status { get; set; }
		public string share_type { get; set; }
	}
}
