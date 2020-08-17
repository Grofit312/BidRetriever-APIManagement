namespace _440DocumentManagement.Models
{
	public class ProjectScopeOfWork : BaseModel
	{
		// Required
		public string file_id { get; set; }
		public int file_page_number { get; set; }
		public int file_page_x1 { get; set; }
		public int file_page_x2 { get; set; }
		public int file_page_y1 { get; set; }
		public int file_page_y2 { get; set; }
		public int match_end_char_index { get; set; }
		public int match_start_char_index { get; set; }
		public int match_start_sentence_index { get; set; }
		public int spec_pages { get; set; }
		public string match_sentence { get; set; }
		public string project_id { get; set; }
		public string section_code { get; set; }
		public string section_name { get; set; }
		public string section_id { get; set; }
	}
}
