using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;

namespace _440DocumentManagement.Models
{
    public class ValidSheetNameWordFindRequest
    {
        [BindProperty(Name = "begin_with")]
        [JsonProperty("begin_with")]
        [Description("Search criteria to find out sheet names begin with the given literals")]
        public string BeginWith { get; set; }
    }
}
