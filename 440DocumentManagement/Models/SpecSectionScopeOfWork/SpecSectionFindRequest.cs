namespace _440DocumentManagement.Models
{
	public class SpecSectionFindRequest
	{
		public string division_number { get; set; }
		public string section_number { get; set; }
		public string start_section_number { get; set; }
		public string end_section_number { get; set; }
		public string sort_field { get; set; }
		public string section_type { get; set; }
	}
}
