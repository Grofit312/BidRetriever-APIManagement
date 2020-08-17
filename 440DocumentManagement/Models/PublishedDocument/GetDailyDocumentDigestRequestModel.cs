using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace _440DocumentManagement.Models
{
	public class GetDailyDocumentDigestRequestModel : BaseModel
	{
		[BindProperty(Name = "daily_digest_date")]
		[JsonProperty("daily_digest_date")]
		public DateTime? DailyDigestDate { get; set; }
	}
}
