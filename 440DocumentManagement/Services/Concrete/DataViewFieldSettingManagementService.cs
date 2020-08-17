using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Concrete
{
	public class DataViewFieldSettingManagementService : IDataViewFieldSettingManagementService
	{
		private readonly IBaseService _baseService;

		public DataViewFieldSettingManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateDataViewFieldSetting(DatabaseHelper dbHelper, DataViewFieldSettingModel newRecord)
		{
			newRecord.DataViewFieldStatus = newRecord.DataViewFieldStatus ?? "active";
			return _baseService.CreateRecord(newRecord, "data_view_field_settings", "data_view_field_setting_id");
		}

		public List<Dictionary<string, object>> FindDataViewFieldSettings(DatabaseHelper dbHelper, DataViewFieldSettingFindRequestModel request)
		{
			request.DataViewFieldStatus = request.DataViewFieldStatus ?? "active";
			try
			{
				using (var cmd = dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT "
						+ "data_view_field_settings.create_datetime, data_view_field_settings.create_user_id, "
						+ "data_view_field_settings.data_view_field_alignment, "
						+ "data_view_field_settings.data_view_field_display, "
						+ "data_view_field_settings.data_view_field_format, "
						+ "data_view_field_settings.data_view_field_heading, "
						+ "data_view_field_settings.data_view_field_id, "
						+ "data_view_field_settings.data_view_field_sequence, "
						+ "data_view_field_settings.data_view_field_setting_id, "
						+ "data_view_field_settings.data_view_field_sort, "
						+ "data_view_field_settings.data_view_field_sort_sequence, "
						+ "data_view_field_settings.data_view_field_status, "
						+ "data_view_field_settings.data_view_field_type, "
						+ "data_view_field_settings.data_view_field_width, "
						+ "data_view_field_settings.data_view_id, "
						+ "data_view_field_settings.edit_datetime, data_view_field_settings.edit_user_id, "
						+ "data_source_fields.data_source_field_name as data_view_field_name "
						+ "FROM data_view_field_settings "
						+ "LEFT JOIN data_source_fields ON data_view_field_settings.data_view_field_id = data_source_fields.data_source_field_id "
						+ "WHERE data_view_field_settings.data_view_id = @data_view_id AND data_view_field_settings.data_view_field_status = @data_view_field_status";
					cmd.Parameters.AddWithValue("@data_view_id", request.DataViewId);
					cmd.Parameters.AddWithValue("@data_view_field_status", request.DataViewFieldStatus);
					
					var resultList = new List<Dictionary<string, object>>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							resultList.Add(new Dictionary<string, object>
							{
								{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
								{ "create_user_id", Convert.ToString(reader["create_user_id"]) },
								{ "data_view_field_alignment", Convert.ToString(reader["data_view_field_alignment"]) },
								{ "data_view_field_display", Convert.ToString(reader["data_view_field_display"]) },
								{ "data_view_field_format", Convert.ToString(reader["data_view_field_format"]) },
								{ "data_view_field_heading", Convert.ToString(reader["data_view_field_heading"]) },
								{ "data_view_field_id", Convert.ToString(reader["data_view_field_id"]) },
								{ "data_view_field_sequence", Convert.ToString(reader["data_view_field_sequence"]) },
								{ "data_view_field_setting_id", Convert.ToString(reader["data_view_field_setting_id"]) },
								{ "data_view_field_sort", Convert.ToString(reader["data_view_field_sort"]) },
								{ "data_view_field_sort_sequence", Convert.ToString(reader["data_view_field_sort_sequence"]) },
								{ "data_view_field_status", Convert.ToString(reader["data_view_field_status"]) },
								{ "data_view_field_type", Convert.ToString(reader["data_view_field_type"]) },
								{ "data_view_field_width", Convert.ToString(reader["data_view_field_width"]) },
								{ "data_view_id", Convert.ToString(reader["data_view_id"]) },
								{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
								{ "edit_user_id", Convert.ToString(reader["edit_user_id"]) },
								{ "data_view_field_name", Convert.ToString(reader["data_view_field_name"]) },
							});
						}
					}
					return resultList;
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
			//return _baseService.FindRecords<
			//	DataViewFieldSettingFindRequestModel,
			//	DataViewFieldSettingModel,
			//	DataViewFieldSettingModel,
			//	DataViewFieldSettingModel>(dbHelper, request, new string[] { "data_view_field_settings" });
		}

		public DataViewFieldSettingModel GetDataViewFieldSetting(DatabaseHelper dbHelper, DataViewFieldSettingGetRequestModel request)
		{
			return _baseService.GetRecord<DataViewFieldSettingGetRequestModel, DataViewFieldSettingModel>(request, "data_view_field_settings");
		}

		public int UpdateDataViewFieldSetting(DatabaseHelper dbHelper, DataViewFieldSettingUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "data_view_field_settings");
		}
	}
}
