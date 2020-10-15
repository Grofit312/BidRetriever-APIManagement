using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;

namespace _440DocumentManagement.Models
{
	public class ValidSheetNameWordGetRequest
	{
        [BindProperty(Name = "sheet_name_word")]
        [JsonProperty("sheet_name_word")]
        [Description("Sheet name word")]
        public string sheet_name_word { get; set; }
	}
}
