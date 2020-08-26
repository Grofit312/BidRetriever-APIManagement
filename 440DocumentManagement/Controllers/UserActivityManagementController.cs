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
	[OpenApiTag("User Activity Management")]
	public class UserActivityManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public UserActivityManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}


		[HttpPost]
		[Route("LogUserActivity")]
		public IActionResult Post(UserActivity request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"activity_name", "application_name"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var userActivityId = request.user_activity_id ?? Guid.NewGuid().ToString();
				var activityDateTime = string.IsNullOrEmpty(request.activity_datetime) ? DateTime.UtcNow : DateTimeHelper.ConvertToUTCDateTime(request.activity_datetime);

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "INSERT INTO user_activity_log "
							+ "(activity_data, activity_datetime, activity_level, activity_name, application_name, customer_id, document_id, file_id, "
							+ "notification_id, project_id, user_activity_id, user_id) "
							+ "VALUES(@activity_data, @activity_datetime, @activity_level, @activity_name, @application_name, @customer_id, @document_id, @file_id, "
							+ "@notification_id, @project_id, @user_activity_id, @user_id)";

					cmd.Parameters.AddWithValue("activity_data", request.activity_data ?? "");
					cmd.Parameters.AddWithValue("activity_datetime", activityDateTime);
					cmd.Parameters.AddWithValue("activity_level", request.activity_level ?? "");
					cmd.Parameters.AddWithValue("activity_name", request.activity_name);
					cmd.Parameters.AddWithValue("application_name", request.application_name);
					cmd.Parameters.AddWithValue("customer_id", request.customer_id ?? "");
					cmd.Parameters.AddWithValue("document_id", request.document_id ?? "");
					cmd.Parameters.AddWithValue("file_id", request.file_id ?? "");
					cmd.Parameters.AddWithValue("notification_id", request.notification_id ?? "");
					cmd.Parameters.AddWithValue("project_id", request.project_id ?? "");
					cmd.Parameters.AddWithValue("user_activity_id", userActivityId);
					cmd.Parameters.AddWithValue("user_id", request.user_id ?? "");

					cmd.ExecuteNonQuery();
				}

				return Ok(new
				{
					status = "completed",
					user_activity_id = userActivityId
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
	}
}
