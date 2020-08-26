using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Project Scope Management")]
	public class ProjectScopeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public ProjectScopeManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateProjectScopeOfWork")]
		public IActionResult Post(ProjectScopeOfWork request)
		{
			try
			{
				// Check missing parameters
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"file_id", "file_page_number", "file_page_x1", "file_page_x2", "file_page_y1", "file_page_y2",
					"match_end_char_index", "match_sentence", "match_start_char_index", "match_start_sentence_index",
					"section_code", "section_name", "spec_pages", "section_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// Create record
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "INSERT INTO project_scope_of_work "
						+ "(file_id, file_page_number, file_page_x1, file_page_x2, file_page_y1, file_page_y2, "
						+ "match_end_char_index, match_sentence, match_start_char_index, match_start_sentence_index, "
						+ "project_id, section_code, section_name, spec_pages, section_id) "
						+ "VALUES(@file_id, @file_page_number, @file_page_x1, @file_page_x2, @file_page_y1, @file_page_y2, "
						+ "@match_end_char_index, @match_sentence, @match_start_char_index, @match_start_sentence_index, "
						+ "@project_id, @section_code, @section_name, @spec_pages, @section_id)";

					cmd.Parameters.AddWithValue("file_id", request.file_id);
					cmd.Parameters.AddWithValue("file_page_number", request.file_page_number);
					cmd.Parameters.AddWithValue("file_page_x1", request.file_page_x1);
					cmd.Parameters.AddWithValue("file_page_x2", request.file_page_x2);
					cmd.Parameters.AddWithValue("file_page_y1", request.file_page_y1);
					cmd.Parameters.AddWithValue("file_page_y2", request.file_page_y2);
					cmd.Parameters.AddWithValue("match_end_char_index", request.match_end_char_index);
					cmd.Parameters.AddWithValue("match_sentence", request.match_sentence);
					cmd.Parameters.AddWithValue("match_start_char_index", request.match_start_char_index);
					cmd.Parameters.AddWithValue("match_start_sentence_index", request.match_start_sentence_index);
					cmd.Parameters.AddWithValue("project_id", request.project_id ?? "");
					cmd.Parameters.AddWithValue("section_code", request.section_code);
					cmd.Parameters.AddWithValue("section_name", request.section_name);
					cmd.Parameters.AddWithValue("spec_pages", request.spec_pages);
					cmd.Parameters.AddWithValue("section_id", request.section_id);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
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
		[Route("FindProjectScopeOfWork")]
		public IActionResult get(ProjectScopeGetRequest request)
		{
			try
			{
				if (request.project_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide project_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT file_id, file_page_number, file_page_x1, file_page_x2, file_page_y1, "
						+ "file_page_y2, match_end_char_index, match_sentence, match_start_char_index, "
						+ "match_start_sentence_index, project_id, section_code, section_name, spec_pages, section_id "
						+ "FROM project_scope_of_work "
						+ "WHERE project_id='" + request.project_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, string>
							{
								{ "file_id", _dbHelper.SafeGetString(reader, 0) },
								{ "file_page_number", _dbHelper.SafeGetInteger(reader, 1) },
								{ "file_page_x1", _dbHelper.SafeGetInteger(reader, 2) },
								{ "file_page_x2", _dbHelper.SafeGetInteger(reader, 3) },
								{ "file_page_y1", _dbHelper.SafeGetInteger(reader, 4) },
								{ "file_page_y2", _dbHelper.SafeGetInteger(reader, 5) },
								{ "match_end_char_index", _dbHelper.SafeGetInteger(reader, 6) },
								{ "match_sentence", _dbHelper.SafeGetString(reader, 7) },
								{ "match_start_char_index", _dbHelper.SafeGetInteger(reader, 8) },
								{ "match_start_sentence_index", _dbHelper.SafeGetInteger(reader, 9) },
								{ "project_id", _dbHelper.SafeGetString(reader, 10) },
								{ "section_code", _dbHelper.SafeGetString(reader, 11) },
								{ "section_name", _dbHelper.SafeGetString(reader, 12) },
								{ "spec_pages", _dbHelper.SafeGetInteger(reader, 13) },
								{ "section_id", _dbHelper.SafeGetString(reader, 14) },
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
	}
}
