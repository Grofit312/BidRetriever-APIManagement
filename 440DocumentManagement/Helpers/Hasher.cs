namespace _440DocumentManagement.Helpers
{
	public class Hasher
	{
		public static string Create(string value)
		{
			return BCrypt.Net.BCrypt.HashPassword(value);
		}

		public static bool Validate(string value, string hash)
			=> BCrypt.Net.BCrypt.Verify(value, hash);
	}
}
