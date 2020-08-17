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
	public class SpecSectionScopeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public SpecSectionScopeManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateSpecSectionScopeOfWork")]
		public IActionResult post(SpecSectionScopeOfWork request)
		{
			try
			{
				// Check missing parameters
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"division_name", "division_number", "search_matches", "search_string", "section_id", "section_name"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var sectionId = request.section_id ?? Guid.NewGuid().ToString();

				// Create record
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "INSERT INTO spec_sections_scope_of_work "
						+ "(csi_spec_number, division_name, division_number, search_matches, search_string, "
						+ "section_id, section_name, section_number, status, section_type, csi_95_search_string) "
						+ "VALUES(@csi_spec_number, @division_name, @division_number, @search_matches, @search_string, "
						+ "@section_id, @section_name, @section_number, @status, @section_type, @csi_95_search_string)";

					cmd.Parameters.AddWithValue("csi_spec_number", request.csi_spec_number ?? "");
					cmd.Parameters.AddWithValue("division_name", request.division_name);
					cmd.Parameters.AddWithValue("division_number", request.division_number);
					cmd.Parameters.AddWithValue("search_matches", request.search_matches);
					cmd.Parameters.AddWithValue("search_string", request.search_string);
					cmd.Parameters.AddWithValue("section_id", sectionId);
					cmd.Parameters.AddWithValue("section_name", request.section_name);
					cmd.Parameters.AddWithValue("section_number", request.section_number ?? "");
					cmd.Parameters.AddWithValue("status", request.status ?? "active");
					cmd.Parameters.AddWithValue("section_type", request.section_type ?? "");
					cmd.Parameters.AddWithValue("csi_95_search_string", request.csi_95_search_string ?? "");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						section_id = sectionId,
						status = "Completed"
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
		[Route("FindSpecSectionScopeOfWork")]
		public IActionResult get(SpecSectionFindRequest request)
		{
			try
			{
				if (request.division_number == null && request.section_number == null && request.section_type == null
					&& request.start_section_number == null && request.end_section_number == null && request.sort_field == null)
				{
					return BadRequest(new
					{
						status = "Please provide query parameter"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE ";

					if (request.section_number != null)
					{
						whereString = whereString + "section_number='" + request.section_number + "' AND ";
					}
					if (request.division_number != null)
					{
						whereString = whereString + "division_number='" + request.division_number + "' AND ";
					}
					if (request.section_type != null)
					{
						whereString = whereString + "section_type='" + request.section_type + "' AND ";
					}
					if (request.start_section_number != null)
					{
						whereString = whereString + "section_number>'" + request.start_section_number + "' AND ";
					}
					if (request.end_section_number != null)
					{
						whereString = whereString + "section_number<'" + request.end_section_number + "' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					var orderString = "";

					if (request.sort_field == "section_number")
					{
						orderString = " ORDER BY section_number ASC";
					}
					else if (request.sort_field == "search_matches")
					{
						orderString = " ORDER BY search_matches ASC";
					}

					cmd.CommandText = "SELECT csi_spec_number, division_name, division_number, search_matches, "
						+ "search_string, section_id, section_name, section_number, status, section_type, csi_95_search_string "
						+ "FROM spec_sections_scope_of_work "
						+ whereString + orderString;

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, string>
							{
								{ "csi_spec_number", _dbHelper.SafeGetString(reader, 0) },
								{ "division_name", _dbHelper.SafeGetString(reader, 1) },
								{ "division_number", _dbHelper.SafeGetString(reader, 2) },
								{ "search_matches", _dbHelper.SafeGetInteger(reader, 3) },
								{ "search_string", _dbHelper.SafeGetString(reader, 4) },
								{ "section_id", _dbHelper.SafeGetString(reader, 5) },
								{ "section_name", _dbHelper.SafeGetString(reader, 6) },
								{ "section_number", _dbHelper.SafeGetString(reader, 7) },
								{ "status", _dbHelper.SafeGetString(reader, 8) },
								{ "section_type", _dbHelper.SafeGetString(reader, 9) },
								{ "csi_95_search_string", _dbHelper.SafeGetString(reader, 10) },
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
		[Route("UpdateSpecSectionScopeOfWork")]
		public IActionResult post(SpecSectionUpdateRequest request)
		{
			try
			{
				// Check required parameter
				if (request.search_section_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide search_section_id"
					});
				}

				// Update record
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE spec_sections_scope_of_work SET "
						+ "csi_spec_number = COALESCE(@csi_spec_number, csi_spec_number), "
						+ "division_name = COALESCE(@division_name, division_name), "
						+ "division_number = COALESCE(@division_number, division_number), "
						+ "search_matches = COALESCE(@search_matches, search_matches), "
						+ "search_string = COALESCE(@search_string, search_string), "
						+ "section_name = COALESCE(@section_name, section_name), "
						+ "section_number = COALESCE(@section_number, section_number), "
						+ "section_type = COALESCE(@section_type, section_type), "
						+ "status = COALESCE(@status, status), "
						+ "csi_95_search_string = COALESCE(@csi_95_search_string, csi_95_search_string) "
						+ "WHERE section_id='" + request.search_section_id + "'";

					cmd.Parameters.AddWithValue("csi_spec_number", (object)request.csi_spec_number ?? DBNull.Value);
					cmd.Parameters.AddWithValue("division_name", (object)request.division_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("division_number", (object)request.division_number ?? DBNull.Value);
					cmd.Parameters.AddWithValue("search_matches", (object)request.search_matches ?? DBNull.Value);
					cmd.Parameters.AddWithValue("search_string", (object)request.search_string ?? DBNull.Value);
					cmd.Parameters.AddWithValue("section_name", (object)request.section_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("section_number", (object)request.section_number ?? DBNull.Value);
					cmd.Parameters.AddWithValue("section_type", (object)request.section_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("csi_95_search_string", (object)request.csi_95_search_string ?? DBNull.Value);

					int countAffected = cmd.ExecuteNonQuery();

					if (countAffected > 0)
					{
						return Ok(new
						{
							section_id = request.search_section_id,
							status = "Updated"
						});
					}
					else
					{
						return BadRequest(new
						{
							status = "Cannot find section_id"
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
