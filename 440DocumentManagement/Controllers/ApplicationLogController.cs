using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Application Log Management")]
	public class ApplicationLogController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public ApplicationLogController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpPost]
		[Route("LogAppOpp")]
		public IActionResult Post(AppTransactionLog log)
		{
			try
			{
				var logId = log.log_id ?? Guid.NewGuid().ToString();

				// check log_id duplication
				if (__checkLogExists(logId) == true)
				{
					return Ok(new
					{
						log_id = logId,
						status = "duplicated"
					});
				}

				// write log record
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var columns = "(log_id, submission_id, routine_name, routine_version, project_id, document_id, "
											+ "operation_name, operation_datetime, operation_data, notification_id, user_id, "
											+ "customer_id, create_datetime, edit_datetime, function_name, file_id, "
											+ "operation_status_desc, operation_status, transaction_level, device_id)";
					var values = "(@log_id, @submission_id, @routine_name, @routine_version, @project_id, @document_id, "
											+ "@operation_name, @operation_datetime, @operation_data, @notification_id, @user_id, "
											+ "@customer_id, @create_datetime, @edit_datetime, @function_name, @file_id, "
											+ "@operation_status_desc, @operation_status, @transaction_level, @device_id)";

					cmd.CommandText = $"INSERT INTO app_transaction_log {columns} VALUES{values}";

					cmd.Parameters.AddWithValue("log_id", logId);
					cmd.Parameters.AddWithValue("submission_id", log.submission_id ?? "");
					cmd.Parameters.AddWithValue("routine_name", log.routine_name ?? "");
					cmd.Parameters.AddWithValue("routine_version", log.routine_version ?? "");
					cmd.Parameters.AddWithValue("project_id", log.project_id ?? "");
					cmd.Parameters.AddWithValue("document_id", log.document_id ?? "");
					cmd.Parameters.AddWithValue("operation_name", log.operation_name ?? "");
					cmd.Parameters.AddWithValue("operation_datetime", DateTimeHelper.ConvertToUTCDateTime(log.operation_datetime));
					cmd.Parameters.AddWithValue("operation_data", log.operation_data ?? "");
					cmd.Parameters.AddWithValue("notification_id", log.notification_id ?? "");
					cmd.Parameters.AddWithValue("user_id", log.user_id ?? "");
					cmd.Parameters.AddWithValue("customer_id", log.customer_id ?? "");
					cmd.Parameters.AddWithValue("create_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("function_name", log.function_name ?? "");
					cmd.Parameters.AddWithValue("file_id", log.file_id ?? "");
					cmd.Parameters.AddWithValue("operation_status_desc", log.operation_status_desc ?? "");
					cmd.Parameters.AddWithValue("operation_status", log.operation_status ?? "active");
					cmd.Parameters.AddWithValue("transaction_level", log.transaction_level ?? "transaction");
					cmd.Parameters.AddWithValue("device_id", log.device_id ?? "");

					cmd.ExecuteNonQuery();
				}

				return Ok(new
				{
					log_id = logId,
					status = "completed"
				});
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
		[Route("FindLog")]
		public IActionResult Get(LogFindRequest request)
		{
			try
			{
				var detailLevel = request.detail_level ?? "basic";

				// validation check
				if (request.doc_id == null
					&& request.file_id == null
					&& request.function_name == null
					&& request.operation_name == null
					&& request.operation_status == null
					&& request.project_id == null
					&& request.submission_id == null)
				{
					return BadRequest(new
					{
						status = "please provide at least one query parameter"
					});
				}

				// run query
				using (var cmd = _dbHelper.SpawnCommand())
				{
					// build query string
					var whereString = " WHERE ";

					if (request.doc_id != null)
					{
						whereString += $"app_transaction_log.document_id='{request.doc_id}' AND ";
					}
					if (request.file_id != null)
					{
						whereString += $"app_transaction_log.file_id='{request.file_id}' AND ";
					}
					if (request.function_name != null)
					{
						whereString += $"app_transaction_log.function_name='{request.function_name}' AND ";
					}
					if (request.operation_name != null)
					{
						whereString += $"app_transaction_log.operation_name='{request.operation_name}' AND ";
					}
					if (request.project_id != null)
					{
						whereString += $"app_transaction_log.project_id='{request.project_id}' AND ";
					}
					if (request.submission_id != null)
					{
						whereString += $"app_transaction_log.submission_id='{request.submission_id}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					var limit = request.limit == -1 ? "" : $"LIMIT {request.limit}";

					cmd.CommandText = "SELECT app_transaction_log.create_datetime, app_transaction_log.customer_id, app_transaction_log.document_id, "
													+ "project_documents.doc_name, project_documents.doc_number, project_documents.doc_revision, "
													+ "app_transaction_log.edit_datetime, app_transaction_log.file_id, files.file_type, "
													+ "app_transaction_log.function_name, app_transaction_log.log_id, app_transaction_log.operation_data, "
													+ "app_transaction_log.operation_datetime, app_transaction_log.operation_name, app_transaction_log.operation_status, "
													+ "app_transaction_log.operation_status_desc, app_transaction_log.project_id, projects.project_name, app_transaction_log.routine_name, "
													+ "app_transaction_log.routine_version, app_transaction_log.submission_id, app_transaction_log.user_id, users.user_email, project_submissions.submission_name, app_transaction_log.transaction_level, project_documents.doc_type,  "
													+ "user_devices.device_name, users.user_firstname, users.user_lastname "
													+ "FROM app_transaction_log "
													+ "LEFT OUTER JOIN project_documents ON project_documents.doc_id=app_transaction_log.document_id "
													+ "LEFT OUTER JOIN files ON files.file_id=app_transaction_log.file_id "
													+ "LEFT OUTER JOIN projects ON projects.project_id=app_transaction_log.project_id "
													+ "LEFT OUTER JOIN users ON users.user_id=app_transaction_log.user_id "
													+ "LEFT OUTER JOIN project_submissions on project_submissions.project_submission_id=app_transaction_log.submission_id "
													+ "LEFT OUTER JOIN user_devices ON user_devices.user_device_id=app_transaction_log.device_id "
													+ whereString
													+ $" ORDER BY app_transaction_log.operation_datetime {(request.desc ? "DESC" : "ASC")} OFFSET {request.offset} {limit}";

					// execute query
					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>();
							var userFirstName = _dbHelper.SafeGetString(reader, 27);
							var userLastName = _dbHelper.SafeGetString(reader, 28);
							var userDisplayName = "";

							if (!string.IsNullOrEmpty(userFirstName) && !string.IsNullOrEmpty(userLastName))
							{
								userDisplayName = $"{userLastName}, {userFirstName}";
							}
							else
							{
								userDisplayName = $"{userLastName}{userFirstName}".Trim();
							}

							result.Add("create_datetime", _dbHelper.SafeGetDatetimeString(reader, 0));
							result.Add("customer_id", _dbHelper.SafeGetString(reader, 1));
							result.Add("document_id", _dbHelper.SafeGetString(reader, 2));
							result.Add("document_name", _dbHelper.SafeGetString(reader, 3));
							result.Add("document_number", _dbHelper.SafeGetString(reader, 4));
							result.Add("document_revision", _dbHelper.SafeGetString(reader, 5));
							result.Add("edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 6));
							result.Add("file_id", _dbHelper.SafeGetString(reader, 7));
							result.Add("file_type", _dbHelper.SafeGetString(reader, 8));
							result.Add("function_name", _dbHelper.SafeGetString(reader, 9));
							result.Add("log_id", _dbHelper.SafeGetString(reader, 10));
							result.Add("operation_data", _dbHelper.SafeGetString(reader, 11));
							result.Add("operation_datetime", _dbHelper.SafeGetDatetimeString(reader, 12));
							result.Add("operation_name", _dbHelper.SafeGetString(reader, 13));
							result.Add("operation_status", _dbHelper.SafeGetString(reader, 14));
							result.Add("operation_status_desc", _dbHelper.SafeGetString(reader, 15));
							result.Add("project_id", _dbHelper.SafeGetString(reader, 16));
							result.Add("project_name", _dbHelper.SafeGetString(reader, 17));
							result.Add("routine_name", _dbHelper.SafeGetString(reader, 18));
							result.Add("routine_version", _dbHelper.SafeGetString(reader, 19));
							result.Add("submission_id", _dbHelper.SafeGetString(reader, 20));
							result.Add("user_id", _dbHelper.SafeGetString(reader, 21));
							result.Add("user_email", _dbHelper.SafeGetString(reader, 22));
							result.Add("submission_name", _dbHelper.SafeGetString(reader, 23));
							result.Add("transaction_level", _dbHelper.SafeGetString(reader, 24));
							result.Add("document_type", _dbHelper.SafeGetString(reader, 25));
							result.Add("device_name", _dbHelper.SafeGetString(reader, 26));
							result.Add("user_displayname", userDisplayName);

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
		[Route("DeleteLog")]
		public IActionResult Post(LogDeleteRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.submission_id))
				{
					return BadRequest(new
					{
						status = "Please provide submission_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"DELETE FROM app_transaction_log WHERE submission_id='{request.submission_id}'";
					var deletedCount = cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = $"Deleted {deletedCount} record(s)"
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


		private bool __checkLogExists(string logId)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT EXISTS (SELECT true FROM app_transaction_log WHERE log_id='{logId}')";
				return (bool)cmd.ExecuteScalar();
			}
		}
	}
}
