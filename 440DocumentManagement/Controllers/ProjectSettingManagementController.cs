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
	public class ProjectSettingManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public ProjectSettingManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateProjectSetting")]
		public IActionResult Post(ProjectSetting projectSetting)
		{
			try
			{
				// check missing parameter
				var missingParameter = projectSetting.CheckRequiredParameters(new string[]
				{
					"project_id",
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

				var projectSettingId = projectSetting.project_setting_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check if project setting exists
					cmd.CommandText = $"SELECT project_setting_id FROM project_settings WHERE project_setting_id='{projectSettingId}'";
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							reader.Close();

							// update existing project setting
							cmd.CommandText = "UPDATE project_settings SET "
											+ "project_id = COALESCE(@project_id, project_id), "
											+ "setting_name = COALESCE(@setting_name, setting_name), "
											+ "setting_value = COALESCE(@setting_value, setting_value), "
											+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
											+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
											+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
											+ "setting_group = COALESCE(@setting_group, setting_group), "
											+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
											+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
											+ "setting_environment = COALESCE(@setting_environment, setting_environment), "
											+ "status = COALESCE(@status, status), "
											+ "edit_datetime = @edit_datetime "
											+ $"WHERE project_setting_id='{projectSettingId}'";

							cmd.Parameters.AddWithValue("project_id", (object)projectSetting.project_id ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_environment", (object)projectSetting.setting_environment ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_name", (object)projectSetting.setting_name ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_value", (object)projectSetting.setting_value ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_value_data_type", (object)projectSetting.setting_value_data_type ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_tooltiptext", (object)projectSetting.setting_tooltiptext ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_desc", (object)projectSetting.setting_desc ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_group", (object)projectSetting.setting_group ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_sequence", (object)projectSetting.setting_sequence ?? DBNull.Value);
							cmd.Parameters.AddWithValue("setting_help_link", (object)projectSetting.setting_help_link ?? DBNull.Value);
							cmd.Parameters.AddWithValue("status", (object)projectSetting.status ?? DBNull.Value);
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

							cmd.ExecuteNonQuery();

							return Ok(new { project_setting_id = projectSettingId, status = "updated existing setting" });
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO project_settings "
						+ "(project_setting_id, project_id, setting_name, setting_value, setting_value_data_type, "
						+ "setting_group, setting_sequence, setting_tooltiptext, setting_help_link, setting_environment, status, "
						+ "create_datetime, edit_datetime, setting_desc) "
						+ "VALUES(@project_setting_id, @project_id, @setting_name, @setting_value, @setting_value_data_type, "
						+ "@setting_group, @setting_sequence, @setting_tooltiptext, @setting_help_link, @setting_environment, @status, "
						+ "@create_datetime, @edit_datetime, @setting_desc)";

					cmd.Parameters.AddWithValue("project_setting_id", projectSettingId);
					cmd.Parameters.AddWithValue("project_id", projectSetting.project_id ?? "");
					cmd.Parameters.AddWithValue("setting_name", projectSetting.setting_name);
					cmd.Parameters.AddWithValue("setting_value", projectSetting.setting_value);
					cmd.Parameters.AddWithValue("setting_value_data_type", projectSetting.setting_value_data_type);
					cmd.Parameters.AddWithValue("setting_group", projectSetting.setting_group ?? "");
					cmd.Parameters.AddWithValue("setting_sequence", projectSetting.setting_sequence ?? "");
					cmd.Parameters.AddWithValue("setting_tooltiptext", projectSetting.setting_tooltiptext ?? "");
					cmd.Parameters.AddWithValue("setting_help_link", projectSetting.setting_help_link ?? "");
					cmd.Parameters.AddWithValue("status", projectSetting.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("setting_desc", projectSetting.setting_desc ?? "");
					cmd.Parameters.AddWithValue("setting_environment", projectSetting.setting_environment ?? "");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						project_setting_id = projectSettingId,
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
		[Route("GetProjectSettings")]
		public IActionResult Get(ProjectSettingsGetRequest request)
		{
			try
			{
				// Check Missingn Parameters
				if (request.project_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide project_id"
					});
				}

				var detailLevel = request.detail_level ?? "basic";
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = $" WHERE project_id='{request.project_id}' AND ";

					if (request.project_setting_id != null)
					{
						whereString += "project_setting_id=@project_setting_id AND ";
						cmd.Parameters.AddWithValue("@project_setting_id", request.project_setting_id);
					}
					if (request.setting_name != null)
					{
						whereString += "setting_name=@setting_name AND ";
						cmd.Parameters.AddWithValue("@setting_name", request.setting_name);
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT project_id, project_setting_id, setting_name, setting_value, "
						+ "setting_value_data_type, setting_tooltiptext, "
						+ "setting_desc, setting_group, setting_sequence, setting_help_link, setting_environment, "
						+ "create_datetime, edit_datetime, "
						+ "create_user_id, edit_user_id "
						+ "FROM project_settings" + whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								["project_id"] = _dbHelper.SafeGetString(reader, 0),
								["project_setting_id"] = _dbHelper.SafeGetString(reader, 1),
								["setting_name"] = _dbHelper.SafeGetString(reader, 2),
								["setting_value"] = _dbHelper.SafeGetString(reader, 3),
								["setting_value_data_type"] = _dbHelper.SafeGetString(reader, 4),
								["setting_tooltiptext"] = _dbHelper.SafeGetString(reader, 5),
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								result["setting_desc"] = _dbHelper.SafeGetString(reader, 6);
								result["setting_group"] = _dbHelper.SafeGetString(reader, 7);
								result["setting_sequence"] = _dbHelper.SafeGetString(reader, 8);
								result["setting_help_link"] = _dbHelper.SafeGetString(reader, 9);
								result["setting_environment"] = _dbHelper.SafeGetString(reader, 10);
								result["create_datetime"] = reader.GetValue(11) is DBNull ? "" : ((DateTime)reader.GetValue(11)).ToString();
								result["edit_datetime"] = reader.GetValue(12) is DBNull ? "" : ((DateTime)reader.GetValue(12)).ToString();
							}

							if (detailLevel == "admin")
							{
								result["create_user_id"] = _dbHelper.SafeGetString(reader, 13);
								result["edit_user_id"] = _dbHelper.SafeGetString(reader, 14);
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
		[Route("UpdateProjectSetting")]
		public IActionResult Post(ProjectSettingUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_project_setting_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide search_project_setting_id"
					});
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE project_settings SET "
						+ "setting_name = COALESCE(@setting_name, setting_name), "
						+ "setting_value = COALESCE(@setting_value, setting_value), "
						+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
						+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
						+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
						+ "setting_group = COALESCE(@setting_group, setting_group), "
						+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
						+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
						+ "setting_environment = COALESCE(@setting_environment, setting_environment), "
						+ "status = COALESCE(@status, status), "
						+ "edit_datetime = @edit_datetime "
						+ "WHERE project_setting_id='" + request.search_project_setting_id + "'";

					cmd.Parameters.AddWithValue("setting_name", (object)request.setting_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value", (object)request.setting_value ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value_data_type", (object)request.setting_value_data_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_tooltiptext", (object)request.setting_tooltiptext ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_desc", (object)request.setting_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_group", (object)request.setting_group ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_sequence", (object)request.setting_sequence ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_help_link", (object)request.setting_help_link ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_environment", (object)request.setting_environment ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching project setting found"
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
