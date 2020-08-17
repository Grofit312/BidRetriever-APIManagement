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
	public class ValidDisciplineManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public ValidDisciplineManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateDiscipline")]
		public IActionResult Post(Discipline discipline)
		{
			try
			{
				if (string.IsNullOrEmpty(discipline.discipline_name))
				{
					return BadRequest(new
					{
						status = "Please provide discipline_name"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var timestamp = DateTime.UtcNow;
					var disciplineId = Guid.NewGuid().ToString();

					cmd.CommandText = "INSERT INTO valid_disciplines "
									+ "(discipline_id, discipline_name, discipline_prefix, customer_id, status, create_datetime, edit_datetime) "
									+ "VALUES(@discipline_id, @discipline_name, @discipline_prefix, @customer_id, @status, @create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("discipline_id", disciplineId);
					cmd.Parameters.AddWithValue("discipline_name", discipline.discipline_name);
					cmd.Parameters.AddWithValue("discipline_prefix", discipline.discipline_prefix ?? "");
					cmd.Parameters.AddWithValue("customer_id", discipline.customer_id ?? "");
					cmd.Parameters.AddWithValue("status", discipline.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						discipline_id = disciplineId,
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
		[Route("GetDiscipline")]
		public IActionResult Get(DisciplineGetRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = "";

					if (!string.IsNullOrEmpty(request.discipline_id))
					{
						where = $" WHERE discipline_id='{request.discipline_id}'";
					}
					else if (!string.IsNullOrEmpty(request.discipline_name))
					{
						where = $" WHERE discipline_name='{request.discipline_name}'";
					}
					else
					{
						return BadRequest(new { status = "Please provide discipline_id or discipline_name" });
					}

					cmd.CommandText = "SELECT class, create_userid, create_datetime, confidence, discipline_id, "
									+ "discipline_name, discipline_prefix, customer_id, edit_userid, edit_datetime, occurances "
									+ "FROM valid_disciplines " + where;

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new Dictionary<string, object>
							{
								{ "class", _dbHelper.SafeGetString(reader, 0) },
								{ "create_user_id", _dbHelper.SafeGetString(reader, 1) },
								{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 2) },
								{ "confidence", _dbHelper.SafeGetIntegerRaw(reader, 3) },
								{ "discipline_id", _dbHelper.SafeGetString(reader, 4) },
								{ "discipline_name", _dbHelper.SafeGetString(reader, 5) },
								{ "discipline_prefix", _dbHelper.SafeGetString(reader, 6) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 7) },
								{ "edit_user_id", _dbHelper.SafeGetString(reader, 8) },
								{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 9) },
								{ "occurances", _dbHelper.SafeGetIntegerRaw(reader, 10) },
							});
						}

						return BadRequest(new
						{
							status = "discipline not found"
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

		[HttpGet]
		[Route("FindDisciplines")]
		public IActionResult Get(DisciplineFindRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = "WHERE 1=1 AND ";

					if (!string.IsNullOrEmpty(request.discipline_id))
					{
						where += $"discipline_id='{request.discipline_id}' AND ";
					}
					if (!string.IsNullOrEmpty(request.customer_id))
					{
						where += $"customer_id='{request.customer_id}' AND ";
					}

					where = where.Remove(where.Length - 5);

					cmd.CommandText = "SELECT class, create_userid, create_datetime, confidence, discipline_id, "
									+ "discipline_name, discipline_prefix, customer_id, edit_userid, edit_datetime, occurances "
									+ "FROM valid_disciplines " + where;

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, object>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								{ "class", _dbHelper.SafeGetString(reader, 0) },
								{ "create_user_id", _dbHelper.SafeGetString(reader, 1) },
								{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 2) },
								{ "confidence", _dbHelper.SafeGetIntegerRaw(reader, 3) },
								{ "discipline_id", _dbHelper.SafeGetString(reader, 4) },
								{ "discipline_name", _dbHelper.SafeGetString(reader, 5) },
								{ "discipline_prefix", _dbHelper.SafeGetString(reader, 6) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 7) },
								{ "edit_user_id", _dbHelper.SafeGetString(reader, 8) },
								{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 9) },
								{ "occurances", _dbHelper.SafeGetIntegerRaw(reader, 10) },
							});
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

		[HttpPost]
		[Route("UpdateDiscipline")]
		public IActionResult Post(DisciplineUpdateRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = "";

					if (!string.IsNullOrEmpty(request.search_discipline_id))
					{
						where = $" WHERE discipline_id='{request.search_discipline_id}'";
					}
					else if (!string.IsNullOrEmpty(request.search_discipline_name))
					{
						where = $" WHERE discipline_name='{request.search_discipline_name}'";
					}
					else
					{
						return BadRequest(new { status = "Please provide search criteria" });
					}

					cmd.CommandText = "UPDATE valid_disciplines SET "
									+ "confidence = COALESCE(@confidence, confidence), "
									+ "discipline_name = COALESCE(@discipline_name, discipline_name), "
									+ "discipline_prefix = COALESCE(@discipline_prefix, discipline_prefix), "
									+ "occurances = COALESCE(@occurances, occurances), "
									+ "status = COALESCE(@status, status), "
									+ "edit_datetime = @edit_datetime" + where;

					cmd.Parameters.AddWithValue("confidence", (object)request.confidence ?? DBNull.Value);
					cmd.Parameters.AddWithValue("discipline_name", (object)request.discipline_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("discipline_prefix", (object)request.discipline_prefix ?? DBNull.Value);
					cmd.Parameters.AddWithValue("occurances", (object)request.occurances ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					var updatedRows = cmd.ExecuteNonQuery();

					if (updatedRows > 0)
					{
						return Ok(new
						{
							status = "updated"
						});
					}

					return BadRequest(new
					{
						status = "no matching disciplines found"
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
	}
}
