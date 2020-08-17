using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace _440DocumentManagement.Models.DataViewFilterModel
{
	public class DataViewFilterModel : BaseModel
	{
		[BindProperty(Name = "data_view_filter_id")]
		[JsonProperty("data_view_filter_id")]
		public string DataViewFilterId { get; set; }
		[BindProperty(Name = "data_view_id")]
		[JsonProperty("data_view_id")]
		public string DataViewId { get; set; }
		[BindProperty(Name = "data_view_filter_name")]
		[JsonProperty("data_view_filter_name")]
		public string DataViewFilterName { get; set; }
		[BindProperty(Name = "data_view_filter_sql")]
		[JsonProperty("data_view_filter_sql")]
		public string DataViewFilterSql { get; set; }
		[BindProperty(Name = "data_view_filter_status")]
		[JsonProperty("data_view_filter_status")]
		public string DataViewFilterStatus { get; set; }
		[BindProperty(Name = "data_source_id")]
		[JsonProperty("data_source_id")]
		public string DataSourceId { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "user_id")]
		[JsonProperty("user_id")]
		public string UserId { get; set; }
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

	public class DataViewFilterFindRequestModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "data_source_id")]
		public string DataSourceId { get; set; }
		[BindProperty(Name = "user_id")]
		public string UserId { get; set; }
		[BindProperty(Name = "data_view_filter_status")]
		public string DataViewFilterStatus { get; set; }
	}
	public class DataViewFilterFindRequestExtendedModel: BaseModel
	{
		public string[] CustomerId { get; set; }
		public string DataSourceId { get; set; }
		public string UserId { get; set; }
		public string DataViewFilterStatus { get; set; }
	}

	public class DataViewFilterGetRequestModel : BaseModel
	{
		[BindProperty(Name = "data_view_filter_id")]
		public string DataViewFilterId { get; set; }
	}

	public class DataViewFilterUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_data_view_filter_id")]
		public string SearchDataViewFilterId { get; set; }

		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_filter_name")]
		public string DataViewFilterName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_filter_sql")]
		public string DataViewFilterSql { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_id")]
		public string DataSourceId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_filter_status")]
		public string DataViewFilterStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "user_id")]
		public string UserId { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
