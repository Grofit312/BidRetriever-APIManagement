namespace _440DocumentManagement.Models
{
	public class ValidSheetNameWord : BaseModel
	{
		// Required
		public string sheet_name_word { get; set; }
		public string sheet_name_word_abbrv { get; set; }
		public bool ocr { get; set; } = false;
		public bool manual { get; set; } = false;
	}
}
