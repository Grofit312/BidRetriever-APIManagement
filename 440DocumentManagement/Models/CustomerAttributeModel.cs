using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace _440DocumentManagement.Models.CustomerAttribute
{
	public class CustomerAttributeModel : BaseModel
	{
		[BindProperty(Name = "customer_attribute_name")]
		[JsonProperty("customer_attribute_name")]
		public string CustomerAttributeName { get; set; }
		[BindProperty(Name = "customer_attribute_displayname")]
		[JsonProperty("customer_attribute_displayname")]
		public string CustomerAttributeDisplayname { get; set; }
		[BindProperty(Name = "customer_attribute_id")]
		[JsonProperty("customer_attribute_id")]
		public string CustomerAttributeId { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "customer_attribute_source")]
		[JsonProperty("customer_attribute_source")]
		public string CustomerAttributeSource { get; set; }
		[BindProperty(Name = "customer_attribute_datatype")]
		[JsonProperty("customer_attribute_datatype")]
		public string CustomerAttributeDatatype { get; set; }
		[BindProperty(Name = "customer_attribute_desc")]
		[JsonProperty("customer_attribute_desc")]
		public string CustomerAttributeDesc { get; set; }
		[BindProperty(Name = "system_attribute_id")]
		[JsonProperty("system_attribute_id")]
		public string SystemAttributeId { get; set; }
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
		[BindProperty(Name = "customer_attribute_status")]
		[JsonProperty("customer_attribute_status")]
		public string CustomerAttributeStatus { get; set; }
		[BindProperty(Name = "default_alignment")]
		[JsonProperty("default_alignment")]
		public string DefaultAlignment { get; set; }
		[BindProperty(Name = "default_heading")]
		[JsonProperty("default_heading")]
		public string DefaultHeading { get; set; }
		[BindProperty(Name = "default_width")]
		[JsonProperty("default_width")]
		public int? DefaultWidth { get; set; }
		[BindProperty(Name = "default_format")]
		[JsonProperty("default_format")]
		public string DefaultFormat { get; set; }
	}

	public class CustomerAttributeFindRequestModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "customer_attribute_status")]
		public string CustomerAttributeStatus { get; set; }
	}
	public class CustomerAttributeFindRequestExtendedModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		public string[] CustomerId { get; set; }
		[BindProperty(Name = "customer_attribute_status")]
		public string CustomerAttributeStatus { get; set; }
	}

	public class CustomerAttributeUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_customer_attribute_id")]
		public string SearchCustomerAttributeId { get; set; }

		[BindProperty(Name = "customer_attribute_datatype")]
		public string CustomerAttributeDatatype { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_attribute_desc")]
		public string CustomerAttributeDesc { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_attribute_name")]
		public string CustomerAttributeName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_attribute_displayname")]
		public string CustomerAttributeDisplayname { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_attribute_status")]
		public string CustomerAttributeStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_attribute_source")]
		public string CustomerAttributeSource { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
