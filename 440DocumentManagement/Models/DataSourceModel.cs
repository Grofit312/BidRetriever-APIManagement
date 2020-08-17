using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Models.DataSource
{
	public class DataSourceModel : BaseModel
	{
		[BindProperty(Name = "data_source_id")]
		[JsonProperty("data_source_id")]
		public string DataSourceId { get; set; }
		[BindProperty(Name = "data_source_name")]
		[JsonProperty("data_source_name")]
		public string DataSourceName { get; set; }
		[BindProperty(Name = "data_source_desc")]
		[JsonProperty("data_source_desc")]
		public string DataSourceDesc { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "data_source_base_query")]
		[JsonProperty("data_source_base_query")]
		public string DataSourceBaseQuery { get; set; }
		[BindProperty(Name = "data_source_status")]
		[JsonProperty("data_source_status")]
		public string DataSourceStatus { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		public string CustomerId { get; set; }
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
	public class DataSourceExtendedModel : DataSourceModel
	{
		[BindProperty(Name = "data_source_fields")]
		[JsonProperty("data_source_fields")]
		public List<object> DataSourceFields { get; set; }
	}

	public class DataSourceFindRequestModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "data_source_status")]
		public string DataSourceStatus { get; set; }
	}
	public class DataSourceFindRequestExtendedModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		public string[] CustomerId { get; set; }
		[BindProperty(Name = "data_source_status")]
		public string DataSourceStatus { get; set; }
	}

	public class DataSourceGetRequestModel : BaseModel
	{
		[BindProperty(Name = "data_source_id")]
		public string DataSourceId { get; set; }
	}

	public class DataSourceUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_data_source_id")]
		public string SearchDataSourceId { get; set; }

		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_desc")]
		public string DataSourceDesc { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_name")]
		public string DataSourceName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_base_query")]
		public string DataSourceBaseQuery { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_status")]
		public string DataSourceStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
