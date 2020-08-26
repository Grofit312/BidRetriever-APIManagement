using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Models.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("User Notification Management")]
	public class UserNotificationController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public UserNotificationController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpPost]
		[Route("CreateNotificationTemplate")]
		public IActionResult Post(NotificationTemplate notificationTemplate)
		{
			try
			{
				// check missing parameter
				var missingParameter = notificationTemplate.CheckRequiredParameters(new string[]
				{
					"notification_type",
					"template_from_address",
					"template_html",
					"template_name",
					"template_subject_line"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var notificationTemplateId = notificationTemplate.notification_template_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check id already exists
					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM notification_templates WHERE notification_template_id='{notificationTemplateId}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new { notification_template_id = notificationTemplateId, status = "duplicated" });
					}

					// create record
					var columns = "(notification_template_id, notification_type, status, template_desc, template_from_address, "
																	+ "template_from_name, template_html, template_name, template_subject_line, "
																	+ "create_datetime, edit_datetime)";
					var values = "(@notification_template_id, @notification_type, @status, @template_desc, @template_from_address, "
																	+ "@template_from_name, @template_html, @template_name, @template_subject_line, "
																	+ "@create_datetime, @edit_datetime)";

					cmd.CommandText = $"INSERT INTO notification_templates {columns} VALUES{values}";

					cmd.Parameters.AddWithValue("notification_template_id", notificationTemplateId);
					cmd.Parameters.AddWithValue("notification_type", notificationTemplate.notification_type);
					cmd.Parameters.AddWithValue("status", notificationTemplate.status ?? "active");
					cmd.Parameters.AddWithValue("template_desc", notificationTemplate.template_desc ?? "");
					cmd.Parameters.AddWithValue("template_from_address", notificationTemplate.template_from_address);
					cmd.Parameters.AddWithValue("template_from_name", notificationTemplate.template_from_name ?? "");
					cmd.Parameters.AddWithValue("template_html", notificationTemplate.template_html);
					cmd.Parameters.AddWithValue("template_name", notificationTemplate.template_name);
					cmd.Parameters.AddWithValue("template_subject_line", notificationTemplate.template_subject_line);
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						notification_template_id = notificationTemplateId,
						status = "completed",
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
		[Route("FindNotificationTemplate")]
		public IActionResult Get(NotificationFindRequest request)
		{
			try
			{
				// validation check
				if (request.template_type == null)
				{
					return BadRequest(new { status = "template_type is required" });
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT notification_template_id, template_name, template_desc, template_html, status, "
						+ "create_datetime, edit_datetime, template_subject_line, template_from_address, template_from_name, notification_type "
						+ "FROM notification_templates WHERE "
						+ "notification_type='" + request.template_type + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								{ "notification_template_id", _dbHelper.SafeGetString(reader, 0) },
								{ "template_name", _dbHelper.SafeGetString(reader, 1) },
								{ "template_desc", _dbHelper.SafeGetString(reader, 2) },
								{ "template_html", _dbHelper.SafeGetString(reader, 3) },
								{ "status", _dbHelper.SafeGetString(reader, 4) },
								{ "create_datetime", reader.GetDateTime(5).ToString() },
								{ "edit_datetime", reader.GetDateTime(6).ToString() },
								{ "template_subject_line", _dbHelper.SafeGetString(reader, 7) },
								{ "template_from_address", _dbHelper.SafeGetString(reader, 8) },
								{ "template_from_name", _dbHelper.SafeGetString(reader, 9) },
								{ "notification_type", _dbHelper.SafeGetString(reader, 10) },
							};

							return Ok(result);
						}
						else
						{
							return BadRequest(new
							{
								status = "cannot find specified template"
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


		[HttpGet]
		[Route("GetNotificationTemplate")]
		public IActionResult Get([FromQuery(Name = "template_id")] string template_id)
		{
			try
			{
				// validation check
				if (template_id == null || template_id == "")
				{
					return BadRequest(new
					{
						status = "template_id is required"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT template_name, template_desc, template_html, status, "
						+ "create_datetime, edit_datetime, template_subject_line, template_from_address, template_from_name, notification_type "
						+ "FROM notification_templates WHERE "
						+ "notification_template_id='" + template_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								{ "notification_template_id", template_id },
								{ "template_name", _dbHelper.SafeGetString(reader, 0) },
								{ "template_desc", _dbHelper.SafeGetString(reader, 1) },
								{ "template_html", _dbHelper.SafeGetString(reader, 2) },
								{ "status", _dbHelper.SafeGetString(reader, 3) },
								{ "create_datetime", reader.GetDateTime(4).ToString() },
								{ "edit_datetime", reader.GetDateTime(5).ToString() },
								{ "template_subject_line", _dbHelper.SafeGetString(reader, 6) },
								{ "template_from_address", _dbHelper.SafeGetString(reader, 7) },
								{ "template_from_name", _dbHelper.SafeGetString(reader, 8) },
								{ "notification_type", _dbHelper.SafeGetString(reader, 9) },
							};

							return Ok(result);
						}
						else
						{
							return BadRequest(new
							{
								status = "cannot find specified template"
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
		[Route("UpdateNotificationTemplate")]
		public IActionResult Post(NotificationUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_notification_template_id == null)
				{
					return BadRequest(new
					{
						status = "search_notification_template_id is required"
					});
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE notification_template_id='" + request.search_notification_template_id + "'";
					var queryString = "UPDATE notification_templates SET "
						+ "notification_type = COALESCE(@notification_type, notification_type), "
						+ "status = COALESCE(@status, status), "
						+ "template_desc = COALESCE(@template_desc, template_desc), "
						+ "template_from_address = COALESCE(@template_from_address, template_from_address), "
						+ "template_from_name = COALESCE(@template_from_name, template_from_name), "
						+ "template_html = COALESCE(@template_html, template_html), "
						+ "template_name = COALESCE(@template_name, template_name), "
						+ "template_subject_line = COALESCE(@template_subject_line, template_subject_line), "
						+ "edit_datetime = @edit_datetime";

					if (true == true) // check if api_key has admin access
					{
						queryString += ", notification_template_id = COALESCE(@notification_template_id, notification_template_id)";
					}

					queryString = queryString + whereString;

					cmd.CommandText = queryString;
					cmd.Parameters.AddWithValue("notification_type", (object)request.notification_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("template_desc", (object)request.template_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("template_from_address", (object)request.template_from_address ?? DBNull.Value);
					cmd.Parameters.AddWithValue("template_from_name", (object)request.template_from_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("template_html", (object)request.template_html ?? DBNull.Value);
					cmd.Parameters.AddWithValue("template_name", (object)request.template_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("template_subject_line", (object)request.template_subject_line ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("notification_template_id", (object)request.notification_template_id ?? DBNull.Value);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching notification template found"
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
		[Route("CreateUserNotification")]
		public IActionResult Post(UserNotification userNotification)
		{
			try
			{
				// validation check
				var missingParameter = userNotification.CheckRequiredParameters(new string[] { "user_email" });

				if (missingParameter == null)
				{
					var userNotificationId = userNotification.user_notification_id ?? Guid.NewGuid().ToString();
					var timestamp = DateTime.UtcNow;

					using (var cmd = _dbHelper.SpawnCommand())
					{
						cmd.CommandText = $"SELECT EXISTS (SELECT true FROM user_notifications WHERE user_notification_id='{userNotificationId}')";

						if ((bool)cmd.ExecuteScalar() == false)
						{
							cmd.CommandText = "INSERT INTO user_notifications (create_user_id, user_notification_id, user_id, user_email, "
																			+ "project_id, submission_id, customer_id, customer_name, "
																			+ "notification_actual_from_address, notification_actual_from_name, notification_actual_html, notification_actual_subject, "
																			+ "notification_name, notification_send_datetime, notification_template_id, status, create_datetime, "
																			+ "edit_datetime) "
																			+ "VALUES(@create_user_id, @user_notification_id, @user_id, @user_email, "
																			+ "@project_id, @submission_id, @customer_id, @customer_name, "
																			+ "@notification_actual_from_address, @notification_actual_from_name, @notification_actual_html, @notification_actual_subject, "
																			+ "@notification_name, @notification_send_datetime, @notification_template_id, "
																			+ "@status, @create_datetime, @edit_datetime)";

							cmd.Parameters.AddWithValue("create_user_id", userNotification.create_user_id ?? "");
							cmd.Parameters.AddWithValue("user_notification_id", userNotificationId);
							cmd.Parameters.AddWithValue("user_id", userNotification.user_id ?? "");
							cmd.Parameters.AddWithValue("user_email", userNotification.user_email);
							cmd.Parameters.AddWithValue("project_id", userNotification.project_id ?? "");
							cmd.Parameters.AddWithValue("submission_id", userNotification.submission_id ?? "");
							cmd.Parameters.AddWithValue("customer_id", userNotification.customer_id ?? "");
							cmd.Parameters.AddWithValue("customer_name", userNotification.customer_name ?? "");
							cmd.Parameters.AddWithValue("notification_name", userNotification.notification_name ?? "");
							cmd.Parameters.AddWithValue("notification_send_datetime", DateTimeHelper.ConvertToUTCDateTime(userNotification.notification_send_datetime));
							cmd.Parameters.AddWithValue("notification_template_id", userNotification.notification_template_id ?? "");
							cmd.Parameters.AddWithValue("notification_actual_from_address", userNotification.notification_actual_from_address ?? "");
							cmd.Parameters.AddWithValue("notification_actual_from_name", userNotification.notification_actual_from_name ?? "");
							cmd.Parameters.AddWithValue("notification_actual_html", userNotification.notification_actual_html ?? "");
							cmd.Parameters.AddWithValue("notification_actual_subject", userNotification.notification_actual_subject ?? "");
							cmd.Parameters.AddWithValue("status", userNotification.status ?? "active");
							cmd.Parameters.AddWithValue("create_datetime", timestamp);
							cmd.Parameters.AddWithValue("edit_datetime", timestamp);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								user_notification_id = userNotificationId,
								status = "completed"
							});
						}
						else
						{
							return Ok(new
							{
								user_notification_id = userNotificationId,
								status = "duplicated"
							});
						}
					}
				}
				else
				{
					return BadRequest(new
					{
						status = missingParameter + " is required"
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


		[HttpPost]
		[Route("UpdateUserNotification")]
		public IActionResult Post(UserNotificationUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_user_notification_id == null)
				{
					return BadRequest(new { status = "search_user_notification_id is required" });
				}

				// update
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE user_notification_id='" + request.search_user_notification_id + "'";
					var queryString = "UPDATE user_notifications SET "
																					+ "create_user_id = COALESCE(@create_user_id, create_user_id), "
																					+ "customer_id = COALESCE(@customer_id, customer_id), "
																					+ "customer_name = COALESCE(@customer_name, customer_name), "
																					+ "notification_actual_from_address = COALESCE(@notification_actual_from_address, notification_actual_from_address), "
																					+ "notification_actual_from_name = COALESCE(@notification_actual_from_name, notification_actual_from_name), "
																					+ "notification_actual_html = COALESCE(@notification_actual_html, notification_actual_html), "
																					+ "notification_actual_subject = COALESCE(@notification_actual_subject, notification_actual_subject), "
																					+ "notification_name = COALESCE(@notification_name, notification_name), "
																					+ "notification_send_datetime = COALESCE(@notification_send_datetime, notification_send_datetime), "
																					+ "notification_template_id = COALESCE(@notification_template_id, notification_template_id), "
																					+ "project_id = COALESCE(@project_id, project_id), "
																					+ "status = COALESCE(@status, status), "
																					+ "submission_id = COALESCE(@submission_id, submission_id), "
																					+ "user_email = COALESCE(@user_email, user_email), "
																					+ "user_id = COALESCE(@user_id, user_id), "
																					+ "edit_datetime = @edit_datetime";

					queryString = queryString + whereString;

					cmd.CommandText = queryString;
					cmd.Parameters.AddWithValue("create_user_id", (object)request.create_user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_id", (object)request.customer_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_name", (object)request.customer_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("notification_actual_from_address", (object)request.notification_actual_from_address ?? DBNull.Value);
					cmd.Parameters.AddWithValue("notification_actual_from_name", (object)request.notification_actual_from_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("notification_actual_html", (object)request.notification_actual_html ?? DBNull.Value);
					cmd.Parameters.AddWithValue("notification_actual_subject", (object)request.notification_actual_subject ?? DBNull.Value);
					cmd.Parameters.AddWithValue("notification_name", (object)request.notification_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("notification_send_datetime", request.notification_send_datetime != null ? (object)DateTimeHelper.ConvertToUTCDateTime(request.notification_send_datetime) : DBNull.Value);
					cmd.Parameters.AddWithValue("notification_template_id", (object)request.notification_template_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("project_id", (object)request.project_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("submission_id", (object)request.submission_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_email", (object)request.user_email ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_id", (object)request.user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching user notification found"
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


		[HttpGet]
		[Route("FindUserNotifications")]
		public IActionResult Get(UserNotificationFindRequest request)
		{
			try
			{
				// Validation check
				if (request.customer_id == null && request.project_id == null && request.notification_send_datetime == null
								&& request.submission_id == null && request.user_id == null)
				{
					return BadRequest(new { status = "Please provide at least one query parameter" });
				}

				// Find notifications
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE ";

					if (request.customer_id != null)
					{
						whereString += $"user_notifications.customer_id='{request.customer_id}' AND ";
					}
					if (request.project_id != null)
					{
						whereString += $"user_notifications.project_id='{request.project_id}' AND ";
					}
					if (request.notification_send_datetime != null)
					{
						whereString += $"user_notifications.notification_send_datetime='{request.notification_send_datetime}' AND ";
					}
					if (request.submission_id != null)
					{
						whereString += $"user_notifications.submission_id='{request.submission_id}' AND ";
					}
					if (request.user_id != null)
					{
						whereString += $"user_notifications.user_id='{request.user_id}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT user_notifications.create_user_id, user_notifications.customer_id, user_notifications.customer_name, "
																					+ "user_notifications.notification_actual_from_address, user_notifications.notification_actual_from_name, "
																					+ "user_notifications.notification_actual_html, user_notifications.notification_actual_subject, "
																					+ "user_notifications.notification_name, user_notifications.notification_send_datetime, user_notifications.notification_template_id, "
																					+ "user_notifications.project_id, user_notifications.status, user_notifications.submission_id, "
																					+ "user_notifications.user_email, user_notifications.user_id, user_notifications.user_notification_id, "
																					+ "user_notifications.create_datetime, user_notifications.edit_datetime, "
																					+ "notification_templates.notification_type, notification_templates.template_desc, notification_templates.template_from_address, "
																					+ "notification_templates.template_from_name, notification_templates.template_html, notification_templates.template_name, notification_templates.template_subject_line, "
																					+ "project_submissions.submission_name "
																					+ "FROM user_notifications LEFT JOIN notification_templates ON user_notifications.notification_template_id=notification_templates.notification_template_id "
																					+ "LEFT JOIN project_submissions ON project_submissions.project_submission_id=user_notifications.submission_id"
																					+ whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>
							{
								{ "create_user_id", _dbHelper.SafeGetString(reader, 0) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 1) },
								{ "customer_name", _dbHelper.SafeGetString(reader, 2) },
								{ "notification_actual_from_address", _dbHelper.SafeGetString(reader, 3) },
								{ "notification_actual_from_name", _dbHelper.SafeGetString(reader, 4) },
								{ "notification_actual_html", _dbHelper.SafeGetString(reader, 5) },
								{ "notification_actual_subject", _dbHelper.SafeGetString(reader, 6) },
								{ "notification_name", _dbHelper.SafeGetString(reader, 7) },
								{ "notification_send_datetime", _dbHelper.SafeGetDatetimeString(reader, 8) },
								{ "notification_template_id", _dbHelper.SafeGetString(reader, 9) },
								{ "project_id", _dbHelper.SafeGetString(reader, 10) },
								{ "status", _dbHelper.SafeGetString(reader, 11) },
								{ "submission_id", _dbHelper.SafeGetString(reader, 12) },
								{ "user_email", _dbHelper.SafeGetString(reader, 13) },
								{ "user_id", _dbHelper.SafeGetString(reader, 14) },
								{ "user_notification_id", _dbHelper.SafeGetString(reader, 15) },
								{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 16) },
								{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 17) },
								{ "notification_type", _dbHelper.SafeGetString(reader, 18) },
								{ "template_desc", _dbHelper.SafeGetString(reader, 19) },
								{ "template_from_address", _dbHelper.SafeGetString(reader, 20) },
								{ "template_from_name", _dbHelper.SafeGetString(reader, 21) },
								{ "template_html", _dbHelper.SafeGetString(reader, 22) },
								{ "template_name", _dbHelper.SafeGetString(reader, 23) },
								{ "template_subject_line", _dbHelper.SafeGetString(reader, 24) },
								{ "submission_name", _dbHelper.SafeGetString(reader, 25) },
							};

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
	}
}
