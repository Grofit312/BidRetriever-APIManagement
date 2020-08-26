using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Data View Field Setting Management")]
	public class DataViewFieldSettingManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		private readonly IDataViewFieldSettingManagementService _dataViewFieldSettingManagementService;

		public DataViewFieldSettingManagementController(
			IDataViewFieldSettingManagementService dataViewFieldSettingManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_dataViewFieldSettingManagementService = dataViewFieldSettingManagementService;
		}

		[HttpPost]
		[Route("CreateDataViewFieldSetting")]
		public IActionResult CreateDataViewFieldSetting(DataViewFieldSettingModel newRecord)
		{
			try
			{
				var missingParameter = newRecord.CheckRequiredParameters(new string[]
				{
					"DataViewFieldHeading", "DataViewId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var newRecordId = _dataViewFieldSettingManagementService.CreateDataViewFieldSetting(_dbHelper, newRecord);
				return Ok(new
				{
					status = "completed",
					data_view_field_setting_id = newRecordId
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpPost]
		[Route("DeleteDataViewFieldSetting")]
		public IActionResult DeleteDataViewFieldSetting(DataViewFieldSettingDeleteRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"DataViewFieldSettingId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "DELETE FROM data_view_field_settings WHERE data_view_field_setting_id = @data_view_field_setting_id";
					cmd.Parameters.AddWithValue("@data_view_field_setting_id", request.DataViewFieldSettingId);
					cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = "completed",
						message = "The data view field setting is deleted successfully"
					});
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("FindDataViewFieldSettings")]
		public IActionResult FindDataViewFieldSettings(DataViewFieldSettingFindRequestModel request)
		{
			try
			{
				var missingParamter = request.CheckRequiredParameters(new string[]
				{
					"DataViewId"
				});
				if (!string.IsNullOrEmpty(missingParamter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParamter} is required"
					});
				}

				var result = _dataViewFieldSettingManagementService.FindDataViewFieldSettings(_dbHelper, request);
				return Ok(result);
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("GetDataViewFieldSetting")]
		public IActionResult GetDataViewFieldSetting(DataViewFieldSettingGetRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"DataViewFieldSettingId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var record = _dataViewFieldSettingManagementService.GetDataViewFieldSetting(_dbHelper, request);
				return Ok(record);
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpPost]
		[Route("UpdateDataViewFieldSetting")]
		public IActionResult UpdateDataViewFieldSetting(DataViewFieldSettingUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchDataViewFieldSettingId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var affectedRowCount = _dataViewFieldSettingManagementService.UpdateDataViewFieldSetting(_dbHelper, request);
				return Ok(new
				{
					status = "completed",
					message = $"{affectedRowCount} rows are updated"
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}
	}
}
