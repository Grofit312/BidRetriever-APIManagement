using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace _440DocumentManagement.Models.DataSourceField
{
	public class DataSourceFieldModel : BaseModel
	{
		[BindProperty(Name = "data_source_field_id")]
		[JsonProperty("data_source_field_id")]
		public string DataSourceFieldId { get; set; }
		[BindProperty(Name = "data_source_field_name")]
		[JsonProperty("data_source_field_name")]
		public string DataSourceFieldName { get; set; }
		[BindProperty(Name = "data_source_field_displayname")]
		[JsonProperty("data_source_field_displayname")]
		public string DataSourceFieldDisplayname { get; set; }
		[BindProperty(Name = "data_source_field_desc")]
		[JsonProperty("data_source_field_desc")]
		public string DataSourceFieldDesc { get; set; }
		[BindProperty(Name = "data_source_field_datatype")]
		[JsonProperty("data_source_field_datatype")]
		public string DataSourceFieldDatatype { get; set; }
		[BindProperty(Name = "data_source_id")]
		[JsonProperty("data_source_id")]
		public string DataSourceId { get; set; }
		[BindProperty(Name = "required_field")]
		[JsonProperty("required_field")]
		public bool? RequiredField { get; set; }
		[BindProperty(Name = "customer_attribute_id")]
		[JsonProperty("customer_attribute_id")]
		public string CustomerAttributeId { get; set; }
		[BindProperty(Name = "data_source_field_heading")]
		[JsonProperty("data_source_field_heading")]
		public string DataSourceFieldHeading { get; set; }
		[BindProperty(Name = "data_source_field_status")]
		[JsonProperty("data_source_field_status")]
		public string DataSourceFieldStatus { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		public DateTime? EditDatetime { get; set; }
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		public string CreateUserId { get; set; }
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		public string EditUserId { get; set; }
	}

	public class DataSourceFieldGetRequestModel : BaseModel
	{
		[BindProperty(Name = "data_source_field_id")]
		public string DataSourceFieldId { get; set; }
	}

	public class DataSourceFieldUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_data_source_field_id")]
		public string SearchDataSourceFieldId { get; set; }

		[BindProperty(Name = "customer_attribute_id")]
		public string CustomerAttributeId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_field_heading")]
		public string DataSourceFieldHeading { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_field_name")]
		public string DataSourceFieldName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_field_displayname")]
		public string DataSourceFieldDisplayname { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "required_field")]
		public bool? RequiredField { get; set; }
		[BindProperty(Name = "data_source_field_status")]
		public string DataSourceFieldStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
