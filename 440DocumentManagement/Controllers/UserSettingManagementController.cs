using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class UserSettingManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public UserSettingManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateUserSetting")]
		public IActionResult Post(UserSetting userSetting)
		{
			try
			{
				// check missing parameter
				var missingParameter = userSetting.CheckRequiredParameters(new string[]
				{
					"user_id",
					"setting_name",
					"setting_value",
					"setting_value_data_type"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var userSettingId = userSetting.user_setting_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check if user setting exists
					cmd.CommandText = $"SELECT user_setting_id FROM user_settings WHERE user_setting_id='{userSettingId}'";
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							reader.Close();

							// update existing user setting
							cmd.CommandText = "UPDATE user_settings SET "
											+ "user_id = COALESCE(@user_id, user_id), "
											+ "setting_name = COALESCE(@setting_name, setting_name), "
											+ "setting_value = COALESCE(@setting_value, setting_value), "
											+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
											+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
											+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
											+ "setting_group = COALESCE(@setting_group, setting_group), "
											+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
											+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
											+ "status = COALESCE(@status, status), "
											+ "user_device_id = COALESCE(@user_device_id, user_device_id), "
											+ "edit_datetime = @edit_datetime "
											+ $"WHERE user_setting_id='{userSettingId}'";

							cmd.Parameters.AddWithValue("user_id", (object)userSetting.user_id ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_name", (object)userSetting.setting_name ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_value", (object)userSetting.setting_value ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_value_data_type", (object)userSetting.setting_value_data_type ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_tooltiptext", (object)userSetting.setting_tooltiptext ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_desc", (object)userSetting.setting_desc ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_group", (object)userSetting.setting_group ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_sequence", (object)userSetting.setting_sequence ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_help_link", (object)userSetting.setting_help_link ?? DBNull.Value);
							cmd.Parameters.AddWithValue("status", (object)userSetting.status ?? DBNull.Value);
							cmd.Parameters.AddWithValue("user_device_id", (object)userSetting.user_device_id ?? DBNull.Value);
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								user_setting_id = userSettingId,
								status = "updated existing setting"
							});
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO user_settings "
						+ "(user_setting_id, user_device_id, user_id, setting_name, setting_value, setting_value_data_type, "
						+ "setting_group, setting_sequence, setting_tooltiptext, setting_help_link, status, "
						+ "create_datetime, edit_datetime, setting_desc) "
						+ "VALUES(@user_setting_id, @user_device_id, @user_id, @setting_name, @setting_value, @setting_value_data_type, "
						+ "@setting_group, @setting_sequence, @setting_tooltiptext, @setting_help_link, @status, "
						+ "@create_datetime, @edit_datetime, @setting_desc)";

					cmd.Parameters.AddWithValue("user_device_id", userSetting.user_device_id ?? "");
					cmd.Parameters.AddWithValue("user_setting_id", userSettingId);
					cmd.Parameters.AddWithValue("user_id", userSetting.user_id ?? "");
					cmd.Parameters.AddWithValue("setting_name", userSetting.setting_name);
					cmd.Parameters.AddWithValue("setting_value", userSetting.setting_value);
					cmd.Parameters.AddWithValue("setting_value_data_type", userSetting.setting_value_data_type);
					cmd.Parameters.AddWithValue("setting_group", userSetting.setting_group ?? "");
					cmd.Parameters.AddWithValue("setting_sequence", userSetting.setting_sequence ?? "");
					cmd.Parameters.AddWithValue("setting_tooltiptext", userSetting.setting_tooltiptext ?? "");
					cmd.Parameters.AddWithValue("setting_help_link", userSetting.setting_help_link ?? "");
					cmd.Parameters.AddWithValue("status", userSetting.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("setting_desc", userSetting.setting_desc ?? "");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						user_setting_id = userSettingId,
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
		[Route("GetUserSettings")]
		public IActionResult Get(UserSettingsGetRequest request)
		{
			try
			{
				// check missing parameter
				if (request.user_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide user_id"
					});
				}

				var detailLevel = request.detail_level ?? "basic";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = $" WHERE user_settings.user_id='{request.user_id}' AND ";

					whereString += request.user_setting_id == null ? ""
							: $"user_settings.user_setting_id='{request.user_setting_id}' AND ";
					whereString += request.setting_name == null ? ""
							: $"user_settings.setting_name='{request.setting_name}' AND ";

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT user_settings.user_id, users.customer_id, user_settings.user_setting_id, user_settings.setting_name, user_settings.setting_value, "
						+ "user_settings.setting_value_data_type, user_settings.setting_tooltiptext, "
						+ "user_settings.setting_desc, user_settings.setting_group, user_settings.setting_sequence, user_settings.setting_help_link, user_settings.status, "
						+ "user_settings.create_user_id, user_settings.edit_user_id, user_settings.user_device_id "
						+ "FROM user_settings LEFT JOIN users ON users.user_id=user_settings.user_id "
						+ whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								["user_id"] = _dbHelper.SafeGetString(reader, 0),
								["customer_id"] = _dbHelper.SafeGetString(reader, 1),
								["user_setting_id"] = _dbHelper.SafeGetString(reader, 2),
								["setting_name"] = _dbHelper.SafeGetString(reader, 3),
								["setting_value"] = _dbHelper.SafeGetString(reader, 4),
								["setting_value_data_type"] = _dbHelper.SafeGetString(reader, 5),
								["setting_tooltiptext"] = _dbHelper.SafeGetString(reader, 6),
								["user_device_id"] = _dbHelper.SafeGetString(reader, 14)
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								result["setting_desc"] = _dbHelper.SafeGetString(reader, 7);
								result["setting_group"] = _dbHelper.SafeGetString(reader, 8);
								result["setting_sequence"] = _dbHelper.SafeGetString(reader, 9);
								result["setting_help_link"] = _dbHelper.SafeGetString(reader, 10);
								result["status"] = _dbHelper.SafeGetString(reader, 11);
							}

							if (detailLevel == "admin")
							{
								result["create_user_id"] = _dbHelper.SafeGetString(reader, 12);
								result["edit_user_id"] = _dbHelper.SafeGetString(reader, 13);
							}

							resultList.Add(result);
						}
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


		[HttpPost]
		[Route("UpdateUserSetting")]
		public IActionResult Post(UserSettingUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_user_setting_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide search_user_setting_id"
					});
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE user_settings SET "
						+ "setting_name = COALESCE(@setting_name, setting_name), "
						+ "setting_value = COALESCE(@setting_value, setting_value), "
						+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
						+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
						+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
						+ "setting_group = COALESCE(@setting_group, setting_group), "
						+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
						+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
						+ "status = COALESCE(@status, status), "
						+ "user_device_id = COALESCE(@user_device_id, user_device_id), "
						+ "edit_datetime = @edit_datetime "
						+ $"WHERE user_setting_id='{request.search_user_setting_id}'";

					cmd.Parameters.AddWithValue("setting_name", (object)request.setting_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value", (object)request.setting_value ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value_data_type", (object)request.setting_value_data_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_tooltiptext", (object)request.setting_tooltiptext ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_desc", (object)request.setting_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_group", (object)request.setting_group ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_sequence", (object)request.setting_sequence ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_help_link", (object)request.setting_help_link ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_device_id", (object)request.user_device_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching user setting found"
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
