namespace _440DocumentManagement.Models
{
	public class UserFavorite : BaseModel
	{
		// Required
		public string favorite_id { get; set; }
		public string favorite_type { get; set; }
		public string user_id { get; set; }

		// Optional
		public string project_id { get; set; }
		public string user_favorite_id { get; set; }
		public string status { get; set; }

		public string file_id { get; set; }
		public string favorite_name { get; set; }
		public string favorite_displayname { get; set; }
	}
}
