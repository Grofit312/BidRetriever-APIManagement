using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace _440DocumentManagement.Models.DashboardPanel
{
	public class DashboardPanelModel : BaseModel
	{
		[BindProperty(Name = "panel_id")]
		[JsonProperty("panel_id")]
		[Description("The GUID that identifies the unique panel")]
		public string PanelId { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		[Description("The date and time that this panel was created")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		[Description("The guid of the user who created this panel")]
		public string CreateUserId { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		[Description("The date and time that this panel was last edited.")]
		public DateTime? EditDatetime { get; set; }
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		[Description("The user GUID for the user that last edited this panel")]
		public string EditUserId { get; set; }
		[BindProperty(Name = "panel_start_datetime")]
		[JsonProperty("panel_start_datetime")]
		[Description("The start date time for the panel")]
		public DateTime? PanelStartDatetime { get; set; }
		[BindProperty(Name = "panel_end_datetime")]
		[JsonProperty("panel_end_datetime")]
		[Description("The end date time for the panel")]
		public DateTime? PanelEndDatetime { get; set; }
		[BindProperty(Name = "panel_time_interval")]
		[JsonProperty("panel_time_interval")]
		[Description("The interval for the data of the panel.")]
		public string PanelTimeInterval { get; set; }
		[BindProperty(Name = "panel_chart_type")]
		[JsonProperty("panel_chart_type")]
		[Description("The type of chart to display")]
		public string PanelChartType { get; set; }
		[BindProperty(Name = "panel_analytic_datasource")]
		[JsonProperty("panel_analytic_datasource")]
		[Description("The GUID of the analytic data source.")]
		[BindRequired]
		public string PanelAnalyticDatasource { get; set; }
		[BindProperty(Name = "panel_row")]
		[JsonProperty("panel_row")]
		[Description("The row this panel is displayed on the dashboard")]
		public short? PanelRow { get; set; }
		[BindProperty(Name = "panel_column")]
		[JsonProperty("panel_column")]
		[Description("The column this panel is displayed on the dashboard")]
		public short? PanelColumn { get; set; }
		[BindProperty(Name = "panel_height")]
		[JsonProperty("panel_height")]
		[Description("The height of the panel")]
		public short? PanelHeight { get; set; }
		[BindProperty(Name = "panel_width")]
		[JsonProperty("panel_width")]
		[Description("The width of the panel")]
		public short? PanelWidth { get; set; }
		[BindProperty(Name = "panel_name")]
		[JsonProperty("panel_name")]
		[Description("The user assigned name of this panel")]
		[BindRequired]
		public string PanelName { get; set; }
		[BindProperty(Name = "panel_desc")]
		[JsonProperty("panel_desc")]
		[Description("The description of this panel")]
		public string PanelDesc { get; set; }
		[BindProperty(Name = "dashboard_id")]
		[JsonProperty("dashboard_id")]
		[Description("The GUID of the dashboard that this panel is assigned.")]
		[BindRequired]
		public string DashboardId { get; set; }
		[BindProperty(Name = "panel_analytic_datasource_interval")]
		[JsonProperty("panel_analytic_datasource_interval")]
		[Description("The Interval used when displaying this panel. Values: Day, Week, Month, Quarter, Year.")]
		public string PanelAnalyticDatasourceInterval { get; set; }
		[BindProperty(Name = "panel_end_date_offset")]
		[JsonProperty("panel_end_date_offset")]
		[Description("The end date time for the panel. This value will be defined in “Intervals” from current date. A value of 30 would return data from 30 “Intervals” after todays date. A value of -5 would return data from five days prior to todays date. A value of 0 would show only historical data, because the end date would be zero intervals from todays date. If this value is not provided, the offset will default to a value of Zero.")]
		public int? PanelEndDateOffset { get; set; }
		[BindProperty(Name = "panel_header")]
		[JsonProperty("panel_header")]
		[Description("The text header that will be displayed above the graph. This value can contain standard attributes separated by <>. Example: <company_name>")]
		public string PanelHeader { get; set; }
		[BindProperty(Name = "panel_start_date_offset")]
		[JsonProperty("panel_start_date_offset")]
		[Description("The start date time for the panel as an offset from today. This value will be defined in intervals from the current date. A value of -30 would start the data 30 intervals prior to todays date. A value of zero would only return future data since the start date would be zero days from todays date. A value of 7 would return data starting seven intervals in the future. If a value is not provided, the system will default to a value of zero.")]
		public int? PanelStartDateOffset { get; set; }
	}

	public class DashboardPanelCreateResponseModel : BaseModel
	{
		[BindProperty(Name = "status")]
		[JsonProperty("status")]
		[Description("Status of the api call")]
		public string Status { get; set; }
		[BindProperty(Name = "panel_id")]
		[JsonProperty("panel_id")]
		[Description("The GUID that identifies the unique panel")]
		public string PanelId { get; set; }
	}

	public class DashboardPanelFindRequestModel : BaseModel
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
		[BindProperty(Name = "dashboard_id")]
		[JsonProperty("dashboard_id")]
		[Description("")]
		public string DashboardId { get; set; }
	}

	public class DashboardPanelGetRequestModel : BaseModel
	{
		[BindProperty(Name = "panel_id")]
		[JsonProperty("panel_id")]
		[Description("The GUID that identifies the unique panel")]
		public string PanelId { get; set; }
	}

	public class DashboardPanelUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_panel_id")]
		[JsonProperty("search_panel_id")]
		[Description("The GUID that identifies the unique panel")]
		[BindRequired]
		public string SearchPanelId { get; set; }

		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		[Description("The date and time that this panel was created")]
		public DateTime? CreateDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "create_user_id")]
		[JsonProperty("create_user_id")]
		[Description("The guid of the user who created this panel")]
		public string CreateUserId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		[Description("The date and time that this panel was last edited.")]
		public DateTime? EditDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "edit_user_id")]
		[JsonProperty("edit_user_id")]
		[Description("The user GUID for the user that last edited this panel")]
		public string EditUserId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_start_datetime")]
		[JsonProperty("panel_start_datetime")]
		[Description("The start date time for the panel")]
		public DateTime? PanelStartDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "panel_end_datetime")]
		[JsonProperty("panel_end_datetime")]
		[Description("The end date time for the panel")]
		public DateTime? PanelEndDatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "panel_time_interval")]
		[JsonProperty("panel_time_interval")]
		[Description("The interval for the data of the panel.")]
		public string PanelTimeInterval { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_chart_type")]
		[JsonProperty("panel_chart_type")]
		[Description("The type of chart to display")]
		public string PanelChartType { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_analytic_datasource")]
		[JsonProperty("panel_analytic_datasource")]
		[Description("The GUID of the analytic data source.")]
		public string PanelAnalyticDatasource { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_row")]
		[JsonProperty("panel_row")]
		[Description("The row this panel is displayed on the dashboard")]
		public short? PanelRow { get; set; } = ApiExtension.UNDEFINED_SHORT;
		[BindProperty(Name = "panel_column")]
		[JsonProperty("panel_column")]
		[Description("The column this panel is displayed on the dashboard")]
		public short? PanelColumn { get; set; } = ApiExtension.UNDEFINED_SHORT;
		[BindProperty(Name = "panel_height")]
		[JsonProperty("panel_height")]
		[Description("The height of the panel")]
		public short? PanelHeight { get; set; } = ApiExtension.UNDEFINED_SHORT;
		[BindProperty(Name = "panel_width")]
		[JsonProperty("panel_width")]
		[Description("The width of the panel")]
		public short? PanelWidth { get; set; } = ApiExtension.UNDEFINED_SHORT;
		[BindProperty(Name = "panel_name")]
		[JsonProperty("panel_name")]
		[Description("The user assigned name of this panel")]
		public string PanelName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_desc")]
		[JsonProperty("panel_desc")]
		[Description("The description of this panel")]
		public string PanelDesc { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "dashboard_id")]
		[JsonProperty("dashboard_id")]
		[Description("The GUID of the dashboard that this panel is assigned.")]
		public string DashboardId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_analytic_datasource_interval")]
		[JsonProperty("panel_analytic_datasource_interval")]
		[Description("The Interval used when displaying this panel. Values: Day, Week, Month, Quarter, Year.")]
		public string PanelAnalyticDatasourceInterval { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_end_date_offset")]
		[JsonProperty("panel_end_date_offset")]
		[Description("The end date time for the panel. This value will be defined in “Intervals” from current date. A value of 30 would return data from 30 “Intervals” after todays date. A value of -5 would return data from five days prior to todays date. A value of 0 would show only historical data, because the end date would be zero intervals from todays date. If this value is not provided, the offset will default to a value of Zero.")]
		public int? PanelEndDateOffset { get; set; } = ApiExtension.UNDEFINED_INT;
		[BindProperty(Name = "panel_header")]
		[JsonProperty("panel_header")]
		[Description("The text header that will be displayed above the graph. This value can contain standard attributes separated by <>. Example: <company_name>")]
		public string PanelHeader { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "panel_start_date_offset")]
		[JsonProperty("panel_start_date_offset")]
		[Description("The start date time for the panel as an offset from today. This value will be defined in intervals from the current date. A value of -30 would start the data 30 intervals prior to todays date. A value of zero would only return future data since the start date would be zero days from todays date. A value of 7 would return data starting seven intervals in the future. If a value is not provided, the system will default to a value of zero.")]
		public int? PanelStartDateOffset { get; set; } = ApiExtension.UNDEFINED_INT;
	}
}
