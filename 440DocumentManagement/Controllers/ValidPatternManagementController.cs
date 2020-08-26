using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Valid Pattern Management")]
	public class ValidPatternManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public ValidPatternManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateValidPattern")]
		public IActionResult Post(ValidPattern validPattern)
		{
			try
			{
				// missing parameter check
				var missingParameter = validPattern.CheckRequiredParameters(new string[] { "pattern_class", "example", "pattern" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// check if pattern already exists
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM valid_patterns WHERE pattern='{validPattern.pattern}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						// already exists
						cmd.CommandText = $"UPDATE valid_patterns SET occurrences = occurrences + 1 WHERE pattern='{validPattern.pattern}'";
						cmd.ExecuteNonQuery();

						return Ok(new
						{
							status = "completed"
						});
					}
					else
					{
						// not exists
						cmd.CommandText = "INSERT INTO valid_patterns (pattern_class, example, occurrences, pattern, stripped_pattern, pattern_length) "
							+ "VALUES(@pattern_class, @example, @occurrences, @pattern, @stripped_pattern, @pattern_length)";

						cmd.Parameters.AddWithValue("pattern_class", validPattern.pattern_class);
						cmd.Parameters.AddWithValue("example", validPattern.example);
						cmd.Parameters.AddWithValue("pattern", validPattern.pattern);
						cmd.Parameters.AddWithValue("stripped_pattern", validPattern.pattern.Replace("S", ""));
						cmd.Parameters.AddWithValue("occurrences", 1);
						cmd.Parameters.AddWithValue("pattern_length", validPattern.pattern.Length);

						cmd.ExecuteNonQuery();

						return Ok(new
						{
							status = "completed "
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
		[Route("UpdateValidPattern")]
		public IActionResult Post(ValidPatternUpdateRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.pattern))
				{
					return BadRequest(new
					{
						status = "Please provide pattern"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"UPDATE valid_patterns SET occurrences = occurrences + 1 WHERE pattern='{request.pattern}'";

					var result = cmd.ExecuteNonQuery();

					if (result == 0)
					{
						return BadRequest(new
						{
							status = "pattern doesn't exist"
						});
					}

					return Ok(new
					{
						status = "updated"
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
		[Route("FindValidPatterns")]
		public IActionResult Get(ValidPatternFindRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT pattern_class, example, occurrences, pattern, stripped_pattern, pattern_length FROM valid_patterns";

					if (!string.IsNullOrEmpty(request.pattern_class))
					{
						cmd.CommandText += $" WHERE pattern_class='{request.pattern_class}'";
					}
					cmd.CommandText += " ORDER BY occurrences DESC";

					if (request.limit != null)
					{
						cmd.CommandText += $" LIMIT {request.limit}";
					}

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, object>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								{ "pattern_class", _dbHelper.SafeGetString(reader, 0) },
								{ "example", _dbHelper.SafeGetString(reader, 1) },
								{ "occurrences", _dbHelper.SafeGetIntegerRaw(reader, 2) },
								{ "pattern", _dbHelper.SafeGetString(reader, 3) },
								{ "stripped_pattern", _dbHelper.SafeGetString(reader, 4) },
								{ "pattern_length", _dbHelper.SafeGetIntegerRaw(reader, 5) },
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


		[HttpGet]
		[Route("GetValidPattern")]
		public IActionResult Get(ValidPatternGetRequest request)
		{
			try
			{
				if (request.pattern == null && request.stripped_pattern == null)
				{
					return BadRequest(new
					{
						status = "Please provide pattern or stripped pattern"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = "WHERE";

					if (request.pattern == null)
					{
						where += $" stripped_pattern='{request.stripped_pattern}'";
					}
					else
					{
						where += $" pattern='{request.pattern}'";
					}

					cmd.CommandText = "SELECT pattern_class, example, occurrences, pattern, stripped_pattern, pattern_length FROM valid_patterns "
						+ where;

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new Dictionary<string, object>
							{
								{ "pattern_class", _dbHelper.SafeGetString(reader, 0) },
								{ "example", _dbHelper.SafeGetString(reader, 1) },
								{ "occurrences", _dbHelper.SafeGetIntegerRaw(reader, 2) },
								{ "pattern", _dbHelper.SafeGetString(reader, 3) },
								{ "stripped_pattern", _dbHelper.SafeGetString(reader, 4) },
								{ "pattern_length", _dbHelper.SafeGetIntegerRaw(reader, 5) },
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "pattern not found"
							});
						}
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
		[Route("CreateValidSheetNumber")]
		public IActionResult Post(ValidSheetNumber validSheetNumber)
		{
			try
			{
				// missing parameter check
				var missingParameter = validSheetNumber.CheckRequiredParameters(new string[] { "sheet_number", "ocr", "manual" });

				if (missingParameter != null)
				{
					return BadRequest(new { status = $"{missingParameter} is required" });
				}

				// check if already exists
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM sheet_number_library WHERE sheet_number='{validSheetNumber.sheet_number}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						// already exists
						if (validSheetNumber.manual && validSheetNumber.ocr)
						{
							cmd.CommandText = "UPDATE sheet_number_library SET edit_datetime = @edit_datetime, manual_occurrences = manual_occurrences + 1, ocr_occurrences = ocr_occurrences + 1 WHERE sheet_number='" + validSheetNumber.sheet_number + "'";
						}
						else if (validSheetNumber.manual)
						{
							cmd.CommandText = "UPDATE sheet_number_library SET edit_datetime = @edit_datetime, manual_occurrences = manual_occurrences + 1 WHERE sheet_number='" + validSheetNumber.sheet_number + "'";
						}
						else if (validSheetNumber.ocr)
						{
							cmd.CommandText = "UPDATE sheet_number_library SET edit_datetime = @edit_datetime, ocr_occurrences = ocr_occurrences + 1 WHERE sheet_number='" + validSheetNumber.sheet_number + "'";
						}

						cmd.Parameters.AddWithValue("edit_datetime", timestamp);

						cmd.ExecuteNonQuery();

						return Ok(new { status = "completed" });
					}
					else
					{
						// not exists
						var sheetNumberStripped = Regex.Replace(validSheetNumber.sheet_number, @"[\.\-\x5F]", "");
						var sheetNumberPattern = Regex.Replace(validSheetNumber.sheet_number, @"[A-Za-z]", "A");
						sheetNumberPattern = Regex.Replace(sheetNumberPattern, @"[\.\-\x5F]", "S");
						sheetNumberPattern = Regex.Replace(sheetNumberPattern, @"[0-9\-]", "N");
						var strippedPattern = sheetNumberPattern.Replace("S", "");

						cmd.CommandText = "INSERT INTO sheet_number_library (sheet_number, sheet_number_stripped, sheet_number_pattern, manual_occurrences, ocr_occurrences, stripped_pattern, create_datetime, edit_datetime) "
																						+ "VALUES(@sheet_number, @sheet_number_stripped, @sheet_number_pattern, @manual_occurrences, @ocr_occurrences, @stripped_pattern, @create_datetime, @edit_datetime)";

						cmd.Parameters.AddWithValue("sheet_number", validSheetNumber.sheet_number);
						cmd.Parameters.AddWithValue("sheet_number_stripped", sheetNumberStripped);
						cmd.Parameters.AddWithValue("sheet_number_pattern", sheetNumberPattern);
						cmd.Parameters.AddWithValue("manual_occurrences", validSheetNumber.manual ? 1 : 0);
						cmd.Parameters.AddWithValue("ocr_occurrences", validSheetNumber.ocr ? 1 : 0);
						cmd.Parameters.AddWithValue("stripped_pattern", strippedPattern);
						cmd.Parameters.AddWithValue("create_datetime", timestamp);
						cmd.Parameters.AddWithValue("edit_datetime", timestamp);

						cmd.ExecuteNonQuery();

						return Ok(new
						{
							status = "completed "
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
		[Route("FindValidSheetNumbers")]
		public IActionResult Get(ValidSheetNumberFindRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT sheet_number, sheet_number_stripped, sheet_number_pattern, manual_occurrences, ocr_occurrences, stripped_pattern, create_datetime, edit_datetime FROM sheet_number_library";

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, object>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								{ "sheet_number", _dbHelper.SafeGetString(reader, 0) },
								{ "sheet_number_stripped", _dbHelper.SafeGetString(reader, 1) },
								{ "sheet_number_pattern", _dbHelper.SafeGetString(reader, 2) },
								{ "manual_occurrences", _dbHelper.SafeGetIntegerRaw(reader, 3) },
								{ "ocr_occurrences", _dbHelper.SafeGetIntegerRaw(reader, 4) },
								{ "stripped_pattern", _dbHelper.SafeGetString(reader, 5) },
								{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 6) },
								{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 7) },
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
		[Route("CreateValidSheetNameWord")]
		public IActionResult Post(ValidSheetNameWord request)
		{
			try
			{
				// missing parameter check
				var missingParameter = request.CheckRequiredParameters(new string[] { "sheet_name_word", "sheet_name_word_abbrv", "ocr", "manual" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// check if already exists
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM sheet_name_word_library WHERE LOWER(sheet_name_word)='{request.sheet_name_word.ToLower()}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						// already exists
						if (request.manual && request.ocr)
						{
							cmd.CommandText = "UPDATE sheet_name_word_library SET sheet_name_manual_occurrences = sheet_name_manual_occurrences + 1, sheet_name_ocr_occurrences = sheet_name_ocr_occurrences + 1 WHERE LOWER(sheet_name_word)='" + request.sheet_name_word.ToLower() + "'";
						}
						else if (request.manual)
						{
							cmd.CommandText = "UPDATE sheet_name_word_library SET sheet_name_manual_occurrences = sheet_name_manual_occurrences + 1 WHERE LOWER(sheet_name_word)='" + request.sheet_name_word.ToLower() + "'";
						}
						else if (request.ocr)
						{
							cmd.CommandText = "UPDATE sheet_name_word_library SET sheet_name_ocr_occurrences = sheet_name_ocr_occurrences + 1 WHERE LOWER(sheet_name_word)='" + request.sheet_name_word.ToLower() + "'";
						}

						cmd.ExecuteNonQuery();

						return Ok(new
						{
							status = "completed"
						});
					}
					else
					{
						// not exists
						cmd.CommandText = "INSERT INTO sheet_name_word_library (sheet_name_word, sheet_name_word_abbrv, sheet_name_manual_occurrences, sheet_name_ocr_occurrences) "
																						+ "VALUES(@sheet_name_word, @sheet_name_word_abbrv, @sheet_name_manual_occurrences, @sheet_name_ocr_occurrences)";

						cmd.Parameters.AddWithValue("sheet_name_word", request.sheet_name_word.ToUpper());
						cmd.Parameters.AddWithValue("sheet_name_word_abbrv", request.sheet_name_word_abbrv.ToUpper());
						cmd.Parameters.AddWithValue("sheet_name_manual_occurrences", request.manual ? 1 : 0);
						cmd.Parameters.AddWithValue("sheet_name_ocr_occurrences", request.ocr ? 1 : 0);

						cmd.ExecuteNonQuery();

						return Ok(new
						{
							status = "completed "
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
		[Route("GetSheetNameWord")]
		public IActionResult Get(SheetNameWordGetRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.sheet_name_word))
				{
					return Ok(new
					{
						status = "Please provide sheet_name_word"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT sheet_name_word, sheet_name_word_abbrv, sheet_name_manual_occurrences, "
																					+ "sheet_name_ocr_occurrences, invalid_word_flag FROM sheet_name_word_library "
																					+ "WHERE LOWER(sheet_name_word)='" + request.sheet_name_word.ToLower() + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new Dictionary<string, object>
							{
								{ "sheet_name_word", _dbHelper.SafeGetString(reader, 0) },
								{ "sheet_name_word_abbrv", _dbHelper.SafeGetString(reader, 1) },
								{ "sheet_name_manual_occurrences", _dbHelper.SafeGetIntegerRaw(reader, 2) },
								{ "sheet_name_ocr_occurrences", _dbHelper.SafeGetIntegerRaw(reader, 3) },
								{ "invalid_word_flag", _dbHelper.SafeGetBooleanRaw(reader, 4) },
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "sheet_name_word not found"
							});
						}
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
