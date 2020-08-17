namespace _440DocumentManagement.Models
{
	public class SpecSectionScopeOfWork : BaseModel
	{
		// Required
		public string division_name { get; set; }
		public string division_number { get; set; }
		public int search_matches { get; set; }
		public string search_string { get; set; }
		public string section_name { get; set; }

		// Optional
		public string section_id { get; set; }
		public string csi_spec_number { get; set; }
		public string section_number { get; set; }
		public string section_type { get; set; }
		public string csi_95_search_string { get; set; }
		public string status { get; set; }
	}
}
