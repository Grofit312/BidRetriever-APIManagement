using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;

namespace _440DocumentManagement.Models
{
	public class ValidSheetNameWord : BaseModel
	{
        [BindProperty(Name = "sheet_name_word")]
        [JsonProperty("sheet_name_word")]
        [Description("Sheet name word")]
        public string SheetNameWord { get; set; }

        [BindProperty(Name = "sheet_name_word_abbrv")]
        [JsonProperty("sheet_name_word_abbrv")]
        [Description("Sheet name word abbreviation")]
        public string SheetNameWordAbbrv { get; set; }

        [BindProperty(Name = "ocr")]
        [JsonProperty("ocr")]
        [Description("Indicates whether sheet name was indexed by OCR")]
        public bool Ocr { get; set; } = false;

        [BindProperty(Name = "manual")]
        [JsonProperty("manual")]
        [Description("Indicates whether sheet name was indexed manually")]
        public bool Manual { get; set; } = false;
	}
}
