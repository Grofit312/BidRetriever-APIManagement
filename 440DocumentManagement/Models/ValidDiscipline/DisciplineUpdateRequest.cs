namespace _440DocumentManagement.Models
{
	public class DisciplineUpdateRequest
	{
		public string search_discipline_id { get; set; }
		public string search_discipline_name { get; set; }

		public int confidence { get; set; }
		public string discipline_name { get; set; }
		public string discipline_prefix { get; set; }
		public int occurances { get; set; }
		public string status { get; set; }
	}
}
