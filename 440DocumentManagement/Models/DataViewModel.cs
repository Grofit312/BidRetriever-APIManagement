using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Models.DataView
{
	public class DataViewModel : BaseModel
	{
		[BindProperty(Name = "company_id")]
		[JsonProperty("company_id")]
		public string CompanyId { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		public string CreateUserId { get; set; }
		[BindProperty(Name = "data_source_id")]
		[JsonProperty("data_source_id")]
		public string DataSourceId { get; set; }
		[BindProperty(Name = "data_filter_id")]
		[JsonProperty("data_filter_id")]
		public string DataFilterId { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		public DateTime? EditDatetime { get; set; }
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		public string EditUserId { get; set; }
		[BindProperty(Name = "group_id")]
		[JsonProperty("group_id")]
		public string GroupId { get; set; }
		[BindProperty(Name = "office_id")]
		[JsonProperty("office_id")]
		public string OfficeId { get; set; }
		[BindProperty(Name = "user_id")]
		[JsonProperty("user_id")]
		public string UserId { get; set; }
		[BindProperty(Name = "view_desc")]
		[JsonProperty("view_desc")]
		public string ViewDesc { get; set; }
		[BindProperty(Name = "view_field_list")]
		[JsonProperty("view_field_list")]
		public string ViewFieldList { get; set; }
		[BindProperty(Name = "view_field_settings")]
		[JsonProperty("view_field_settings")]
		public string ViewFieldSettings { get; set; }
		[BindProperty(Name = "view_filter")]
		[JsonProperty("view_filter")]
		public string ViewFilter { get; set; }
		[BindProperty(Name = "view_id")]
		[JsonProperty("view_id")]
		public string ViewId { get; set; }
		[BindProperty(Name = "view_name")]
		[JsonProperty("view_name")]
		public string ViewName { get; set; }
		[BindProperty(Name = "view_query_generated")]
		[JsonProperty("view_query_generated")]
		public string ViewQueryGenerated { get; set; }
		[BindProperty(Name = "view_type")]
		[JsonProperty("view_type")]
		public string ViewType { get; set; }
		[BindProperty(Name = "view_sort")]
		[JsonProperty("view_sort")]
		public string ViewSort { get; set; }
		[BindProperty(Name = "view_status")]
		[JsonProperty("view_status")]
		public string ViewStatus { get; set; }
	}

	public class DataViewFindRequestModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "detail_level")]
		public string DetailLevel { get; set; }
		[BindProperty(Name = "group_id")]
		public string GroupId { get; set; }
		[BindProperty(Name = "office_id")]
		public string OfficeId { get; set; }
		[BindProperty(Name = "user_id")]
		public string UserId { get; set; }
		[BindProperty(Name = "view_type")]
		public string ViewType { get; set; }
		[BindProperty(Name = "view_status")]
		public string ViewStatus { get; set; }
	}
	public class DataViewFindResponseBasicModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "group_id")]
		[JsonProperty("group_id")]
		public string GroupId { get; set; }
		[BindProperty(Name = "office_id")]
		[JsonProperty("office_id")]
		public string OfficeId { get; set; }
		[BindProperty(Name = "office_name")]
		[JsonProperty("office_name")]
		public string OfficeName { get; set; }
		[BindProperty(Name = "user_id")]
		[JsonProperty("user_id")]
		public string UserId { get; set; }
		[BindProperty(Name = "view_data_source_name")]
		[JsonProperty("view_data_source_name")]
		public string ViewDataSourceName { get; set; }
		[BindProperty(Name = "view_desc")]
		[JsonProperty("view_desc")]
		public string ViewDesc { get; set; }
		[BindProperty(Name = "view_filter_name")]
		[JsonProperty("view_filter_name")]
		public string ViewFilterName { get; set; }
		[BindProperty(Name = "view_id")]
		[JsonProperty("view_id")]
		public string ViewId { get; set; }
		[BindProperty(Name = "view_name")]
		[JsonProperty("view_name")]
		public string ViewName { get; set; }
		[BindProperty(Name = "view_type")]
		[JsonProperty("view_type")]
		public string ViewType { get; set; }
	}
	public class DataViewFindResponseAllModel : DataViewFindResponseBasicModel
	{
		[BindProperty(Name = "company_name")]
		[JsonProperty("company_name")]
		public string CompanyName { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		public DateTime? EditDatetime { get; set; }
		[BindProperty(Name = "user_displayname")]
		[JsonProperty("user_displayname")]
		public string UserDisplayname { get; set; }
	}
	public class DataViewFindResponseAdminModel : DataViewFindResponseAllModel
	{
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		public string CreateUserId { get; set; }
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		public string EditUserId { get; set; }
	}

	public class DataViewGetRequestModel : BaseModel
	{
		[BindProperty(Name = "view_id")]
		public string ViewId { get; set; }
	}

	public class DataViewUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_view_id")]
		public string SearchViewId { get; set; }

		[BindProperty(Name = "data_filter_id")]
		public string DataFilterId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_source_id")]
		public string DataSourceId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "group_id")]
		public string GroupId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "office_id")]
		public string OfficeId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_desc")]
		public string ViewDesc { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "user_id")]
		public string UserId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_field_list")]
		public string ViewFieldList { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_field_setting_id")]
		public string ViewFieldSettingId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_name")]
		public string ViewName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_sort")]
		public string ViewSort { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_type")]
		public string ViewType { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "view_status")]
		public string ViewStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
	}

	public class DataViewDetailsModel
	{
		public class CustomerAttributeSimpleModel
		{
			public string CustomerAttributeId { get; set; }
			public string CustomerAttributeName { get; set; }
			public string CustomerAttributeSource { get; set; }
			public string SystemAttributeId { get; set; }
		}

		public class DataFilterSimpleModel
		{
			public string DataViewFilterName { get; set; }
			public string DataViewFilterSql { get; set; }
		}

		public class DataSourceFieldSimpleModel
		{
			public string DataSourceFieldId { get; set; }
			public string DataSourceFieldName { get; set; }
			public CustomerAttributeSimpleModel CustomerAttribute { get; set; }
		}

		public class DataSourceSimpleModel
		{
			public string DataSourceId { get; set; }
			public string DataSourceName { get; set; }
			public List<DataSourceFieldSimpleModel> DataSourceFields { get; set; }
		}

		public string ViewId { get; set; }
		public string ViewName { get; set; }
		public DataFilterSimpleModel ViewFilter { get; set; }
		public DataSourceSimpleModel ViewSource { get; set; }
	}
}
