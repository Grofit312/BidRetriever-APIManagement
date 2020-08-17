namespace _440DocumentManagement.Models
{
	public class SourceFilesFindRequest
	{
		public string project_id { get; set; }
		public string detail_level { get; set; } = "basic";
	}
}
