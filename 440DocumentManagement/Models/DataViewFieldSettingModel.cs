using _440DocumentManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace _440DocumentManagement.Models
{
	public class DataViewFieldSettingModel : BaseModel
	{
		[BindProperty(Name = "data_view_field_setting_id")]
		[JsonProperty("data_view_field_setting_id")]
		public string DataViewFieldSettingId { get; set; }
		[BindProperty(Name = "data_view_field_id")]
		[JsonProperty("data_view_field_id")]
		public string DataViewFieldId { get; set; }
		[BindProperty(Name = "data_view_id")]
		[JsonProperty("data_view_id")]
		public string DataViewId { get; set; }
		[BindProperty(Name = "data_view_field_heading")]
		[JsonProperty("data_view_field_heading")]
		public string DataViewFieldHeading { get; set; }
		[BindProperty(Name = "data_view_field_alignment")]
		[JsonProperty("data_view_field_alignment")]
		public string DataViewFieldAlignment { get; set; }
		[BindProperty(Name = "data_view_field_sequence")]
		[JsonProperty("data_view_field_sequence")]
		public int? DataViewFieldSequence { get; set; }
		[BindProperty(Name = "data_view_field_width")]
		[JsonProperty("data_view_field_width")]
		public int? DataViewFieldWidth { get; set; }
		[BindProperty(Name = "data_view_field_format")]
		[JsonProperty("data_view_field_format")]
		public string DataViewFieldFormat { get; set; }
		[BindProperty(Name = "data_view_field_sort")]
		[JsonProperty("data_view_field_sort")]
		public string DataViewFieldSort { get; set; }
		[BindProperty(Name = "data_view_field_status")]
		[JsonProperty("data_view_field_status")]
		public string DataViewFieldStatus { get; set; }
		[BindProperty(Name = "data_view_field_display")]
		[JsonProperty("data_view_field_display")]
		public string DataViewFieldDisplay { get; set; }
		[BindProperty(Name = "data_view_field_sort_sequence")]
		[JsonProperty("data_view_field_sort_sequence")]
		public int? DataViewFieldSortSequence { get; set; }
		[BindProperty(Name = "data_view_field_type")]
		[JsonProperty("data_view_field_type")]
		public string DataViewFieldType { get; set; }
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

	public class DataViewFieldSettingDeleteRequestModel : BaseModel
	{
		[BindProperty(Name = "data_view_field_setting_id")]
		public string DataViewFieldSettingId { get; set; }
	}

	public class DataViewFieldSettingFindRequestModel : BaseModel
	{
		[BindProperty(Name = "data_view_id")]
		public string DataViewId { get; set; }
		[BindProperty(Name = "data_view_field_status")]
		public string DataViewFieldStatus { get; set; }
	}

	public class DataViewFieldSettingGetRequestModel : BaseModel
	{
		[BindProperty(Name = "data_view_field_setting_id")]
		public string DataViewFieldSettingId { get; set; }
	}

	public class DataViewFieldSettingUpdateRequestModel : BaseModel
	{
		[BindProperty(Name = "search_data_view_field_setting_id")]
		public string SearchDataViewFieldSettingId { get; set; }

		[BindProperty(Name = "data_view_field_id")]
		public string DataViewFieldId { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_field_alignment")]
		public string DataViewFieldAlignment { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_field_format")]
		public string DataViewFieldFormat { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_field_heading")]
		public string DataViewFieldHeading { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_field_sequence")]
		public int? DataViewFieldSequence { get; set; }
		[BindProperty(Name = "data_view_field_sort")]
		public string DataViewFieldSort { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_field_sort_sequence")]
		public int? DataViewFieldSortSequence { get; set; }
		[BindProperty(Name = "data_view_field_display")]
		public string DataViewFieldDisplay { get; set; } = ApiExtension.UNDEFINED_STRING;
		[BindProperty(Name = "data_view_field_width")]
		public int? DataViewFieldWidth { get; set; }
		[BindProperty(Name = "data_view_field_status")]
		public string DataViewFieldStatus { get; set; } = ApiExtension.UNDEFINED_STRING;
	}
}
