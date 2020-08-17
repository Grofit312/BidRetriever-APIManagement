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
	}
}
