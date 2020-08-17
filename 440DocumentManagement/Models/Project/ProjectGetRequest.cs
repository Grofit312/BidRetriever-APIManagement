namespace _440DocumentManagement.Models.Project
{
	public class ProjectGetRequest : BaseModel
	{
		public string project_id { get; set; }
		public string detail_level { get; set; }
	}
}
