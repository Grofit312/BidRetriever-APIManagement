using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace _440DocumentManagement.Models.Dashboard
{
	public class DashboardModel : BaseModel
	{
		[BindProperty(Name = "dashboard_id")]
		[JsonProperty("dashboard_id")]
		[Description("The guid of a specific Dashboard")]
		public string DashboardId { get; set; }
		[BindProperty(Name = "dashboard_name")]
		[JsonProperty("dashboard_name")]
		[Description("The name of the dashboard")]
		public string DashboardName { get; set; }
		[BindProperty(Name = "dashboard_start_datetime")]
		[JsonProperty("dashboard_start_datetime")]
		[Description("The date and time that this dashboard starts displaying")]
		public DateTime? DashboardStartDatetime { get; set; }
		[BindProperty(Name = "dashboard_end_datetime")]
		[JsonProperty("dashboard_end_datetime")]
		[Description("The date and time that this dashboard stops displaying")]
		public DateTime? DashboardEndDatetime { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		[Description("The date and time this dashboard was created")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		[Description("The date and time this dashboard was last edited")]
		public DateTime? EditDatetime { get; set; }
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		[Description("The user_id for the user who created this dashboard")]
		public string CreateUserId { get; set; }
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		[Description("The user_id for the user who last edited this dashboard")]
		public string EditUserId { get; set; }
		[BindProperty(Name = "dashboard_version_number")]
		[JsonProperty("dashboard_version_number")]
		[Description("The number of times this dashboard has been edited.")]
		public int? DashboardVersionNumber { get; set; }
		[BindProperty(Name = "dashboard_status")]
		[JsonProperty("dashboard_status")]
		[Description("This is one of the following values:  draft, active, deleted, archived, inactive.")]
		public string DashboardStatus { get; set; }
		[BindProperty(Name = "dashboard_template_id")]
		[JsonProperty("dashboard_template_id")]
		[Description("The parameter identifies the dashboard template used to create this dashboard.")]
		public string DashboardTemplateId { get; set; }
		[BindProperty(Name = "dashboard_type")]
		[JsonProperty("dashboard_type")]
		[Description("This parameter identifies if the dashboard is a normal, night, or Alert dashboard.")]
		public string DashboardType { get; set; }
		[BindProperty(Name = "dashboard_file_bucketname")]
		[JsonProperty("dashboard_file_bucketname")]
		[Description("This parameter identifies the S3 bucket name that should be used to download the dashboard file.")]
		public string DashboardFileBucketname { get; set; }
		[BindProperty(Name = "dashboard_file_key")]
		[JsonProperty("dashboard_file_key")]
		[Description("This parameter identifies the S3 key that identifies the dashboard file to be downloaded for this dashboard.")]
		public string DashboardFileKey { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID from the customer who owns this dashboard.")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "user_id")]
		[JsonProperty("user_id")]
		[Description("This is the GUID from the user who owns this dashboard.")]
		public string UserId { get; set; }
		[BindProperty(Name = "office_id")]
		[JsonProperty("office_id")]
		[Description("This is the GUID from the customer office that owns this dashbaord")]
		public string OfficeId { get; set; }
		[BindProperty(Name = "device_id")]
		[JsonProperty("device_id")]
		[Description("")]
		public string DeviceId { get; set; }
	}

	public class DashboardCreateResponseModel : BaseModel
	{
		[BindProperty(Name = "status")]
		[JsonProperty("status")]
		[Description("Status of the api call")]
		public string Status { get; set; }
		[BindProperty(Name = "dashboard_id")]
		[JsonProperty("dashboard_id")]
		[Description("The guid of a specific Dashboard")]
		public string DashboardId { get; set; }
	}

	public class DashboardFindRequestModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID that identifies the customer_id that will return all dashboards that are available to the customer_id specified. If no customer_id or other filter value is provided, the system will return just the default dashboards.")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "user_id")]
		[JsonProperty("user_id")]
		[Description("This is the GUID that identifies the user_id that will return all dashboards that are available to the user_id specified.")]
		public string UserId { get; set; }
		[BindProperty(Name = "office_id")]
		[JsonProperty("office_id")]
		[Description("This is the GUID that identifies the office_id that will return all dashboards that are available to the office_id specified.")]
		public string OfficeId { get; set; }
		[BindProperty(Name = "device_id")]
		[JsonProperty("device_id")]
		[Description("")]
		public string DeviceId { get; set; }
		[BindProperty(Name = "dashboard_status")]
		[JsonProperty("dashboard_status")]
		[Description("This is one of the following values:  draft, active, deleted, archived, inactive.")]
		public string DashboardStatus { get; set; }
		[BindProperty(Name = "dashboard_type")]
		[JsonProperty("dashboard_type")]
		[Description("This parameter identifies if the dashboard is a normal, night, or Alert dashboard.")]
		public string DashboardType { get; set; }
	}

	public class DashboardGetRequestModel : BaseModel
	{
		[BindProperty(Name = "dashboard_id")]
		[JsonProperty("dashboard_id")]
		[Description("The guid of a specific Dashboard")]
		[BindRequired]
		public string DashboardId { get; set; }
	}

	public class DashboardUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_dashboard_id")]
		[JsonProperty("search_dashboard_id")]
		[Description("The system returns the dashboard_id GUID of the dashboard that is to be updated.")]
		public string SearchDashboardId { get; set; }

		[BindProperty(Name = "dashboard_name")]
		[JsonProperty("dashboard_name")]
		[Description("The name of the dashboard")]
		public string DashboardName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_start_datetime")]
		[JsonProperty("dashboard_start_datetime")]
		[Description("The date and time that this dashboard starts displaying")]
		public DateTime? DashboardStartDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "dashboard_end_datetime")]
		[JsonProperty("dashboard_end_datetime")]
		[Description("The date and time that this dashboard stops displaying")]
		public DateTime? DashboardEndDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		[Description("The date and time this dashboard was created")]
		public DateTime? CreateDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		[Description("The date and time this dashboard was last edited")]
		public DateTime? EditDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		[Description("The user_id for the user who created this dashboard")]
		public string CreateUserId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		[Description("The user_id for the user who last edited this dashboard")]
		public string EditUserId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_version_number")]
		[JsonProperty("dashboard_version_number")]
		[Description("The number of times this dashboard has been edited.")]
		public int? DashboardVersionNumber { get; set; } = ApiExtension.UNDEFINED_INT;
		[BindProperty(Name = "dashboard_status")]
		[JsonProperty("dashboard_status")]
		[Description("This is one of the following values:  draft, active, deleted, archived, inactive.")]
		public string DashboardStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_template_id")]
		[JsonProperty("dashboard_template_id")]
		[Description("The parameter identifies the dashboard template used to create this dashboard.")]
		public string DashboardTemplateId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_type")]
		[JsonProperty("dashboard_type")]
		[Description("This parameter identifies if the dashboard is a normal, night, or Alert dashboard.")]
		public string DashboardType { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_file_bucketname")]
		[JsonProperty("dashboard_file_bucketname")]
		[Description("This parameter identifies the S3 bucket name that should be used to download the dashboard file.")]
		public string DashboardFileBucketname { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_file_key")]
		[JsonProperty("dashboard_file_key")]
		[Description("This parameter identifies the S3 key that identifies the dashboard file to be downloaded for this dashboard.")]
		public string DashboardFileKey { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID from the customer who owns this dashboard.")]
		public string CustomerId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "user_id")]
		[JsonProperty("user_id")]
		[Description("This is the GUID from the user who owns this dashboard.")]
		public string UserId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "office_id")]
		[JsonProperty("office_id")]
		[Description("This is the GUID from the customer office that owns this dashbaord")]
		public string OfficeId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "device_id")]
		[JsonProperty("device_id")]
		[Description("")]
		public string DeviceId { get; set; } = ApiExtension.UNDEFINED_STRING;
	}

	public class GetAnalyticDataRequestModel : BaseModel
	{
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("")]
		[BindRequired]
		public string CustomerId { get; set; }
		[BindProperty(Name = "company_id")]
		[JsonProperty("company_id")]
		[Description("")]
		public string CompanyId { get; set; }
		[BindProperty(Name = "datasource_id")]
		[JsonProperty("datasource_id")]
		[Description("")]
		public string DatasourceId { get; set; }
		[BindProperty(Name = "datasource_startdatetime")]
		[JsonProperty("datasource_startdatetime")]
		[Description("")]
		public string DatasourceStartdatetime { get; set; }
		[BindProperty(Name = "datasource_enddatetime")]
		[JsonProperty("datasource_enddatetime")]
		[Description("")]
		public string DatasourceEnddatetime { get; set; }
		[BindProperty(Name = "datasource_interval")]
		[JsonProperty("datasource_interval")]
		[Description("")]
		public string DatasourceInterval { get; set; }
		[BindProperty(Name = "analytic_type")]
		[JsonProperty("analytic_type")]
		[Description("")]
		public string AnalyticType { get; set; }
	}
}
