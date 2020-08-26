using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Models.Dashboard;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SDAPI.Models.Dashboard;

namespace SDAPI.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Dashboard Management")]
	public class DashboardManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		private readonly IDashboardManagementService _dashboardManagementService;

		public DashboardManagementController(
			IDashboardManagementService dashboardManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_dashboardManagementService = dashboardManagementService;
		}


		[HttpPost]
		[Route("CreateDashboard")]
		[OpenApiOperation("Create a new dashboard record", "Create a new dashboard record")]
		[ProducesResponseType(typeof(DashboardCreateResponseModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult CreateDashboard(DashboardModel newRecord)
		{
			try
			{
				if (newRecord == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Can't insert null value"
					});
				}

				// Verify the Required Fields
				var missingParameter = newRecord.CheckRequiredParameters(new string[]
				{
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var newRecordId = _dashboardManagementService.CreateRecord(newRecord);
				return Ok(new DashboardCreateResponseModel
				{
					Status = Constants.ApiStatus.SUCCESS,
					DashboardId = newRecordId
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("FindDashboards")]
		[OpenApiOperation(
			"Search for dashboard records",
			"<p>Returns a list of All dashboards available for the customer and returns all information about the dashboard.</p>"
			+ "<p>This routine is used by applications that allow a user to select from all predefind.</p>"
			+ "<p>It provides them with a list of the available Dashboards.</p>"
			+ "<p style='color: red'>Note: default dashboards will have a customer_id = `default`, so all queries should return the default DataSources.</p>"
		)]
		[ProducesResponseType(typeof(List<DashboardModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult FindDashboards(DashboardFindRequestModel request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Request can't be null."
					});
				}

				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[] { });
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var records = _dashboardManagementService.FindRecords(request);
				return Ok(records);
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}
		
		[HttpGet]
		[Route("GetDashboard")]
		[OpenApiOperation(
			"Gets the specified dashboard",
			"Locates the Dashboard specified and returns all information about the Dashboard including all defined fields."
		)]
		[ProducesResponseType(typeof(DashboardModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult GetDashboard(DashboardGetRequestModel request)
		{
			try
			{
				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[] { "DashboardId" });
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var record = _dashboardManagementService.GetRecord(request);
				if (record == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"The dashboard record with id ({request.DashboardId}) is not existed."
					});
				}

				return Ok(record);
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpPost]
		[Route("UpdateDashboard")]
		[OpenApiOperation(
			"This API call allows an application to update an existing Dashboard",
			"This API call allows an application to update an existing Dashboard"
		)]
		[ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult UpdateDashboard(DashboardUpdateRequestModel request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Request Can't contains null"
					});
				}

				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchDashboardId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseResponseModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				int affectedRowCount = _dashboardManagementService.UpdateRecords(request);
				if (affectedRowCount == 0)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"No matching record found for dashboard_id = {request.SearchDashboardId}"
					});
				}

				return Ok(new BaseResponseModel
				{
					Status = Constants.ApiStatus.SUCCESS,
					Message = $"{affectedRowCount} records are updated successfully."
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpPost]
		[Route("RegisterDevice")]
		public IActionResult Post(Device device)
		{
			try
			{
				// check missing parameter
				var missingParameter = device.CheckRequiredParameters(new string[] {
					"device_mac_address", "device_serial_number", "device_type_id",
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// get next device_id
					cmd.CommandText = "SELECT COUNT(*) FROM public.\"SD_devices\"";

					var count = (long)cmd.ExecuteScalar();
					var device_id = (count + 1).ToString("D6");

					// create dashboard record
					var columns = "(device_id, customer_id, device_mac_address, device_name, device_night_end_time, device_night_start_time, "
											+ "device_serial_number, device_status, device_timezone, device_type_id, "
											+ "device_update_frequency, device_wireless_password, device_wireless_ssid, user_email)";
					var values = "(@device_id, @customer_id, @device_mac_address, @device_name, @device_night_end_time, @device_night_start_time, "
											+ "@device_serial_number, @device_status, @device_timezone, @device_type_id, "
											+ "@device_update_frequency, @device_wireless_password, @device_wireless_ssid, @user_email)";

					cmd.CommandText = "INSERT INTO public.\"SD_devices\" " + columns + " VALUES" + values;

					cmd.Parameters.AddWithValue("device_id", device_id);
					cmd.Parameters.AddWithValue("customer_id", device.customer_id ?? "");
					cmd.Parameters.AddWithValue("device_mac_address", device.device_mac_address);
					cmd.Parameters.AddWithValue("device_name", device.device_name ?? "");
					cmd.Parameters.AddWithValue("device_night_end_time", device.device_night_end_time == null ? DBNull.Value : (object)DateTimeHelper.ConvertToUTCDateTime(device.device_night_end_time));
					cmd.Parameters.AddWithValue("device_night_start_time", device.device_night_start_time == null ? DBNull.Value : (object)DateTimeHelper.ConvertToUTCDateTime(device.device_night_start_time));
					cmd.Parameters.AddWithValue("device_serial_number", device.device_serial_number);
					cmd.Parameters.AddWithValue("device_status", device.device_status ?? "active");
					cmd.Parameters.AddWithValue("device_timezone", device.device_timezone ?? "");
					cmd.Parameters.AddWithValue("device_type_id", device.device_type_id);
					cmd.Parameters.AddWithValue("device_update_frequency", device.device_update_frequency);
					cmd.Parameters.AddWithValue("device_wireless_password", device.device_wireless_password ?? "");
					cmd.Parameters.AddWithValue("device_wireless_ssid", device.device_wireless_ssid ?? "");
					cmd.Parameters.AddWithValue("user_email", device.user_email ?? "");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						device_id,
						status = "completed",
					});
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("AssignDashboard")]
		public IActionResult Post(DashboardAssignRequest request, bool isInternalRequest = false)
		{
			try
			{
				// validation check
				if (request.dashboard_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide dashboard_id"
					});
				}

				if (request.device_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide device_id"
					});
				}

				// create record
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = "INSERT INTO public.\"SD_device_dashboards\" "
													+ "(device_id, dashboard_id, create_datetime, edit_datetime, status) "
													+ "VALUES(@device_id, @dashboard_id, @create_datetime, @edit_datetime, @status)";

					cmd.Parameters.AddWithValue("device_id", request.device_id);
					cmd.Parameters.AddWithValue("dashboard_id", request.dashboard_id);
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("status", "active");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = "completed",
					});
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				if (!isInternalRequest)
				{
					_dbHelper.CloseConnection();
				}
			}
		}
		
		[HttpGet]
		[Route("GetDashboards")]
		public IActionResult Get(DashboardGetRequest request)
		{
			try
			{
				// validation check
				if (request.customer_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide customer_id"
					});
				}

				var newDashboards = request.new_dashboards;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE \"SD_dashboards\".customer_id='" + request.customer_id + "' AND ";

					if (newDashboards)
					{
						whereString += "\"SD_dashboards\".dashboard_end_datetime > now() AND \"SD_device_dashboards\".download_datetime IS NULL AND ";
					}
					else
					{
						whereString += "\"SD_dashboards\".dashboard_end_datetime > now() AND ";
					}

					if (request.device_id != null)
					{
						whereString += "\"SD_device_dashboards\".device_id='" + request.device_id + "' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT \"SD_devices\".device_name, \"SD_devices\".device_status, \"SD_device_dashboards\".device_id, \"SD_device_dashboards\".download_datetime, "
													+ "\"SD_dashboards\".customer_id, \"SD_dashboards\".dashboard_create_datetime, \"SD_dashboards\".dashboard_create_userid, "
													+ "\"SD_dashboards\".dashboard_edit_datetime, \"SD_dashboards\".dashboard_edit_userid, \"SD_dashboards\".dashboard_end_datetime, "
													+ "\"SD_dashboards\".dashboard_file_bucketname, \"SD_dashboards\".dashboard_file_key, \"SD_dashboards\".dashboard_id, \"SD_dashboards\".dashboard_name, "
													+ "\"SD_dashboards\".dashboard_start_datetime, \"SD_dashboards\".dashboard_status, \"SD_dashboards\".dashboard_template_id, \"SD_dashboards\".dashboard_type, "
													+ "\"SD_dashboards\".dashboard_version_number, \"SD_device_dashboards\".downloaded_version "
													+ "FROM \"SD_dashboards\" "
													+ "RIGHT OUTER JOIN \"SD_device_dashboards\" ON \"SD_device_dashboards\".dashboard_id=\"SD_dashboards\".dashboard_id "
													+ "RIGHT OUTER JOIN \"SD_devices\" ON \"SD_devices\".device_id=\"SD_device_dashboards\".device_id "
													+ whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								{ "device_name", _dbHelper.SafeGetString(reader, 0) },
								{ "device_status", _dbHelper.SafeGetString(reader, 1) },
								{ "device_id", _dbHelper.SafeGetString(reader, 2) },
								{ "download_datetime", _dbHelper.SafeGetDatetimeString(reader, 3) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 4) },
								{ "dashboard_create_datetime", _dbHelper.SafeGetDatetimeString(reader, 5)},
								{ "dashboard_create_userid", _dbHelper.SafeGetString(reader, 6) },
								{ "dashboard_edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 7) },
								{ "dashboard_edit_userid", _dbHelper.SafeGetString(reader, 8) },
								{ "dashboard_end_datetime", _dbHelper.SafeGetDatetimeString(reader, 9) },
								{ "dashboard_file_bucketname", _dbHelper.SafeGetString(reader, 10) },
								{ "dashboard_file_key", _dbHelper.SafeGetString(reader, 11) },
								{ "dashboard_id", _dbHelper.SafeGetString(reader, 12) },
								{ "dashboard_name", _dbHelper.SafeGetString(reader, 13) },
								{ "dashboard_start_datetime", _dbHelper.SafeGetDatetimeString(reader, 14) },
								{ "dashboard_status", _dbHelper.SafeGetString(reader, 15) },
								{ "dashboard_template_id", _dbHelper.SafeGetString(reader, 16) },
								{ "dashboard_type", _dbHelper.SafeGetString(reader, 17) },
								{ "dashboard_version_number", _dbHelper.SafeGetInteger(reader, 18) },
								{ "downloaded_version", _dbHelper.SafeGetString(reader, 19) },
							};

							resultList.Add(result);
						}

						reader.Close();
						return Ok(resultList);
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}
	}
}
