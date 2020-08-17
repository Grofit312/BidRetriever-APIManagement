using System;
using System.Collections.Generic;
using System.Reflection;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.Log;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class DeviceLogManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		public DeviceLogManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("LogDeviceOpp")]
		public IActionResult LogDeviceOpp(DeviceTransactionLog log)
		{
			try
			{
				var logId = log.log_id ?? Guid.NewGuid().ToString();

				// check log_id duplication
				if (__checkDeviceLogExists(logId) == true)
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
					var columns = "(create_datetime, customer_id, document_id, edit_datetime, file_id, function_name, log_id, notification_id, operation_data, operation_datetime, operation_name, operation_status, operation_status_desc, project_id, routine_name, routine_version, submission_id, user_id, transaction_level, device_id)";
					var values = "(@create_datetime, @customer_id, @document_id, @edit_datetime, @file_id, @function_name, @log_id, @notification_id, @operation_data, @operation_datetime, @operation_name, @operation_status, @operation_status_desc, @project_id, @routine_name, @routine_version, @submission_id, @user_id, @transaction_level, @device_id)";

					cmd.CommandText = $"INSERT INTO public.device_transaction_log {columns} VALUES{values}";

					cmd.Parameters.AddWithValue("@create_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@customer_id", log.customer_id ?? Guid.NewGuid().ToString());
					cmd.Parameters.AddWithValue("@document_id", log.document_id ?? "");
					cmd.Parameters.AddWithValue("@edit_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@file_id", log.file_id ?? "");
					cmd.Parameters.AddWithValue("@function_name", log.function_name ?? "");
					cmd.Parameters.AddWithValue("@log_id", logId);
					cmd.Parameters.AddWithValue("@notification_id", log.notification_id ?? "");
					cmd.Parameters.AddWithValue("@operation_data", log.operation_data ?? "");
					cmd.Parameters.AddWithValue("@operation_datetime", log.operation_datetime ?? DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@operation_name", log.operation_name ?? "");
					cmd.Parameters.AddWithValue("@operation_status", log.operation_status ?? "active");
					cmd.Parameters.AddWithValue("@operation_status_desc", log.operation_status_desc ?? "");
					cmd.Parameters.AddWithValue("@project_id", log.project_id ?? "");
					cmd.Parameters.AddWithValue("@routine_name", log.routine_name ?? "");
					cmd.Parameters.AddWithValue("@routine_version", log.routine_version ?? "");
					cmd.Parameters.AddWithValue("@submission_id", log.submission_id ?? "");
					cmd.Parameters.AddWithValue("@user_id", log.user_id ?? Guid.NewGuid().ToString());
					cmd.Parameters.AddWithValue("@transaction_level", log.transaction_level ?? "transaction");
					cmd.Parameters.AddWithValue("@device_id", log.user_device_id ?? Guid.NewGuid().ToString());

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
		private bool __checkDeviceLogExists(string logId)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT EXISTS (SELECT true FROM device_transaction_log WHERE log_id='{logId}')";
				return (bool)cmd.ExecuteScalar();
			}
		}

		[HttpGet]
		[Route("FindDeviceLog")]
		public IActionResult FindDeviceLog(FindDeviceLogCreteria request)
		{
			try
			{
				List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

				int i = 0;
				PropertyInfo[] properties = request.GetType().GetProperties();
				foreach (PropertyInfo property in properties)
				{
					if (property.GetValue(request) != null)
					{
						i++;
					}
				}
				if (i > 1)
				{
					return BadRequest(new
					{
						status = "Please pass only one argument at a time."
					});
				}

				//string query = @"SELECT ""public"".projects.project_name, ""public"".device_transaction_log.create_datetime, ""public"".project_submissions.submission_name, ""public"".device_transaction_log.function_name, ""public"".device_transaction_log.operation_name, ""public"".device_transaction_log.operation_status, ""public"".device_transaction_log.operation_status_desc, ""public"".device_transaction_log.operation_data, ""public"".project_documents.doc_name,""public"".project_documents.doc_number FROM ""public"".device_transaction_log LEFT OUTER JOIN ""public"".projects ON ""public"".device_transaction_log.project_id= ""public"".projects.project_id LEFT OUTER JOIN ""public"".project_submissions ON ""public"".project_submissions.project_submission_id = ""public"".device_transaction_log.submission_id LEFT OUTER JOIN ""public"".project_documents ON ""public"".project_documents.doc_id = ""public"".device_transaction_log.document_id";
				string query = @"SELECT * FROM ""public"".device_transaction_log LEFT OUTER JOIN ""public"".projects ON ""public"".device_transaction_log.project_id= ""public"".projects.project_id LEFT OUTER JOIN ""public"".project_submissions ON ""public"".project_submissions.project_submission_id = ""public"".device_transaction_log.submission_id LEFT OUTER JOIN ""public"".project_documents ON ""public"".project_documents.doc_id = ""public"".device_transaction_log.document_id";
				string where = string.Empty;
				using (var cmd = _dbHelper.SpawnCommand())
				{
					if (!string.IsNullOrEmpty(request.doc_id))
					{
						where = @" where ""public"".device_transaction_log.doc_id = @doc_id";
						cmd.Parameters.AddWithValue("@doc_id", request.doc_id);
					}
					if (!string.IsNullOrEmpty(request.file_id))
					{
						where = @" where ""public"".device_transaction_log.file_id = @file_id";
						cmd.Parameters.AddWithValue("@file_id", request.file_id);
					}
					if (!string.IsNullOrEmpty(request.function_name))
					{
						where = @" where ""public"".device_transaction_log.function_name = @function_name";
						cmd.Parameters.AddWithValue("@function_name", request.function_name);
					}
					if (!string.IsNullOrEmpty(request.operation_name))
					{
						where = @" where ""public"".device_transaction_log.operation_name = @operation_name";
						cmd.Parameters.AddWithValue("@operation_name", request.operation_name);
					}
					if (!string.IsNullOrEmpty(request.operation_status))
					{
						where = @" where ""public"".device_transaction_log.operation_status = @operation_status";
						cmd.Parameters.AddWithValue("@operation_status", request.operation_status);
					}
					if (!string.IsNullOrEmpty(request.project_id))
					{
						where = @" where ""public"".device_transaction_log.project_id = @project_id";
						cmd.Parameters.AddWithValue("@project_id", request.project_id);
					}
					if (!string.IsNullOrEmpty(request.submission_id))
					{
						where = @" where ""public"".device_transaction_log.submission_id = @submission_id";
						cmd.Parameters.AddWithValue("@submission_id", request.submission_id);
					}
					if (!string.IsNullOrEmpty(request.device_id))
					{
						where = @" where ""public"".device_transaction_log.device_id = @device_id";
						cmd.Parameters.AddWithValue("@device_id", request.device_id);
					}
					cmd.CommandText = query + where;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
								{ "customer_id", Convert.ToString(reader["customer_id"]) },
                //{ "device_name", Convert.ToString(reader["device_name"]) }, //TODO: office_name is not present in DB
                { "document_id", Convert.ToString(reader["document_id"]) },
								{ "document_name", Convert.ToString(reader["doc_name"]) },
								{ "document_number", Convert.ToString(reader["doc_number"]) },
								{ "document_revision", Convert.ToString(reader["doc_revision"]) },
								{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
								{ "file_id", Convert.ToString(reader["file_id"]) },
                //{ "file_type", Convert.ToString(reader["file_type"]) }, //TODO: office_name is not present in DB
                { "function_name", Convert.ToString(reader["function_name"]) },
								{ "log_id", Convert.ToString(reader["log_id"]) },
								{ "operation_data", Convert.ToString(reader["operation_data"]) },
								{ "operation_datetime", Convert.ToString(reader["operation_datetime"]) },
								{ "operation_name", Convert.ToString(reader["operation_name"]) },
								{ "operation_status", Convert.ToString(reader["operation_status"]) },
								{ "operation_status_desc", reader["operation_status_desc"] },
								{ "project_id", Convert.ToString(reader["project_id"]) },
								{ "project_name", Convert.ToString(reader["project_name"]) },
								{ "routine_name", Convert.ToString(reader["routine_name"]) },
								{ "routine_version", Convert.ToString(reader["routine_version"]) },
								{ "submission_id", Convert.ToString(reader["submission_id"]) },
								{ "submission_name", Convert.ToString(reader["submission_name"]) },
								//{ "user_email", Convert.ToString(reader["user_email"]) },//TODO: office_name is not present in DB
								//{ "user_displayname", Convert.ToString(reader["user_displayname"]) },//TODO: office_name is not present in DB
							});
						}
					}
				}
				return Ok(result);
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
