namespace _440DocumentManagement.Models
{
	public class SourceSystemType : BaseModel
	{
		// Required
		public string source_type_name { get; set; }
		public string source_type_desc { get; set; }

		// Optional
		public string source_sys_type_id { get; set; }
		public string source_type_domain { get; set; }
		public string source_type_url { get; set; }
		public string status { get; set; }
	}
}
