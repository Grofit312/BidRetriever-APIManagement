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
	[OpenApiTag("Source System Management")]
	public class SourceSystemsManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public SourceSystemsManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateCustomerSourceSystem")]
		public IActionResult Post(CustomerSourceSystem customerSourceSystem)
		{
			try
			{
				// check missing parameter
				var missingParameter = customerSourceSystem.CheckRequiredParameters(new string[]
				{
					"customer_id", "customer_source_sys_name", "source_sys_type_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var sourceSystemId = customerSourceSystem.customer_source_sys_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{

					// check if customer_setting exists
					if (customerSourceSystem.customer_id != null)
					{
						cmd.CommandText = "SELECT customer_source_sys_id FROM customer_source_systems WHERE customer_source_sys_id='" + sourceSystemId + "'";

						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								var existingSourceSystemId = _dbHelper.SafeGetString(reader, 0);
								reader.Close();

								// update existing customer_setting
								cmd.CommandText = "UPDATE customer_source_systems SET "
												+ "customer_id = COALESCE(@customer_id, customer_id), "
												+ "customer_source_sys_name = COALESCE(@customer_source_sys_name, customer_source_sys_name), "
												+ "source_sys_type_id = COALESCE(@source_sys_type_id, source_sys_type_id), "
												+ "system_url = COALESCE(@system_url, system_url), "
												+ "source_sys_url = COALESCE(@source_sys_url, source_sys_url), "
												+ "username = COALESCE(@username, username), "
												+ "password = COALESCE(@password, password), "
												+ "access_token = COALESCE(@access_token, access_token), "
												+ "status = COALESCE(@status, status), "
												+ "edit_datetime = @edit_datetime "
												+ "WHERE customer_source_sys_id='" + existingSourceSystemId + "'";

								cmd.Parameters.AddWithValue("customer_id", (object)customerSourceSystem.customer_id ?? DBNull.Value);
								cmd.Parameters.AddWithValue("customer_source_sys_name", (object)customerSourceSystem.customer_source_sys_name ?? DBNull.Value);
								cmd.Parameters.AddWithValue("source_sys_type_id", (object)customerSourceSystem.source_sys_type_id ?? DBNull.Value);
								cmd.Parameters.AddWithValue("system_url", (object)customerSourceSystem.system_url ?? DBNull.Value);
								cmd.Parameters.AddWithValue("source_sys_url", (object)customerSourceSystem.source_sys_url);
								cmd.Parameters.AddWithValue("username", (object)customerSourceSystem.username ?? DBNull.Value);
								cmd.Parameters.AddWithValue("password", (object)customerSourceSystem.password ?? DBNull.Value);
								cmd.Parameters.AddWithValue("access_token", (object)customerSourceSystem.access_token ?? DBNull.Value);
								cmd.Parameters.AddWithValue("status", (object)customerSourceSystem.status ?? DBNull.Value);
								cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

								cmd.ExecuteNonQuery();

								return Ok(new
								{
									customer_source_sys_id = existingSourceSystemId,
									status = "updated existing source system"
								});
							}
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO customer_source_systems "
						+ "(customer_source_sys_id, customer_id, customer_source_sys_name, source_sys_type_id, system_url, source_sys_url, "
						+ "username, password, access_token, status, total_access_count, last_access_datetime, "
						+ "create_datetime, edit_datetime) "
						+ "VALUES(@customer_source_sys_id, @customer_id, @customer_source_sys_name, @source_sys_type_id, @system_url, @source_sys_url, "
						+ "@username, @password, @access_token, @status, @total_access_count, @last_access_datetime, "
						+ "@create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("customer_source_sys_id", sourceSystemId);
					cmd.Parameters.AddWithValue("customer_id", customerSourceSystem.customer_id);
					cmd.Parameters.AddWithValue("customer_source_sys_name", customerSourceSystem.customer_source_sys_name);
					cmd.Parameters.AddWithValue("source_sys_type_id", customerSourceSystem.source_sys_type_id);
					cmd.Parameters.AddWithValue("system_url", customerSourceSystem.system_url ?? "");
					cmd.Parameters.AddWithValue("source_sys_url", customerSourceSystem.source_sys_url ?? "");
					cmd.Parameters.AddWithValue("username", customerSourceSystem.username ?? "");
					cmd.Parameters.AddWithValue("password", customerSourceSystem.password ?? "");
					cmd.Parameters.AddWithValue("access_token", customerSourceSystem.access_token ?? "");
					cmd.Parameters.AddWithValue("status", customerSourceSystem.status ?? "active");
					cmd.Parameters.AddWithValue("total_access_count", 0);
					cmd.Parameters.AddWithValue("last_access_datetime", DBNull.Value);
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						customer_source_sys_id = sourceSystemId,
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
		[Route("FindCustomerSourceSystems")]
		public IActionResult Get(CustomerSourceSystemsGetRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "customer_id" });

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
					var whereString = $" WHERE customer_id='{request.customer_id}' AND customer_source_systems.status='active' AND ";

					if (request.customer_source_sys_id != null)
					{
						whereString += $"customer_source_sys_id='{request.customer_source_sys_id}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT customer_source_systems.customer_source_sys_id, "
						+ "customer_source_systems.customer_source_sys_name, source_system_types.source_type_name, customer_source_systems.system_url, "
						+ "customer_source_systems.username, customer_source_systems.password, customer_source_systems.access_token, "
						+ "customer_source_systems.source_sys_type_id, customer_source_systems.customer_id, customer_source_systems.create_datetime, customer_source_systems.edit_datetime, customer_source_systems.status,"
						+ "customer_source_systems.total_access_count, customer_source_systems.create_user_id, customer_source_systems.edit_user_id, "
						+ "customer_source_systems.source_sys_url "
						+ "FROM customer_source_systems LEFT JOIN source_system_types ON customer_source_systems.source_sys_type_id=source_system_types.source_sys_type_id"
						+ whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								["customer_source_sys_id"] = _dbHelper.SafeGetString(reader, 0),
								["customer_source_sys_name"] = _dbHelper.SafeGetString(reader, 1),
								["source_sys_type_name"] = _dbHelper.SafeGetString(reader, 2),
								["source_sys_url"] = _dbHelper.SafeGetString(reader, 15),
								["system_url"] = _dbHelper.SafeGetString(reader, 3),
								["username"] = _dbHelper.SafeGetString(reader, 4),
								["password"] = _dbHelper.SafeGetString(reader, 5),
								["access_token"] = _dbHelper.SafeGetString(reader, 6)
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								result["source_sys_type_id"] = _dbHelper.SafeGetString(reader, 7);
								result["customer_id"] = _dbHelper.SafeGetString(reader, 8);
								result["create_datetime"] = reader.GetValue(9) is DBNull ? "" : ((DateTime)reader.GetValue(9)).ToString();
								result["edit_datetime"] = reader.GetValue(10) is DBNull ? "" : ((DateTime)reader.GetValue(10)).ToString();
								result["status"] = _dbHelper.SafeGetString(reader, 11);
							}

							if (detailLevel == "admin")
							{
								result["total_access_count"] = _dbHelper.SafeGetInteger(reader, 12);
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
		[Route("UpdateCustomerSourceSystem")]
		public IActionResult Post(CustomerSourceSystemUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_customer_source_sys_id == null)
				{
					return BadRequest(new { status = "Please provide search_customer_source_sys_id" });
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE customer_source_systems SET "
						+ "customer_source_sys_name = COALESCE(@customer_source_sys_name, customer_source_sys_name), "
						+ "source_sys_type_id = COALESCE(@source_sys_type_id, source_sys_type_id), "
						+ "system_url = COALESCE(@system_url, system_url), "
						+ "source_sys_url = COALESCE(@source_sys_url, source_sys_url), "
						+ "username = COALESCE(@username, username), "
						+ "password = COALESCE(@password, password), "
						+ "access_token = COALESCE(@access_token, access_token), "
						+ "status = COALESCE(@status, status), "
						+ "customer_id = COALESCE(@customer_id, customer_id), "
						+ "total_access_count = COALESCE(@total_access_count, total_access_count), "
						+ "edit_datetime = @edit_datetime "
						+ "WHERE customer_source_sys_id='" + request.search_customer_source_sys_id + "'";

					cmd.Parameters.AddWithValue("customer_source_sys_name", (object)request.customer_source_sys_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("source_sys_type_id", (object)request.source_sys_type_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("system_url", (object)request.system_url ?? DBNull.Value);
					cmd.Parameters.AddWithValue("source_sys_url", (object)request.source_sys_url ?? DBNull.Value);
					cmd.Parameters.AddWithValue("username", (object)request.username ?? DBNull.Value);
					cmd.Parameters.AddWithValue("password", (object)request.password ?? DBNull.Value);
					cmd.Parameters.AddWithValue("access_token", (object)request.access_token ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_id", (object)request.customer_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("total_access_count", (object)request.total_access_count ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching customer source system found"
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


		[HttpPost]
		[Route("RemoveCustomerSourceSystem")]
		public IActionResult Post(CustomerSourceSystemRemoveRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "customer_source_sys_id" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = missingParameter + " is required"
					});
				}

				var sourceSystemId = request.customer_source_sys_id;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE customer_source_systems SET status = 'deleted' WHERE customer_source_sys_id='" + sourceSystemId + "'";

					if (cmd.ExecuteNonQuery() > 0)
					{
						return Ok(new
						{
							status = "Source system account has been deleted"
						});
					}
					else
					{
						return BadRequest(new
						{
							status = "Source system account doesn't exist"
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
