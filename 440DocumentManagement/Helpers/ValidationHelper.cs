using System.Text.RegularExpressions;

namespace _440DocumentManagement.Helpers
{
	public class ValidationHelper
	{
		static public string ValidateProjectName(string str)
		{
			var projectName = str.Trim();
			projectName = Regex.Replace(projectName, @"^FW:", "");
			projectName = Regex.Replace(projectName, @"^Fw:", "");
			projectName = Regex.Replace(projectName, @"^fW:", "");
			projectName = Regex.Replace(projectName, @"^FW:[#]", "");
			projectName = Regex.Replace(projectName, @"^Fw:[#]", "");
			projectName = Regex.Replace(projectName, @"^fw:[#]", "");
			projectName = Regex.Replace(projectName, @"^FW[#]:", "");
			projectName = Regex.Replace(projectName, @"^Fw[#]:", "");
			projectName = Regex.Replace(projectName, @"^fw[#]:", "");
			projectName = Regex.Replace(projectName, @"[\!\""\$\%\&\'\*\,\.\/\:\;\<\>\?\[\\\]\^\`\{\|\}\~]", "");
			projectName = Regex.Replace(projectName, @" ", "_");
			projectName = Regex.Replace(projectName, @"__", "_");
			projectName = Regex.Replace(projectName, @"[\x00-\x1F\x7F-\x9F]", "");
			projectName = Regex.Replace(projectName, @"^_+", "");
			projectName = Regex.Replace(projectName, @"^-+", "");
			projectName = Regex.Replace(projectName, @"_+$", "");
			projectName = Regex.Replace(projectName, @"-+$", "");

			if (projectName.Length == 0)
			{
				return "";
			}
			else
			{
				return projectName[0].ToString().ToUpper() + projectName.Substring(1);
			}
		}

		static public string ValidateDestinationPath(string str)
		{
			var destinationPath = str.Trim();

			destinationPath = Regex.Replace(destinationPath, @"[\!\$\%\&\'\*\,\.\:\;\<\>\?\[\\\]\^\`\{\|\}\~]", "");
			destinationPath = Regex.Replace(destinationPath, @"[\x00-\x1F\x7F-\x9F]", "");
			destinationPath = Regex.Replace(destinationPath, @"^/+", "");
			destinationPath = Regex.Replace(destinationPath, @"/+$", "");

			return destinationPath.Trim();
		}
	}
}
