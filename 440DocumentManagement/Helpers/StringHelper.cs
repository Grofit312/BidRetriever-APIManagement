using System;

namespace _440DocumentManagement.Helpers
{
	public class StringHelper
	{
		static public string GetFileName(string fullFileName)
		{
			var dotIndex = fullFileName.LastIndexOf(".");

			if (dotIndex >= 0)
			{
				return fullFileName.Substring(0, dotIndex);
			}
			else
			{
				return fullFileName;
			}
		}

		static public string GetFileExtension(string fullFileName)
		{
			var dotIndex = fullFileName.LastIndexOf(".");

			if (dotIndex >= 0)
			{
				try
				{
					return fullFileName.Substring(dotIndex + 1);
				}
				catch (Exception)
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		static public int ConvertToInteger(string text)
		{
			try
			{
				var integer = Int32.Parse(text);
				return integer;
			}
			catch
			{
				return 0;
			}
		}

		static public string GetCompanyDomain(string email)
        {
			try
            {
				var segments = email.Split("@");

				if (segments.Length != 2)
                {
					return null;
                }

				switch (segments[1].ToLower())
                {
					case "gmail.com":
					case "hotmail.com":
					case "outlook.com":
					case "aol.com":
					case "mail.com":
					case "zoho.com":
					case "yahoo.com":
					case "protonmail.com":
						return null;
					default:
						return segments[1];
                }
            }
			catch (Exception)
            {
				return null;
            }
        }
	}
}
