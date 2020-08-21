using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace _440DocumentManagement.Models.AnalyticDatasources
{
	public class AnalyticDatasourcesModel : BaseModel
	{
		[BindProperty(Name = "analytic_datasource_type")]
		[JsonProperty("analytic_datasource_type")]
		[Description("This is the type of analytic data returned.  This will be used to categorize different analytics so a user can filter by the type they need.")]
		public string AnalyticDatasourceType { get; set; }
		[BindProperty(Name = "company_id")]
		[JsonProperty("company_id")]
		[Description("This is the GUID from the customer_companies table when the analytics is retrieving information for a specific company.")]
		public string CompanyId { get; set; }
		[BindProperty(Name = "compatible_chart_types")]
		[JsonProperty("compatible_chart_types")]
		[Description("This parameter provides a list of the chart types that can be used with this data set..")]
		public string CompatibleChartTypes { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID from the customers table that is used to filter data for a specific customer.")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "analytic_datasource_desc")]
		[JsonProperty("analytic_datasource_desc")]
		[Description("The datasource description")]
		public string AnalyticDatasourceDesc { get; set; }
		[BindProperty(Name = "analytic_datasource_enddatetime")]
		[JsonProperty("analytic_datasource_enddatetime")]
		[Description("The date and time that this datasource stops retrieveing data.")]
		public DateTime? AnalyticDatasourceEnddatetime { get; set; }
		[BindProperty(Name = "analytic_datasource_id")]
		[JsonProperty("analytic_datasource_id")]
		[Description("The guid of a specific datasource.  If this value is not specified, the system will generate.")]
		public string AnalyticDatasourceId { get; set; }
		[BindProperty(Name = "analytic_datasource_interval")]
		[JsonProperty("analytic_datasource_interval")]
		[Description("This parameter identifies the date interval that will be returned for this datasource.  Values:Day, Week, Month, Quarter, Year.   If no value is provided all data will be returned.")]
		public string AnalyticDatasourceInterval { get; set; }
		[BindProperty(Name = "analytic_datasource_lambda_arn")]
		[JsonProperty("analytic_datasource_lambda_arn")]
		[Description("The parameter identifies the datasource lambda to be executed if appropriate.")]
		public string AnalyticDatasourceLambdaArn { get; set; }
		[BindProperty(Name = "analytic_datasource_name")]
		[JsonProperty("analytic_datasource_name")]
		[Description("The user supplied name of the datasource")]
		public string AnalyticDatasourceName { get; set; }
		[BindProperty(Name = "analytic_datasource_sql")]
		[JsonProperty("analytic_datasource_sql")]
		[Description("The parameter identifies the datasource sql to be executed if appropriate.")]
		public string AnalyticDatasourceSql { get; set; }
		[BindProperty(Name = "analytic_datasource_startdatetime")]
		[JsonProperty("analytic_datasource_startdatetime")]
		[Description("The date and time that this dashboard starts displaying")]
		public DateTime? AnalyticDatasourceStartdatetime { get; set; }
		[BindProperty(Name = "analytic_datasource_status")]
		[JsonProperty("analytic_datasource_status")]
		[Description("This is one of the following values:  draft, active, deleted, archived, inactive.")]
		public string AnalyticDatasourceStatus { get; set; }
		[BindProperty(Name = "create_datetime")]
		[JsonProperty("create_datetime")]
		[Description("")]
		public DateTime? CreateDatetime { get; set; }
		[BindProperty(Name = "edit_datetime")]
		[JsonProperty("edit_datetime")]
		[Description("")]
		public DateTime? EditDatetime { get; set; }
	}

	public class AnalyticDatasourcesCreateResponseModel : BaseModel
	{
		[BindProperty(Name = "analytic_datasource_id")]
		[JsonProperty("analytic_datasource_id")]
		[Description("The system returns the analytic_datasource_id GUID of the newly created datasource")]
		public string AnalyticDatasourceId { get; set; }
		[BindProperty(Name = "status")]
		[JsonProperty("status")]
		[Description("The status of the api call")]
		public string Status { get; set; }
	}

	public class AnalyticDatasourcesExecuteRequestModel : BaseModel
	{
		[BindProperty(Name = "analytic_datasource_enddatetime")]
		[JsonProperty("analytic_datasource_enddatetime")]
		[Description("The date and time that this datasource stops retrieveing data. If no value is provided, the system will use todays date and the end date.")]
		public DateTime? AnalyticDatasourceEnddatetime { get; set; }
		[BindProperty(Name = "analytic_datasource_id")]
		[JsonProperty("analytic_datasource_id")]
		[Description("The guid of the analytic datasource that is to be executed.")]
		[BindRequired]
		public string AnalyticDatasourceId { get; set; }
		[BindProperty(Name = "analytic_datasource_interval")]
		[JsonProperty("analytic_datasource_interval")]
		[Description("This parameter identifies the date interval that will be returned for this datasource. Values:Day, Week, Month, Quarter, Year. If no value is provided all data will be returned.")]
		public string AnalyticDatasourceInterval { get; set; }
		[BindProperty(Name = "analytic_datasource_startdatetime")]
		[JsonProperty("analytic_datasource_startdatetime")]
		[Description("The date and time that this dashboard starts displaying. If not value is provided, the system will use todays data as the end date.")]
		public DateTime? AnalyticDatasourceStartdatetime { get; set; }
		[BindProperty(Name = "company_id")]
		[JsonProperty("company_id")]
		[Description("This is the GUID from the company that will be used as a filter when the data is returned for this datasource.")]
		public string CompanyId { get; set; }
		[BindProperty(Name = "chart_type")]
		[JsonProperty("chart_type")]
		[Description("This is the type of chart being displayed. The ExecuteAnalyticDatasource will use this to format the data appropriately for the chart type.")]
		public string ChartType { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID from the customer who’s data should be used to return the analytic data.")]
		public string CustomerId { get; set; }
		[BindProperty(Name = "additional_filters")]
		[JsonProperty("additional_filters")]
		[Description("Additional Filters to execute the analytic datasource")]
		public string AdditionalFilters { get; set; }
	}

	public class AnalyticDatasourcesFindRequestModel : BaseModel
	{
		[BindProperty(Name = "analytic_datasource_name")]
		[JsonProperty("analytic_datasource_name")]
		[Description("This parameter that allows the user to select all datasources by the name.")]
		public string AnalyticDatasourceName { get; set; }
		[BindProperty(Name = "analytic_datasource_type")]
		[JsonProperty("analytic_datasource_type")]
		[Description("This parameter that allows the user to select all datasources by a specific type.")]
		public string AnalyticDatasourceType { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID that identifies the customer that will return all datasources that are available to the customer_id specified. Default datasources will always be returned with all customer_ids.")]
		public string CustomerId { get; set; }
	}

	public class AnalyticDatasourcesFindRequestModelForService : BaseModel
	{
		[BindProperty(Name = "analytic_datasource_name")]
		[JsonProperty("analytic_datasource_name")]
		[Description("This parameter that allows the user to select all datasources by the name.")]
		public string AnalyticDatasourceName { get; set; }
		[BindProperty(Name = "analytic_datasource_type")]
		[JsonProperty("analytic_datasource_type")]
		[Description("This parameter that allows the user to select all datasources by a specific type.")]
		public string AnalyticDatasourceType { get; set; }
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID that identifies the customer that will return all datasources that are available to the customer_id specified. Default datasources will always be returned with all customer_ids.")]
		public string[] CustomerId { get; set; }
	}

	public class AnalyticDatasourcesGetRequestModel : BaseModel
	{
		[BindProperty(Name = "analytic_datasource_id")]
		[JsonProperty("analytic_datasource_id")]
		[Description("This is the GUID that identifies the datasource to be returned.")]
		public string AnalyticDatasourceId { get; set; }
	}

	public class AnalyticDatasourcesUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_analytic_datasource_id")]
		[JsonProperty("search_analytic_datasource_id")]
		[Description("The system returns the analytic_datasource_id GUID of the datasource that is to be updated.")]
		public string SearchAnalyticDatasourceId { get; set; }

		[BindProperty(Name = "analytic_datasource_type")]
		[JsonProperty("analytic_datasource_type")]
		[Description("This is the type of analytic data returned.  This will be used to categorize different analytics so a user can filter by the type they need.")]
		public string AnalyticDatasourceType { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "company_id")]
		[JsonProperty("company_id")]
		[Description("This is the GUID from the customer_companies table when the analytics is retrieving information for a specific company.")]
		public string CompanyId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "compatible_chart_types")]
		[JsonProperty("compatible_chart_types")]
		[Description("This parameter provides a list of the chart types that can be used with this data set..")]
		public string CompatibleChartTypes { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "customer_id")]
		[JsonProperty("customer_id")]
		[Description("This is the GUID from the customers table that is used to filter data for a specific customer.")]
		public string CustomerId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "analytic_datasource_desc")]
		[JsonProperty("analytic_datasource_desc")]
		[Description("The datasource description")]
		public string AnalyticDatasourceDesc { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "analytic_datasource_enddatetime")]
		[JsonProperty("analytic_datasource_enddatetime")]
		[Description("The date and time that this datasource stops retrieveing data.")]
		public DateTime? AnalyticDatasourceEnddatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "analytic_datasource_interval")]
		[JsonProperty("analytic_datasource_interval")]
		[Description("This parameter identifies the date interval that will be returned for this datasource.  Values:Day, Week, Month, Quarter, Year.   If no value is provided all data will be returned.")]
		public string AnalyticDatasourceInterval { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "analytic_datasource_lambda_arn")]
		[JsonProperty("analytic_datasource_lambda_arn")]
		[Description("The parameter identifies the datasource lambda to be executed if appropriate.")]
		public string AnalyticDatasourceLambdaArn { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "analytic_datasource_name")]
		[JsonProperty("analytic_datasource_name")]
		[Description("The user supplied name of the datasource")]
		public string AnalyticDatasourceName { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "analytic_datasource_sql")]
		[JsonProperty("analytic_datasource_sql")]
		[Description("The parameter identifies the datasource sql to be executed if appropriate.")]
		public string AnalyticDatasourceSql { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "analytic_datasource_startdatetime")]
		[JsonProperty("analytic_datasource_startdatetime")]
		[Description("The date and time that this dashboard starts displaying")]
		public DateTime? AnalyticDatasourceStartdatetime { get; set; } = ApiExtension.UNDEFINED_DATETIME;
		[BindProperty(Name = "analytic_datasource_status")]
		[JsonProperty("analytic_datasource_status")]
		[Description("This is one of the following values:  draft, active, deleted, archived, inactive.")]
		public string AnalyticDatasourceStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
