namespace _440DocumentManagement.Models
{
	public class UserFavoriteFindRequest
	{
		public string user_id { get; set; }
		public string favorite_id { get; set; }
		public string favorite_type { get; set; }
		public string project_id { get; set; }
	}
}
