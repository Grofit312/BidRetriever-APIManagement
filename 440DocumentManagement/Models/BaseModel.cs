using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace _440DocumentManagement.Models
{
	public class BaseModel
	{
		public string CheckRequiredParameters(string[] requiredParameterNames)
		{
			for (var index = 0; index < requiredParameterNames.Length; index++)
			{
				var parameterName = requiredParameterNames[index];
				var parameterValue = GetType().GetProperty(parameterName).GetValue(this, null);

				if (parameterValue == null)
				{
					return parameterName;
				}
			}

			return null;
		}
	}

	public class BaseResponseModel
	{
		[BindProperty(Name = "status")]
		[JsonProperty("status")]
		[Description("Status of the api call")]
		[Required]
		public string Status { get; set; }
		[BindProperty(Name = "message")]
		[JsonProperty("message")]
		[Description("Message returned")]
		[Required]
		public string Message { get; set; }
	}

	public class BaseErrorModel
	{
		[BindProperty(Name = "status")]
		[JsonProperty("status")]
		[Description("Status of the api call")]
		[Required]
		public string Status { get; set; }
		[BindProperty(Name = "message")]
		[JsonProperty("message")]
		[Description("Error Reason")]
		[Required]
		public string Message { get; set; }
	}
}
