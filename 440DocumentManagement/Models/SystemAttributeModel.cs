using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace _440DocumentManagement.Models.SystemAttribute
{
	public class SystemAttributeModel : BaseModel
	{
		[BindProperty(Name = "system_attribute_id")]
		[JsonProperty("system_attribute_id")]
		public string SystemAttributeId { get; set; }
		[BindProperty(Name = "system_attribute_name")]
		[JsonProperty("system_attribute_name")]
		public string SystemAttributeName { get; set; }
		[BindProperty(Name = "system_attribute_desc")]
		[JsonProperty("system_attribute_desc")]
		public string SystemAttributeDesc { get; set; }
		[BindProperty(Name = "system_attribute_datatype")]
		[JsonProperty("system_attribute_datatype")]
		public string SystemAttributeDatatype { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		public string CreateUserId { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		public DateTime? EditDatetime { get; set; }
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		public string EditUserId { get; set; }
		[BindProperty(Name = "system_attribute_status")]
		[JsonProperty("system_attribute_status")]
		public string SystemAttributeStatus { get; set; }
		[BindProperty(Name = "default_alignment")]
		[JsonProperty("default_alignment")]
		public string DefaultAlignment { get; set; }
		[BindProperty(Name = "default_width")]
		[JsonProperty("default_width")]
		public int? DefaultWidth { get; set; }
		[BindProperty(Name = "default_format")]
		[JsonProperty("default_format")]
		public string DefaultFormat { get; set; }
		[BindProperty(Name = "default_heading")]
		[JsonProperty("default_heading")]
		public string DefaultHeading { get; set; }
		[BindProperty(Name = "system_attribute_source")]
		[JsonProperty("system_attribute_source")]
		public string SystemAttributeSource { get; set; }
	}

	public class SystemAttributeFindRequestModel : BaseModel
	{
		[BindProperty(Name = "system_attribute_source")]
		public string SystemAttributeSource { get; set; }
		[BindProperty(Name = "system_attribute_status")]
		public string SystemAttributeStatus { get; set; }
	}

	public class SystemAttributeUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_system_attribute_id")]
		public string SearchSystemAttributeId { get; set; }

		[BindProperty(Name = "default_alignment")]
		public string DefaultAlignment { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "default_format")]
		public string DefaultFormat { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "default_heading")]
		public string DefaultHeading { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "default_width")]
		public int? DefaultWidth { get; set; }
		[BindProperty(Name = "system_attribute_datatype")]
		public string SystemAttributeDatatype { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "system_attribute_desc")]
		public string SystemAttributeDesc { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "system_attribute_name")]
		public string SystemAttributeName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "system_attribute_status")]
		public string SystemAttributeStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "system_attribute_source")]
		public string SystemAttributeSource { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
