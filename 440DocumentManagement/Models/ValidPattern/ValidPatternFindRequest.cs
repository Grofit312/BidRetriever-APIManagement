namespace _440DocumentManagement.Models
{
	public class ValidPatternFindRequest
	{
		// Optional
		public string pattern_class { get; set; }
        public int min_occurrences { get; set; } = 0;
		// Optional
		public int? limit { get; set; }
	}
}
