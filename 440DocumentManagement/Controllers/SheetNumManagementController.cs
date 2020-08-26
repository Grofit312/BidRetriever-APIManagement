using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.SheetNumManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Sheet Num Management")]
	public class SheetNumManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		public SheetNumManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateSheetnumCandidate")]
		public IActionResult CreateSheetnumCandidate(SheetNumCandidate request)
		{
			try
			{
				if (request != null)
				{
					DateTime _date = DateTime.UtcNow;

					//verify required fields
					var missingParameter = request.CheckRequiredParameters(new string[] { "candidate_id" });

					if (missingParameter != null)
					{
						return BadRequest(new
						{
							status = missingParameter + " is required"
						});
					}

					using (var cmd = _dbHelper.SpawnCommand())
					{
						string query = @"INSERT INTO sheetnum_candidates(
	actual_sheetnum, candidate_id, corrected_confidence, corrected_word_text, discipline, discipline_class, filename, file_id, match_font_type, match_pattern, match_status, match_text, match_x1, match_x2, match_y1, match_y2, original_word_text, project_id, stripped_actual, stripped_confidence, stripped_word_pattern, stripped_word_text, test_number, corrected_word_pattern, create_datetime, match_font_size, original_confidence, original_sequence, original_word_pattern) VALUES (@actual_sheetnum, @candidate_id, @corrected_confidence, @corrected_word_text, @discipline, @discipline_class, @filename, @file_id, @match_font_type, @match_pattern, @match_status, @match_text, @match_x1, @match_x2, @match_y1, @match_y2, @original_word_text, @project_id, @stripped_actual, @stripped_confidence, @stripped_word_pattern, @stripped_word_text, @test_number, @corrected_word_pattern, @create_datetime, @match_font_size, @original_confidence, @original_sequence, @original_word_pattern);";
						cmd.CommandText = query;
						cmd.Parameters.AddWithValue("@actual_sheetnum", (object)request.actual_sheetnum ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@candidate_id", request.candidate_id);
						cmd.Parameters.AddWithValue("@corrected_confidence", request.corrected_confidence);
						cmd.Parameters.AddWithValue("@corrected_word_pattern", request.corrected_word_pattern);
						cmd.Parameters.AddWithValue("@corrected_word_text", (object)request.corrected_word_text ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@create_datetime", _date);
						cmd.Parameters.AddWithValue("@discipline", (object)request.discipline ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@discipline_class", (object)request.discipline_class ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@filename", (object)request.filename ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@file_id", (object)request.file_id ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@match_font_size", request.match_font_size);
						cmd.Parameters.AddWithValue("@match_font_type", (object)request.match_font_type ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@match_pattern", (object)request.match_pattern ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@match_status", (object)request.match_status ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@match_text", (object)request.match_text ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@match_x1", request.match_x1);
						cmd.Parameters.AddWithValue("@match_x2", request.match_x2);
						cmd.Parameters.AddWithValue("@match_y1", request.match_y1);
						cmd.Parameters.AddWithValue("@match_y2", request.match_y2);
						cmd.Parameters.AddWithValue("@original_confidence", request.original_confidence);
						cmd.Parameters.AddWithValue("@original_sequence", request.original_sequence);
						cmd.Parameters.AddWithValue("@original_word_pattern", (object)request.original_word_pattern ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@original_word_text", (object)request.original_word_text ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@project_id", (object)request.project_id ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@stripped_actual", (object)request.stripped_actual ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@stripped_confidence", request.stripped_confidence);
						cmd.Parameters.AddWithValue("@stripped_word_pattern", (object)request.stripped_word_pattern ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@stripped_word_text", (object)request.stripped_word_text ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@test_number", (object)request.test_number ?? DBNull.Value);
						cmd.ExecuteNonQuery();
					}
					return Ok(new
					{
						status = "Success",
						request.candidate_id
					});
				}
				else
				{
					return BadRequest(new
					{
						status = "Request can't contains null"
					});
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					status = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}
	}
}
