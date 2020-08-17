namespace _440DocumentManagement.Models
{
	public class CompanyOfficeGetRequest
	{
		public string company_office_id { get; set; }
		public string detail_level { get; set; } = "basic";
	}
}
