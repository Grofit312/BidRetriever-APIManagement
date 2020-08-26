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
	[OpenApiTag("Source System Type Management")]
	public class SourceSystemTypeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public SourceSystemTypeManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpPost]
		[Route("CreateSourceSystemType")]
		public IActionResult Post(SourceSystemType sourceSystemType)
		{
			try
			{
				// check missing parameter
				var missingParameter = sourceSystemType.CheckRequiredParameters(new string[]
				{
					"source_type_name", "source_type_desc"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = missingParameter + " is required"
					});
				}

				var sourceTypeId = sourceSystemType.source_sys_type_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{

					// check if customer_setting exists
					cmd.CommandText = "SELECT source_sys_type_id FROM source_system_types WHERE source_sys_type_id='" + sourceTypeId + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var existingSourceTypeId = _dbHelper.SafeGetString(reader, 0);
							reader.Close();

							// update existing customer_setting
							cmd.CommandText = "UPDATE source_system_types SET "
											+ "source_type_name = COALESCE(@source_type_name, source_type_name), "
											+ "source_type_desc = COALESCE(@source_type_desc, source_type_desc), "
											+ "source_type_domain = COALESCE(@source_type_domain, source_type_domain), "
											+ "source_type_url = COALESCE(@source_type_url, source_type_url), "
											+ "status = COALESCE(@status, status), "
											+ "edit_datetime = @edit_datetime "
											+ "WHERE source_sys_type_id='" + existingSourceTypeId + "'";

							cmd.Parameters.AddWithValue("source_type_name", (object)sourceSystemType.source_type_name ?? DBNull.Value);
							cmd.Parameters.AddWithValue("source_type_desc", (object)sourceSystemType.source_type_desc ?? DBNull.Value);
							cmd.Parameters.AddWithValue("source_type_domain", (object)sourceSystemType.source_type_domain ?? DBNull.Value);
							cmd.Parameters.AddWithValue("source_type_url", (object)sourceSystemType.source_type_url ?? DBNull.Value);
							cmd.Parameters.AddWithValue("status", (object)sourceSystemType.status ?? DBNull.Value);
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								source_sys_type_id = existingSourceTypeId,
								status = "updated existing source system type"
							});
						}
					}

					// create new one
					cmd.CommandText = "INSERT INTO source_system_types "
						+ "(source_sys_type_id, source_type_name, source_type_desc, source_type_domain, source_type_url, "
						+ "status, total_access_count, last_synch_datetime, "
						+ "create_datetime, edit_datetime) "
						+ "VALUES(@source_sys_type_id, @source_type_name, @source_type_desc, @source_type_domain, @source_type_url, "
						+ "@status, @total_access_count, @last_synch_datetime, "
						+ "@create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("source_sys_type_id", sourceTypeId);
					cmd.Parameters.AddWithValue("source_type_name", sourceSystemType.source_type_name);
					cmd.Parameters.AddWithValue("source_type_desc", sourceSystemType.source_type_desc);
					cmd.Parameters.AddWithValue("source_type_domain", sourceSystemType.source_type_domain ?? "");
					cmd.Parameters.AddWithValue("source_type_url", sourceSystemType.source_type_url ?? "");
					cmd.Parameters.AddWithValue("status", sourceSystemType.status ?? "active");
					cmd.Parameters.AddWithValue("total_access_count", 0);
					cmd.Parameters.AddWithValue("last_synch_datetime", DBNull.Value);
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						source_sys_type_id = sourceTypeId,
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
		[Route("FindSourceSystemType")]
		public IActionResult Get(SourceSystemTypesGetRequest request)
		{
			try
			{
				var detailLevel = request.detail_level ?? "basic";
				var whereString = " WHERE status='active' AND ";

				if (request.source_sys_type_id != null)
				{
					whereString = whereString + "source_sys_type_id='" + request.source_sys_type_id + "' AND ";
				}
				if (request.source_type_name != null)
				{
					whereString = whereString + "source_type_name='" + request.source_type_name + "' AND ";
				}
				if (request.source_type_domain != null)
				{
					whereString = whereString + "source_type_domain='" + request.source_type_domain + "' AND ";
				}
				if (request.source_type_url != null)
				{
					whereString = whereString + "source_type_url='" + request.source_type_url + "' AND ";
				}

				whereString = whereString.Remove(whereString.Length - 5);

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT source_sys_type_id, source_type_name, source_type_domain, source_type_desc, source_type_tooltip, "
						+ "source_type_url, create_datetime, edit_datetime, status, "
						+ "total_access_count, create_user_id, edit_user_id, last_synch_datetime "
						+ "FROM source_system_types"
						+ whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								["source_sys_type_id"] = _dbHelper.SafeGetString(reader, 0),
								["source_type_name"] = _dbHelper.SafeGetString(reader, 1),
								["source_type_domain"] = _dbHelper.SafeGetString(reader, 2),
								["source_type_desc"] = _dbHelper.SafeGetString(reader, 3),
								["source_type_tooltip"] = _dbHelper.SafeGetString(reader, 4),
								["source_type_url"] = _dbHelper.SafeGetString(reader, 5),
								["create_datetime"] = reader.GetValue(6) is DBNull ? "" : ((DateTime)reader.GetValue(6)).ToString(),
								["edit_datetime"] = reader.GetValue(7) is DBNull ? "" : ((DateTime)reader.GetValue(7)).ToString(),
								["status"] = _dbHelper.SafeGetString(reader, 8),
								["total_access_count"] = _dbHelper.SafeGetInteger(reader, 9),
							};

							if (detailLevel == "admin")
							{
								result["create_user_id"] = _dbHelper.SafeGetString(reader, 10);
								result["edit_user_id"] = _dbHelper.SafeGetString(reader, 11);
								result["last_synch_datetime"] = reader.GetValue(12) is DBNull ? "" : ((DateTime)reader.GetValue(12)).ToString();
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
		[Route("UpdateSourceSystemType")]
		public IActionResult Post(SourceSystemTypeUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_source_type_id == null)
				{
					return BadRequest(new { status = "Please provide search_source_type_id" });
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE source_system_types SET "
						+ "source_type_name = COALESCE(@source_type_name, source_type_name), "
						+ "source_type_desc = COALESCE(@source_type_desc, source_type_desc), "
						+ "source_type_domain = COALESCE(@source_type_domain, source_type_domain), "
						+ "source_type_url = COALESCE(@source_type_url, source_type_url), "
						+ "status = COALESCE(@status, status), "
						+ "total_access_count = COALESCE(@total_access_count, total_access_count), "
						+ "last_synch_datetime = COALESCE(@last_synch_datetime, last_synch_datetime), "
						+ "edit_datetime = @edit_datetime "
						+ "WHERE source_sys_type_id='" + request.search_source_type_id + "'";

					cmd.Parameters.AddWithValue("source_type_name", (object)request.source_type_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("source_type_desc", (object)request.source_type_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("source_type_domain", (object)request.source_type_domain ?? DBNull.Value);
					cmd.Parameters.AddWithValue("source_type_url", (object)request.source_type_url ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("total_access_count", (object)request.total_access_count ?? DBNull.Value);
					cmd.Parameters.AddWithValue("last_synch_datetime", request.last_synch_datetime != null ? (object)(DateTimeHelper.ConvertToUTCDateTime(request.last_synch_datetime)) : DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching source system type found"
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
		[Route("RemoveSourceSystemType")]
		public IActionResult Post(SourceSystemTypeRemoveRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "source_sys_type_id" });

				if (missingParameter != null)
				{
					return BadRequest(new { status = missingParameter + " is required" });
				}

				var sourceTypeId = request.source_sys_type_id;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE source_system_types SET status = 'deleted' WHERE source_sys_type_id='" + sourceTypeId + "'";

					if (cmd.ExecuteNonQuery() > 0)
					{
						return Ok(new
						{
							status = "Source system type has been deleted"
						});
					}
					else
					{
						return BadRequest(new
						{
							status = "Source system type doesn't exist"
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
