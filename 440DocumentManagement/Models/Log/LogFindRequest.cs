namespace _440DocumentManagement.Models
{
	public class LogFindRequest : BaseModel
	{
		public string doc_id { get; set; }
		public string file_id { get; set; }
		public string function_name { get; set; }
		public string operation_name { get; set; }
		public string operation_status { get; set; }
		public string project_id { get; set; }
		public string submission_id { get; set; }
		public string detail_level { get; set; }
		public int offset { get; set; } = 0;
		public int limit { get; set; } = 100;
		public bool desc { get; set; } = true;
	}
}
