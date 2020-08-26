using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DestinationType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Destination Type Management")]
	public class DestinationTypeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public DestinationTypeManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpPost]
		[Route("CreateDestinationType")]
		public IActionResult Post(DestinationType destinationType)
		{
			try
			{
				// check missing parameter
				var missingParameter = destinationType.CheckRequiredParameters(new string[]
				{
					"destination_type_name"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var destinationTypeId = destinationType.destination_type_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{

					// check if destination type id exists
					cmd.CommandText = "SELECT destination_type_id FROM destination_types WHERE destination_type_id='" + destinationTypeId + "'";
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							reader.Close();

							// update existing destination type
							cmd.CommandText = "UPDATE destination_types SET "
									+ "destination_type_desc = COALESCE(@destination_type_desc, destination_type_desc), "
									+ "destination_type_domain = COALESCE(@destination_type_domain, destination_type_domain), "
									+ "destination_type_name = COALESCE(@destination_type_name, destination_type_name), "
									+ "status = COALESCE(status, @status), "
									+ "edit_datetime = @edit_datetime "
									+ "WHERE destination_type_id='" + destinationTypeId + "'";

							cmd.Parameters.AddWithValue("destination_type_desc", (object)destinationType.destination_type_desc ?? DBNull.Value);
							cmd.Parameters.AddWithValue("destination_type_domain", (object)destinationType.destination_type_domain ?? DBNull.Value);
							cmd.Parameters.AddWithValue("destination_type_name", (object)destinationType.destination_type_name ?? DBNull.Value);
							cmd.Parameters.AddWithValue("status", (object)destinationType.status ?? DBNull.Value);
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								destination_type_id = destinationTypeId,
								status = "updated existing destination type"
							});
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO destination_types "
													+ "(destination_type_desc, destination_type_domain, destination_type_id, destination_type_name, "
													+ "status, create_datetime, edit_datetime) "
													+ "VALUES(@destination_type_desc, @destination_type_domain, @destination_type_id, @destination_type_name, "
													+ "@status, @create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("destination_type_desc", destinationType.destination_type_desc ?? "");
					cmd.Parameters.AddWithValue("destination_type_domain", destinationType.destination_type_domain ?? "");
					cmd.Parameters.AddWithValue("destination_type_id", destinationTypeId);
					cmd.Parameters.AddWithValue("destination_type_name", destinationType.destination_type_name);
					cmd.Parameters.AddWithValue("status", destinationType.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						destination_type_id = destinationTypeId,
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
		[Route("FindDestinationTypes")]
		public IActionResult Get(DestinationTypeFindRequest request)
		{
			try
			{
				var detailLevel = request.detail_level ?? "basic";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT destination_type_id, destination_type_name, destination_type_domain, "
													+ "create_datetime, edit_datetime, status, "
													+ "create_user_id, edit_user_id "
													+ "FROM destination_types WHERE status='active'";

					if (request.destination_type_id != null)
					{
						cmd.CommandText += " AND destination_type_id='" + request.destination_type_id + "'";
					}

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>>();
						while (reader.Read())
						{
							var destinationType = new Dictionary<string, string>
							{
								{ "destination_type_id", _dbHelper.SafeGetString(reader, 0) },
								{ "destination_type_name", _dbHelper.SafeGetString(reader, 1) },
								{ "destination_type_domain", _dbHelper.SafeGetString(reader, 2) },
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								destinationType["create_datetime"] = ((DateTime)reader.GetValue(3)).ToString();
								destinationType["edit_datetime"] = ((DateTime)reader.GetValue(4)).ToString();
								destinationType["status"] = _dbHelper.SafeGetString(reader, 5);
							}

							if (detailLevel == "admin")
							{
								destinationType["create_user_id"] = _dbHelper.SafeGetString(reader, 6);
								destinationType["edit_user_id"] = _dbHelper.SafeGetString(reader, 7);
							}

							result.Add(destinationType);
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
		[Route("GetDestinationType")]
		public IActionResult Get(DestinationTypeGetRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"destination_type_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new { status = $"{missingParameter} is required" });
				}

				var detailLevel = request.detail_level;

				// retrieve destination type
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT destination_type_id, destination_type_name, destination_type_domain, "
													+ "create_datetime, edit_datetime, status, "
													+ "create_user_id, edit_user_id "
													+ "FROM destination_types WHERE "
													+ "destination_type_id='" + request.destination_type_id + "' AND status='active'";

					var reader = cmd.ExecuteReader();

					if (reader.Read())
					{
						var customerDestination = new Dictionary<string, string>
						{
							{ "destination_type_id", _dbHelper.SafeGetString(reader, 0) },
							{ "destination_type_name", _dbHelper.SafeGetString(reader, 1) },
							{ "destination_type_domain", _dbHelper.SafeGetString(reader, 2) },
							{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 3) },
							{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 4) },
							{ "status", _dbHelper.SafeGetString(reader, 5) },
						};

						if (detailLevel == "admin")
						{
							customerDestination["create_user_id"] = _dbHelper.SafeGetString(reader, 6);
							customerDestination["edit_user_id"] = _dbHelper.SafeGetString(reader, 7);
						}
						return Ok(customerDestination);
					}
					else
					{
						return BadRequest(new
						{
							status = "cannot get destination type!"
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
		[Route("UpdateDestinationType")]
		public IActionResult Post(DestinationTypeUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_destination_type_id == null)
				{
					return BadRequest(new { status = "Please provide search_destination_type_id" });
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE destination_types SET "
													+ "destination_type_name = COALESCE(@destination_type_name, destination_type_name), "
													+ "destination_type_domain = COALESCE(@destination_type_domain, destination_type_domain), "
													+ "destination_type_desc = COALESCE(@destination_type_desc, destination_type_desc), "
													+ "status = COALESCE(@status, status), "
													+ "edit_datetime = @edit_datetime "
													+ "WHERE destination_type_id='" + request.search_destination_type_id + "'";

					cmd.Parameters.AddWithValue("destination_type_name", (object)request.destination_type_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("destination_type_domain", (object)request.destination_type_domain ?? DBNull.Value);
					cmd.Parameters.AddWithValue("destination_type_desc", (object)request.destination_type_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching destination type found"
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
		[Route("RemoveDestinationType")]
		public IActionResult Post(DestinationTypeRemoveRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"destination_type_id"
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
					cmd.CommandText = "UPDATE destination_types SET status = 'deleted' WHERE destination_type_id='" + request.destination_type_id + "'";

					if (cmd.ExecuteNonQuery() > 0)
					{
						return Ok(new
						{
							status = "Destination type has been removed"
						});
					}
					else
					{
						return BadRequest(new
						{
							status = "Destination type doesn't exist"
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
