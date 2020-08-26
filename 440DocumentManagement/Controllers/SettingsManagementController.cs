using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Settings Management")]
	public class SettingsManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public SettingsManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpGet]
		[Route("GetCustomerSettings")]
		public IActionResult Get(CustomerSettingsGetRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"customer_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var detailLevel = request.detail_level ?? "basic";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE customer_id='" + request.customer_id + "' AND ";

					if (request.setting_id != null)
					{
						whereString = whereString + "setting_id='" + request.setting_id + "' AND ";
					}
					if (request.setting_name != null)
					{
						whereString = whereString + "setting_name='" + request.setting_name + "' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT customer_id, customer_setting_id, setting_name, setting_value, "
													+ "setting_value_data_type, setting_tooltiptext, setting_id, "
													+ "setting_desc, setting_group, setting_sequence, setting_help_link, "
													+ "create_datetime, edit_datetime, "
													+ "create_user_id, edit_user_id "
													+ "FROM customer_settings" + whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								["customer_id"] = _dbHelper.SafeGetString(reader, 0),
								["customer_setting_id"] = _dbHelper.SafeGetString(reader, 1),
								["setting_name"] = _dbHelper.SafeGetString(reader, 2),
								["setting_value"] = _dbHelper.SafeGetString(reader, 3),
								["setting_value_data_type"] = _dbHelper.SafeGetString(reader, 4),
								["setting_tooltiptext"] = _dbHelper.SafeGetString(reader, 5),
								["setting_id"] = _dbHelper.SafeGetString(reader, 6)
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								result["setting_desc"] = _dbHelper.SafeGetString(reader, 7);
								result["setting_group"] = _dbHelper.SafeGetString(reader, 8);
								result["setting_sequence"] = _dbHelper.SafeGetString(reader, 9);
								result["setting_help_link"] = _dbHelper.SafeGetString(reader, 10);
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
		[Route("CreateCustomerSetting")]
		public IActionResult Post(CustomerSetting customerSetting)
		{
			try
			{
				// check missing parameter
				var missingParameter = customerSetting.CheckRequiredParameters(new string[]
				{
					"setting_id", "setting_name", "setting_value", "setting_value_data_type"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = missingParameter + " is required"
					});
				}

				var customerSettingId = Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{

					// check if customer_setting exists
					if (customerSetting.customer_id != null)
					{
						cmd.CommandText = "SELECT customer_setting_id FROM customer_settings WHERE customer_id='" + customerSetting.customer_id
														+ "' AND setting_id='" + customerSetting.setting_id + "'";
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								var existingCustomerSettingId = _dbHelper.SafeGetString(reader, 0);
								reader.Close();

								// update existing customer_setting
								cmd.CommandText = "UPDATE customer_settings SET "
										+ "setting_name = COALESCE(@setting_name, setting_name), "
										+ "setting_value = COALESCE(@setting_value, setting_value), "
										+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
										+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
										+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
										+ "setting_group = COALESCE(@setting_group, setting_group), "
										+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
										+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
										+ "status = COALESCE(@status, status), "
										+ "edit_datetime = @edit_datetime "
										+ "WHERE customer_setting_id='" + existingCustomerSettingId + "'";

								cmd.Parameters.AddWithValue("setting_name", (object)customerSetting.setting_name ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_value", (object)customerSetting.setting_value ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_value_data_type", (object)customerSetting.setting_value_data_type ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_tooltiptext", (object)customerSetting.setting_tooltiptext ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_desc", (object)customerSetting.setting_desc ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_group", (object)customerSetting.setting_group ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_sequence", (object)customerSetting.setting_sequence ?? DBNull.Value);
								cmd.Parameters.AddWithValue("setting_help_link", (object)customerSetting.setting_help_link ?? DBNull.Value);
								cmd.Parameters.AddWithValue("status", (object)customerSetting.status ?? DBNull.Value);
								cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

								cmd.ExecuteNonQuery();

								return Ok(new { customer_setting_id = existingCustomerSettingId, status = "updated existing setting" });
							}
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO customer_settings "
													+ "(customer_setting_id, customer_id, setting_name, setting_value, setting_value_data_type, "
													+ "setting_group, setting_sequence, setting_tooltiptext, setting_help_link, status, "
													+ "create_datetime, edit_datetime, setting_desc, setting_id) "
													+ "VALUES(@customer_setting_id, @customer_id, @setting_name, @setting_value, @setting_value_data_type, "
													+ "@setting_group, @setting_sequence, @setting_tooltiptext, @setting_help_link, @status, "
													+ "@create_datetime, @edit_datetime, @setting_desc, @setting_id)";

					cmd.Parameters.AddWithValue("customer_setting_id", customerSettingId);
					cmd.Parameters.AddWithValue("customer_id", customerSetting.customer_id ?? "");
					cmd.Parameters.AddWithValue("setting_name", customerSetting.setting_name);
					cmd.Parameters.AddWithValue("setting_value", customerSetting.setting_value);
					cmd.Parameters.AddWithValue("setting_value_data_type", customerSetting.setting_value_data_type);
					cmd.Parameters.AddWithValue("setting_group", customerSetting.setting_group ?? "");
					cmd.Parameters.AddWithValue("setting_sequence", customerSetting.setting_sequence ?? "");
					cmd.Parameters.AddWithValue("setting_tooltiptext", customerSetting.setting_tooltiptext ?? "");
					cmd.Parameters.AddWithValue("setting_help_link", customerSetting.setting_help_link ?? "");
					cmd.Parameters.AddWithValue("status", customerSetting.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("setting_desc", customerSetting.setting_desc ?? "");
					cmd.Parameters.AddWithValue("setting_id", customerSetting.setting_id ?? "");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						customer_setting_id = customerSettingId,
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


		[HttpPost]
		[Route("UpdateCustomerSetting")]
		public IActionResult Post(CustomerSettingUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_customer_setting_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide search_customer_setting_id"
					});
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE customer_settings SET "
													+ "setting_name = COALESCE(@setting_name, setting_name), "
													+ "setting_value = COALESCE(@setting_value, setting_value), "
													+ "setting_value_data_type = COALESCE(@setting_value_data_type, setting_value_data_type), "
													+ "setting_tooltiptext = COALESCE(@setting_tooltiptext, setting_tooltiptext), "
													+ "setting_desc = COALESCE(@setting_desc, setting_desc), "
													+ "setting_group = COALESCE(@setting_group, setting_group), "
													+ "setting_sequence = COALESCE(@setting_sequence, setting_sequence), "
													+ "setting_help_link = COALESCE(@setting_help_link, setting_help_link), "
													+ "status = COALESCE(@status, status), "
													+ "edit_datetime = @edit_datetime "
													+ "WHERE customer_setting_id='" + request.search_customer_setting_id + "'";

					cmd.Parameters.AddWithValue("setting_name", (object)request.setting_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value", (object)request.setting_value ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_value_data_type", (object)request.setting_value_data_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_tooltiptext", (object)request.setting_tooltiptext ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_desc", (object)request.setting_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_group", (object)request.setting_group ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_sequence", (object)request.setting_sequence ?? DBNull.Value);
					cmd.Parameters.AddWithValue("setting_help_link", (object)request.setting_help_link ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching customer setting found"
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
