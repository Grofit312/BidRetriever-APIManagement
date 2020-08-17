using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class SystemSettingManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public SystemSettingManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpPost]
		[Route("CreateSystemSetting")]
		public IActionResult Post(SystemSetting systemSetting)
		{
			try
			{
				// check missing parameter
				var missingParameter = systemSetting.CheckRequiredParameters(new string[]
				{
					"setting_name", "setting_value", "setting_value_data_type"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var systemSettingId = systemSetting.system_setting_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{

					// check if system setting already exists
					cmd.CommandText = "SELECT * FROM system_settings WHERE system_setting_id='" + systemSettingId + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							reader.Close();

							// update existing customer_setting
							cmd.CommandText = "UPDATE system_settings SET "
											+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
											+ "setting_environment = COALESCE(@setting_environment, setting_environment), "
											+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
											+ "setting_group = COALESCE(@setting_group, setting_group), "
											+ "setting_name = COALESCE(@setting_name, setting_name), "
											+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
											+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
											+ "setting_value = COALESCE(@setting_value, setting_value), "
											+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
											+ "status = COALESCE(@status, status), "
											+ "edit_datetime = @edit_datetime "
											+ "WHERE system_setting_id='" + systemSettingId + "'";

							cmd.Parameters.AddWithValue("setting_desc", (object)systemSetting.setting_desc ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_environment", (object)systemSetting.setting_environment ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_help_link", (object)systemSetting.setting_help_link ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_group", (object)systemSetting.setting_group ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_name", (object)systemSetting.setting_name.Trim() ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_sequence", (object)systemSetting.setting_sequence ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_tooltiptext", (object)systemSetting.setting_tooltiptext ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_value", (object)systemSetting.setting_value.Trim() ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_value_data_type", (object)systemSetting.setting_value_data_type ?? DBNull.Value);
							cmd.Parameters.AddWithValue("status", (object)systemSetting.status ?? DBNull.Value);
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								system_setting_id = systemSettingId,
								status = "updated existing setting"
							});
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO system_settings "
						+ "(system_setting_id, setting_desc, setting_environment, setting_help_link, setting_group, "
						+ "setting_name, setting_sequence, setting_tooltiptext, setting_value, setting_value_data_type, status, "
						+ "create_datetime, edit_datetime) "
						+ "VALUES(@system_setting_id, @setting_desc, @setting_environment, @setting_help_link, @setting_group, "
						+ "@setting_name, @setting_sequence, @setting_tooltiptext, @setting_value, @setting_value_data_type, @status, "
						+ "@create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("system_setting_id", systemSettingId.Trim());
					cmd.Parameters.AddWithValue("setting_desc", systemSetting.setting_desc ?? "");
					cmd.Parameters.AddWithValue("setting_environment", systemSetting.setting_environment ?? "");
					cmd.Parameters.AddWithValue("setting_help_link", systemSetting.setting_help_link ?? "");
					cmd.Parameters.AddWithValue("setting_group", systemSetting.setting_group ?? "");
					cmd.Parameters.AddWithValue("setting_name", systemSetting.setting_name.Trim());
					cmd.Parameters.AddWithValue("setting_sequence", systemSetting.setting_sequence);
					cmd.Parameters.AddWithValue("setting_tooltiptext", systemSetting.setting_tooltiptext ?? "");
					cmd.Parameters.AddWithValue("setting_value", systemSetting.setting_value.Trim());
					cmd.Parameters.AddWithValue("setting_value_data_type", systemSetting.setting_value_data_type);
					cmd.Parameters.AddWithValue("status", systemSetting.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						system_setting_id = systemSettingId,
						status = "completed"
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


		[HttpGet]
		[Route("GetSystemSettings")]
		public IActionResult Get(SystemSettingGetRequest request)
		{
			try
			{
				// check missing parameter
				if (request.setting_name == null && request.system_setting_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide setting_name or system_setting_id"
					});
				}

				var detailLevel = request.detail_level ?? "basic";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE ";

					if (request.system_setting_id != null)
					{
						whereString = whereString + "system_setting_id='" + request.system_setting_id + "' AND ";
					}
					if (request.setting_name != null)
					{
						whereString = whereString + "setting_name='" + request.setting_name + "' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT setting_name, setting_value, setting_value_data_type, setting_tooltiptext, system_setting_id,  "
						+ "setting_desc, setting_group, setting_sequence, setting_help_link, create_datetime, edit_datetime, "
						+ "create_user_id, edit_user_id "
						+ "FROM system_settings" + whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								["setting_name"] = _dbHelper.SafeGetString(reader, 0),
								["setting_value"] = _dbHelper.SafeGetString(reader, 1),
								["setting_value_data_type"] = _dbHelper.SafeGetString(reader, 2),
								["setting_tooltiptext"] = _dbHelper.SafeGetString(reader, 3),
								["system_setting_id"] = _dbHelper.SafeGetString(reader, 4),
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								result["setting_desc"] = _dbHelper.SafeGetString(reader, 5);
								result["setting_group"] = _dbHelper.SafeGetString(reader, 6);
								result["setting_sequence"] = _dbHelper.SafeGetInteger(reader, 7);
								result["setting_help_link"] = _dbHelper.SafeGetString(reader, 8);
								result["create_datetime"] = reader.GetValue(9) is DBNull ? "" : ((DateTime)reader.GetValue(9)).ToString();
								result["edit_datetime"] = reader.GetValue(10) is DBNull ? "" : ((DateTime)reader.GetValue(10)).ToString();
							}

							if (detailLevel == "admin")
							{
								result["create_user_id"] = _dbHelper.SafeGetString(reader, 11);
								result["edit_user_id"] = _dbHelper.SafeGetString(reader, 12);
							}

							resultList.Add(result);
						}

						if (resultList.Count == 0)
						{
							return BadRequest(new
							{
								status = "No matching system settings found."
							});
						}
						else if (resultList.Count == 1)
						{
							return Ok(resultList[0]);
						}
						else
						{
							return Ok(resultList);
						}
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


		[HttpPost]
		[Route("UpdateSystemSetting")]
		public IActionResult Post(SystemSettingUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_system_setting_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide search_system_setting_id"
					});
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE system_settings SET "
						+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
						+ "setting_environment = COALESCE(@setting_environment, setting_environment), "
						+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
						+ "setting_group = COALESCE(@setting_group, setting_group), "
						+ "setting_name = COALESCE(@setting_name, setting_name), "
						+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
						+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
						+ "setting_value = COALESCE(@setting_value, setting_value), "
						+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
						+ "status = COALESCE(@status, status), "
						+ "edit_datetime = @edit_datetime "
						+ "WHERE system_setting_id='" + request.search_system_setting_id + "'";

					cmd.Parameters.AddWithValue("setting_desc", (object)request.setting_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_environment", (object)request.setting_environment ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_help_link", (object)request.setting_help_link ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_group", (object)request.setting_group ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_name", (object)request.setting_name.Trim() ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_sequence", (object)request.setting_sequence ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_tooltiptext", (object)request.setting_tooltiptext ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value", (object)request.setting_value.Trim() ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value_data_type", (object)request.setting_value_data_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching system setting found"
						});
					}
					else
					{
						return Ok(new
						{
							status = "completed"
						});
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
