namespace _440DocumentManagement.Models
{
	public class ValidSheetNumber : BaseModel
	{
		// Required
		public string sheet_number { get; set; }
		public bool ocr { get; set; }
		public bool manual { get; set; }
	}
}
