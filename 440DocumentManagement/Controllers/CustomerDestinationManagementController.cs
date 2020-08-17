using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DestinationSystem;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class CustomerDestinationManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public CustomerDestinationManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateCustomerDestination")]
		public IActionResult Post(CustomerDestination customerDestination)
		{
			try
			{
				// check missing parameter
				var missingParameter = customerDestination.CheckRequiredParameters(new string[]
				{
					"customer_id", "destination_type_id", "destination_url"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var destinationId = customerDestination.destination_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{

					// check if customer destination exists
					cmd.CommandText = "SELECT destination_id FROM customer_destinations WHERE destination_id='" + destinationId + "'";
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var existingDestinationId = _dbHelper.SafeGetString(reader, 0);
							reader.Close();

							// update existing customer destination
							cmd.CommandText = "UPDATE customer_destinations SET "
									+ "destination_access_token = COALESCE(@destination_access_token, destination_access_token), "
									+ "customer_id = COALESCE(@customer_id, customer_id), "
									+ "destination_name = COALESCE(@destination_name, destination_name), "
									+ "destination_password = COALESCE(@destination_password, destination_password), "
									+ "destination_root_path = COALESCE(@destination_root_path, destination_root_path), "
									+ "destination_type_id = COALESCE(@destination_type_id, destination_type_id), "
									+ "destination_type_name = COALESCE(@destination_type_name, destination_type_name), "
									+ "destination_url = COALESCE(@destination_url, destination_url), "
									+ "destination_username = COALESCE(@destination_username, destination_username), "
									+ "status = COALESCE(status, @status), "
									+ "edit_datetime = @edit_datetime "
									+ "WHERE destination_id='" + existingDestinationId + "'";

							cmd.Parameters.AddWithValue(
								"destination_access_token",
								(object)customerDestination.destination_access_token ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"customer_id",
								(object)customerDestination.customer_id ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_name",
								(object)customerDestination.destination_name ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_password",
								(object)customerDestination.destination_password ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_root_path",
								(object)customerDestination.destination_root_path ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_type_id",
								(object)customerDestination.destination_type_id ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_type_name",
								(object)customerDestination.destination_type_name ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_url",
								(object)customerDestination.destination_url ?? DBNull.Value);
							cmd.Parameters.AddWithValue(
								"destination_username",
								(object)customerDestination.destination_username ?? DBNull.Value);
							cmd.Parameters.AddWithValue("status", (object)customerDestination.status ?? DBNull.Value);
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								destination_id = existingDestinationId,
								status = "updated existing destination"
							});
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO customer_destinations "
													+ "(destination_id, destination_access_token, customer_id, destination_name, destination_password, "
													+ "destination_root_path, destination_type_id, destination_type_name, destination_url, destination_username, status, "
													+ "create_datetime, edit_datetime) "
													+ "VALUES(@destination_id, @destination_access_token, @customer_id, @destination_name, @destination_password, "
													+ "@destination_root_path, @destination_type_id, @destination_type_name, @destination_url, @destination_username, @status, "
													+ "@create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("destination_id", destinationId);
					cmd.Parameters.AddWithValue("destination_access_token", customerDestination.destination_access_token ?? "");
					cmd.Parameters.AddWithValue("customer_id", customerDestination.customer_id);
					cmd.Parameters.AddWithValue("destination_name", customerDestination.destination_name ?? "");
					cmd.Parameters.AddWithValue("destination_password", customerDestination.destination_password ?? "");
					cmd.Parameters.AddWithValue("destination_root_path", customerDestination.destination_root_path ?? "");
					cmd.Parameters.AddWithValue("destination_type_id", customerDestination.destination_type_id);
					cmd.Parameters.AddWithValue("destination_type_name", customerDestination.destination_type_name ?? "");
					cmd.Parameters.AddWithValue("destination_url", customerDestination.destination_url);
					cmd.Parameters.AddWithValue("destination_username", customerDestination.destination_username ?? "");
					cmd.Parameters.AddWithValue("status", customerDestination.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						destination_id = destinationId,
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
		[Route("FindCustomerDestinations")]
		public IActionResult Get(CustomerDestinationFindRequest request)
		{
			try
			{
				// validation check
				if (request.customer_id == null)
				{
					return BadRequest(new { status = "Please provide customer_id" });
				}

				var customerId = request.customer_id;
				var detailLevel = request.detail_level ?? "basic";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT customer_id, destination_access_token, destination_id, destination_name, "
													+ "destination_password, destination_root_path, destination_type_name, destination_username, "
													+ "destination_type_id, create_datetime, edit_datetime, status, "
													+ "total_access_count, create_user_id, edit_user_id "
													+ "FROM customer_destinations WHERE customer_id='" + customerId + "' AND status='active'";

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>>();
						while (reader.Read())
						{
							var customerDestination = new Dictionary<string, string>
							{
								{ "customer_id", _dbHelper.SafeGetString(reader, 0) },
								{ "destination_access_token", _dbHelper.SafeGetString(reader, 1) },
								{ "destination_id", _dbHelper.SafeGetString(reader, 2) },
								{ "destination_name", _dbHelper.SafeGetString(reader, 3) },
								{ "destination_password", _dbHelper.SafeGetString(reader, 4) },
								{ "destination_root_path", _dbHelper.SafeGetString(reader, 5) },
								{ "destination_type_name", _dbHelper.SafeGetString(reader, 6) },
								{ "destination_username", _dbHelper.SafeGetString(reader, 7) },
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								customerDestination["destination_type_id"] = _dbHelper.SafeGetString(reader, 8);
								customerDestination["create_datetime"] = ((DateTime)reader.GetValue(9)).ToString();
								customerDestination["edit_datetime"] = ((DateTime)reader.GetValue(10)).ToString();
								customerDestination["status"] = _dbHelper.SafeGetString(reader, 11);
							}

							if (detailLevel == "admin")
							{
								customerDestination["total_access_count"] = _dbHelper.SafeGetInteger(reader, 12);
								customerDestination["create_user_id"] = _dbHelper.SafeGetString(reader, 13);
								customerDestination["edit_user_id"] = _dbHelper.SafeGetString(reader, 14);
							}

							result.Add(customerDestination);
						}
						return Ok(result);
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


		[HttpGet]
		[Route("GetCustomerDestination")]
		public IActionResult Get(CustomerDestinationGetRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"destination_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var detailLevel = request.detail_level;

				// retrieve customer destination
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT create_datetime, customer_id, destination_access_token, destination_id, "
													+ "destination_name, destination_password, destination_root_path, destination_type_id, "
													+ "destination_type_name, destination_username, edit_datetime, status, "
													+ "total_access_count, create_user_id, edit_user_id "
													+ "FROM customer_destinations WHERE "
													+ "destination_id='" + request.destination_id + "' AND status='active'";

					var reader = cmd.ExecuteReader();

					if (reader.Read())
					{
						var customerDestination = new Dictionary<string, string>
						{
							{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 0) },
							{ "customer_id", _dbHelper.SafeGetString(reader, 1) },
							{ "destination_access_token", _dbHelper.SafeGetString(reader, 2) },
							{ "destination_id", _dbHelper.SafeGetString(reader, 3) },
							{ "destination_name", _dbHelper.SafeGetString(reader, 4) },
							{ "destination_password", _dbHelper.SafeGetString(reader, 5) },
							{ "destination_root_path", _dbHelper.SafeGetString(reader, 6) },
							{ "destination_type_id", _dbHelper.SafeGetString(reader, 7) },
							{ "destination_type_name", _dbHelper.SafeGetString(reader, 8) },
							{ "destination_username", _dbHelper.SafeGetString(reader, 9) },
							{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 10) },
							{ "status", _dbHelper.SafeGetString(reader, 11) },
						};

						if (detailLevel == "admin")
						{
							customerDestination["total_access_count"] = _dbHelper.SafeGetInteger(reader, 12);
							customerDestination["create_user_id"] = _dbHelper.SafeGetString(reader, 13);
							customerDestination["edit_user_id"] = _dbHelper.SafeGetString(reader, 14);
						}
						return Ok(customerDestination);
					}
					else
					{
						return BadRequest(new
						{
							status = "cannot get customer destination!"
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
		[Route("UpdateCustomerDestination")]
		public IActionResult Post(CustomerDestinationUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_destination_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide search_destination_id"
					});
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE customer_destinations SET "
													+ "destination_access_token = COALESCE(@destination_access_token, destination_access_token), "
													+ "destination_name = COALESCE(@destination_name, destination_name), "
													+ "destination_password = COALESCE(@destination_password, destination_password), "
													+ "destination_root_path = COALESCE(@destination_root_path, destination_root_path), "
													+ "destination_type_id = COALESCE(@destination_type_id, destination_type_id), "
													+ "destination_url = COALESCE(@destination_url, destination_url), "
													+ "destination_username = COALESCE(@destination_username, destination_username), "
													+ "customer_id = COALESCE(@customer_id, customer_id), "
													+ "total_access_count = COALESCE(@total_access_count, total_access_count), "
													+ "status = COALESCE(@status, status), "
													+ "edit_datetime = @edit_datetime "
													+ "WHERE destination_id='" + request.search_destination_id + "'";

					cmd.Parameters.AddWithValue(
						"destination_access_token",
						(object)request.destination_access_token ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"destination_name",
						(object)request.destination_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"destination_password",
						(object)request.destination_password ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"destination_root_path",
						(object)request.destination_root_path ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"destination_type_id",
						(object)request.destination_type_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("destination_url", (object)request.destination_url ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"destination_username",
						(object)request.destination_username ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_id", (object)request.customer_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("total_access_count", (object)request.total_access_count ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching customer destination found"
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
		[Route("RemoveCustomerDestination")]
		public IActionResult Post(CustomerDestinationRemoveRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "destination_id" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE customer_destinations SET status = 'deleted' WHERE destination_id='" + request.destination_id + "'";

					if (cmd.ExecuteNonQuery() > 0)
					{
						return Ok(new
						{
							status = "Customer destination has been removed"
						});
					}
					else
					{
						return BadRequest(new
						{
							status = "Customer destination doesn't exist"
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
