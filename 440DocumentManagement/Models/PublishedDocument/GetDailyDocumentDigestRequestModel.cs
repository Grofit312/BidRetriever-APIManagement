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

        [BindProperty(Name = "company_id")]
        [JsonProperty("company_id")]
        public string CompanyId { get; set; }

        [BindProperty(Name = "user_id")]
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
