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
	public class CalendarEventManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public CalendarEventManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateCalendarEvent")]
		public IActionResult Post(CalendarEvent request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"calendar_event_name",
					"calendar_event_start_datetime",
					"calendar_event_type",
					"calendar_event_organizer_user_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create event
				var calendarEventId = request.calendar_event_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check existence
					cmd.CommandText = "SELECT EXISTS (SELECT true FROM calendar_events WHERE calendar_event_id='" + calendarEventId + "')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new { status = "duplicated", calendar_event_id = calendarEventId });
					}

					cmd.CommandText = "INSERT INTO calendar_events "
							+ "(calendar_event_color_id, calendar_event_company_id, calendar_event_company_office_id, calendar_event_organizer_company_id, calendar_event_organizer_company_office_id, "
							+ "calendar_event_desc, calendar_event_end_datetime, calendar_event_id, calendar_event_name, "
							+ "calendar_event_organizer_user_id, calendar_event_start_datetime, calendar_event_source_company_id, "
							+ "calendar_event_source_company_office_id, calendar_event_source_user_id, calendar_event_status, "
							+ "calendar_event_type, create_datetime, edit_datetime, google_id, icaluid, outlook_id, project_id, status, calendar_event_location) "
							+ "VALUES(@calendar_event_color_id, @calendar_event_company_id, @calendar_event_company_office_id, @calendar_event_organizer_company_id, @calendar_event_organizer_company_office_id, "
							+ "@calendar_event_desc, @calendar_event_end_datetime, @calendar_event_id, @calendar_event_name, "
							+ "@calendar_event_organizer_user_id, @calendar_event_start_datetime, @calendar_event_source_company_id, "
							+ "@calendar_event_source_company_office_id, @calendar_event_source_user_id, @calendar_event_status, "
							+ "@calendar_event_type, @create_datetime, @edit_datetime, @google_id, @icaluid, @outlook_id, @project_id, @status, @calendar_event_location)";

					cmd.Parameters.AddWithValue("calendar_event_color_id", request.calendar_event_color_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_company_id", request.calendar_event_company_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_company_office_id", request.calendar_event_company_office_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_start_datetime", DateTimeHelper.ConvertToUTCDateTime(request.calendar_event_start_datetime));
					cmd.Parameters.AddWithValue("calendar_event_end_datetime", request.calendar_event_end_datetime != null ? (object)DateTimeHelper.ConvertToUTCDateTime(request.calendar_event_end_datetime) : DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_id", calendarEventId);
					cmd.Parameters.AddWithValue("calendar_event_name", request.calendar_event_name);
					cmd.Parameters.AddWithValue("calendar_event_organizer_user_id", request.calendar_event_organizer_user_id);
					cmd.Parameters.AddWithValue("calendar_event_organizer_company_id", request.calendar_event_organizer_company_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_organizer_company_office_id", request.calendar_event_organizer_company_office_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_source_company_id", request.calendar_event_source_company_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_source_company_office_id", request.calendar_event_source_company_office_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_source_user_id", request.calendar_event_source_user_id ?? "");
					cmd.Parameters.AddWithValue("calendar_event_status", request.calendar_event_status ?? "");
					cmd.Parameters.AddWithValue("calendar_event_type", request.calendar_event_type);
					cmd.Parameters.AddWithValue("calendar_event_desc", request.calendar_event_desc ?? "");
					cmd.Parameters.AddWithValue("calendar_event_location", request.calendar_event_location ?? "");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("google_id", request.google_id ?? "");
					cmd.Parameters.AddWithValue("icaluid", request.icaluid ?? "");
					cmd.Parameters.AddWithValue("outlook_id", request.outlook_id ?? "");
					cmd.Parameters.AddWithValue("project_id", request.project_id ?? "");
					cmd.Parameters.AddWithValue("status", request.status ?? "active");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = "completed",
						calendar_event_id = calendarEventId
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
		[Route("UpdateCalendarEvent")]
		public IActionResult Post(CalendarEventUpdateRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.search_calendar_event_id))
				{
					return BadRequest(new
					{
						status = "Please provide search_calendar_event_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE calendar_events SET "
							+ "calendar_event_company_office_id = COALESCE(@calendar_event_company_office_id, calendar_event_company_office_id), "
							+ "calendar_event_color_id = COALESCE(@calendar_event_color_id, calendar_event_color_id), "
							+ "calendar_event_desc = COALESCE(@calendar_event_desc, calendar_event_desc), "
							+ "calendar_event_end_datetime = COALESCE(@calendar_event_end_datetime, calendar_event_end_datetime), "
							+ "calendar_event_name = COALESCE(@calendar_event_name, calendar_event_name), "
							+ "calendar_event_organizer_user_id = COALESCE(@calendar_event_organizer_user_id, calendar_event_organizer_user_id), "
							+ "calendar_event_organizer_company_id = COALESCE(@calendar_event_organizer_company_id, calendar_event_organizer_company_id), "
							+ "calendar_event_organizer_company_office_id = COALESCE(@calendar_event_organizer_company_office_id, calendar_event_organizer_company_office_id), "
							+ "calendar_event_source_user_id = COALESCE(@calendar_event_source_user_id, calendar_event_source_user_id), "
							+ "calendar_event_source_company_id = COALESCE(@calendar_event_source_company_id, calendar_event_source_company_id), "
							+ "calendar_event_source_company_office_id = COALESCE(@calendar_event_source_company_office_id, calendar_event_source_company_office_id), "
							+ "calendar_event_start_datetime = COALESCE(@calendar_event_start_datetime, calendar_event_start_datetime), "
							+ "calendar_event_status = COALESCE(@calendar_event_status, calendar_event_status), "
							+ "calendar_event_type = COALESCE(@calendar_event_type, calendar_event_type), "
							+ "calendar_event_location = COALESCE(@calendar_event_location, calendar_event_location), "
							+ "google_id = COALESCE(@google_id, google_id), "
							+ "icaluid = COALESCE(@icaluid, icaluid), "
							+ "outlook_id = COALESCE(@outlook_id, outlook_id), "
							+ "project_id = COALESCE(@project_id, project_id), "
							+ "status = COALESCE(@status, status), "
							+ "edit_datetime = @edit_datetime "
							+ "WHERE calendar_event_id='" + request.search_calendar_event_id + "'";

					cmd.Parameters.AddWithValue("calendar_event_company_office_id", (object)request.calendar_event_company_office_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_color_id", (object)request.calendar_event_color_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_desc", (object)request.calendar_event_desc ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_end_datetime", request.calendar_event_end_datetime != null ? (object)DateTimeHelper.ConvertToUTCDateTime(request.calendar_event_end_datetime) : DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_name", (object)request.calendar_event_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_organizer_user_id", (object)request.calendar_event_organizer_user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_organizer_company_id", (object)request.calendar_event_organizer_company_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_organizer_company_office_id", (object)request.calendar_event_organizer_company_office_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_source_user_id", (object)request.calendar_event_source_user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_source_company_id", (object)request.calendar_event_source_company_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_source_company_office_id", (object)request.calendar_event_source_company_office_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_start_datetime", request.calendar_event_start_datetime != null ? (object)DateTimeHelper.ConvertToUTCDateTime(request.calendar_event_start_datetime) : DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_status", (object)request.calendar_event_status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_type", (object)request.calendar_event_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_location", (object)request.calendar_event_location ?? DBNull.Value);
					cmd.Parameters.AddWithValue("google_id", (object)request.google_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("icaluid", (object)request.icaluid ?? DBNull.Value);
					cmd.Parameters.AddWithValue("outlook_id", (object)request.outlook_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("project_id", (object)request.project_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					var affectedRows = cmd.ExecuteNonQuery();

					if (affectedRows == 0)
					{
						return BadRequest(new
						{
							status = "calendar event not found"
						});
					}
				}

				if (!string.IsNullOrEmpty(request.calendar_event_start_datetime))
				{
					var eventType = __getEventType(request.search_calendar_event_id);
					var projectId = __getEventProjectId(request.search_calendar_event_id);

					if (eventType == "project_bid_datetime")
					{
						using (var cmd = _dbHelper.SpawnCommand())
						{
							cmd.CommandText = $"UPDATE projects SET project_bid_datetime='{request.calendar_event_start_datetime}' WHERE project_id='{projectId}'";
							cmd.ExecuteNonQuery();
						}
					}
				}

				return Ok(new
				{
					status = "updated"
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
		[Route("FindCalendarEvents")]
		public IActionResult Get(CalendarEventFindRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = " WHERE 1=1 AND ";

					if (!string.IsNullOrEmpty(request.calendar_event_id))
					{
						where += "calendar_event_id='" + request.calendar_event_id + "' AND ";
					}
					if (!string.IsNullOrEmpty(request.company_id))
					{
						where += $"(calendar_event_company_id='{request.company_id}' OR calendar_event_source_company_id='{request.company_id}' OR calendar_event_organizer_company_id='{request.company_id}') AND ";
					}
					if (!string.IsNullOrEmpty(request.company_office_id))
					{
						where += $"(calendar_event_company_office_id='{request.company_office_id}' OR calendar_event_source_company_office_id='{request.company_office_id}' OR calendar_event_organizer_company_office_id='{request.company_office_id}') AND ";
					}
					if (!string.IsNullOrEmpty(request.user_id))
					{
						//where += $"(calendar_event_organizer_user_id='{request.user_id}' OR calendar_event_source_user_id='{request.user_id}') AND ";
						where += $"calendar_event_organizer_user_id='{request.user_id}' AND ";
					}
					if (!string.IsNullOrEmpty(request.project_id))
					{
						where += "project_id='" + request.project_id + "' AND ";
					}
					if (!string.IsNullOrEmpty(request.type))
					{
						where += "calendar_event_type='" + request.type + "' AND ";
					}
					if (!string.IsNullOrEmpty(request.status))
					{
						where += "status='" + request.status + "' AND ";
					}
					if (!string.IsNullOrEmpty(request.start_datetime))
					{
						if (!string.IsNullOrEmpty(request.end_datetime))
						{
							where += $"(Cast(calendar_event_start_datetime as date) >= Cast('{request.start_datetime}' as date) AND Cast(calendar_event_end_datetime as date) <= Cast('{request.end_datetime}' as date)) AND ";
						}
						else
						{
							where += $"Cast(calendar_event_start_datetime as date) >= Cast('{request.start_datetime}' as date) AND ";
						}
					}

					where = where.Remove(where.Length - 5);

					cmd.CommandText = "SELECT calendar_event_color_id, calendar_event_company_id, calendar_event_company_office_id, "
							+ "calendar_event_desc, calendar_event_end_datetime, calendar_event_id, calendar_event_name, "
							+ "calendar_event_organizer_user_id, calendar_event_organizer_company_id, calendar_event_organizer_company_office_id, "
							+ "calendar_event_source_user_id, calendar_event_source_company_id, calendar_event_source_company_office_id, "
							+ "calendar_event_start_datetime, calendar_event_status, calendar_event_type, create_datetime, "
							+ "edit_datetime, google_id, icaluid, outlook_id, project_id, status, calendar_event_location "
							+ "FROM calendar_events" + where;

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, string>
							{
								{ "calendar_event_color_id", _dbHelper.SafeGetString(reader, 0) },
								{ "calendar_event_company_id", _dbHelper.SafeGetString(reader, 1) },
								{ "calendar_event_company_office_id", _dbHelper.SafeGetString(reader, 2) },
								{ "calendar_event_desc", _dbHelper.SafeGetString(reader, 3) },
								{ "calendar_event_end_datetime", _dbHelper.SafeGetDatetimeString(reader, 4) },
								{ "calendar_event_id", _dbHelper.SafeGetString(reader, 5) },
								{ "calendar_event_name", _dbHelper.SafeGetString(reader, 6) },
								{ "calendar_event_organizer_user_id", _dbHelper.SafeGetString(reader, 7) },
								{ "calendar_event_organizer_company_id", _dbHelper.SafeGetString(reader, 8) },
								{ "calendar_event_organizer_company_office_id", _dbHelper.SafeGetString(reader, 9) },
								{ "calendar_event_source_user_id", _dbHelper.SafeGetString(reader, 10) },
								{ "calendar_event_source_company_id", _dbHelper.SafeGetString(reader, 11) },
								{ "calendar_event_source_company_office_id", _dbHelper.SafeGetString(reader, 12) },
								{ "calendar_event_start_datetime", _dbHelper.SafeGetDatetimeString(reader, 13) },
								{ "calendar_event_status", _dbHelper.SafeGetString(reader, 14) },
								{ "calendar_event_type", _dbHelper.SafeGetString(reader, 15) },
								{ "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 16) },
								{ "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 17) },
								{ "google_id", _dbHelper.SafeGetString(reader, 18) },
								{ "icaluid", _dbHelper.SafeGetString(reader, 19) },
								{ "outlook_id", _dbHelper.SafeGetString(reader, 20) },
								{ "project_id", _dbHelper.SafeGetString(reader, 21) },
								{ "status", _dbHelper.SafeGetString(reader, 22) },
								{ "calendar_event_location", _dbHelper.SafeGetString(reader, 23) },
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
		[Route("GetCalendarEvent")]
		public IActionResult Get(CalendarEventGetRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.calendar_event_id))
				{
					return BadRequest(new
					{
						status = "Please provide calendar_event_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT calendar_event_color_id, calendar_event_company_id, calendar_event_company_office_id, "
							+ "calendar_event_desc, calendar_event_end_datetime, calendar_event_id, calendar_event_name, "
							+ "calendar_event_organizer_user_id, calendar_event_organizer_company_id, calendar_event_organizer_company_office_id, "
							+ "calendar_event_source_user_id, calendar_event_source_company_id, calendar_event_source_company_office_id, "
							+ "calendar_event_start_datetime, calendar_event_status, calendar_event_type, create_datetime, "
							+ "edit_datetime, google_id, icaluid, outlook_id, project_id, status, calendar_event_location "
							+ "FROM calendar_events WHERE calendar_event_id='" + request.calendar_event_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new Dictionary<string, string>
							{
								{ "calendar_event_color_id", _dbHelper.SafeGetString(reader, 0) },
								{ "calendar_event_company_id", _dbHelper.SafeGetString(reader, 1) },
								{ "calendar_event_company_office_id", _dbHelper.SafeGetString(reader, 2) },
								{ "calendar_event_desc", _dbHelper.SafeGetString(reader, 3) },
								{ "calendar_event_end_datetime", _dbHelper.SafeGetDatetimeString(reader, 4) },
								{ "calendar_event_id", _dbHelper.SafeGetString(reader, 5) },
                { "calendar_event_name", _dbHelper.SafeGetString(reader, 6) },
                { "calendar_event_organizer_user_id", _dbHelper.SafeGetString(reader, 7) },
                { "calendar_event_organizer_company_id", _dbHelper.SafeGetString(reader, 8) },
                { "calendar_event_organizer_company_office_id", _dbHelper.SafeGetString(reader, 9) },
                { "calendar_event_source_user_id", _dbHelper.SafeGetString(reader, 10) },
                { "calendar_event_source_company_id", _dbHelper.SafeGetString(reader, 11) },
                { "calendar_event_source_company_office_id", _dbHelper.SafeGetString(reader, 12) },
                { "calendar_event_start_datetime", _dbHelper.SafeGetDatetimeString(reader, 13) },
                { "calendar_event_status", _dbHelper.SafeGetString(reader, 14) },
                { "calendar_event_type", _dbHelper.SafeGetString(reader, 15) },
                { "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 16) },
                { "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 17) },
                { "google_id", _dbHelper.SafeGetString(reader, 18) },
                { "icaluid", _dbHelper.SafeGetString(reader, 19) },
                { "outlook_id", _dbHelper.SafeGetString(reader, 20) },
                { "project_id", _dbHelper.SafeGetString(reader, 21) },
                { "status", _dbHelper.SafeGetString(reader, 22) },
                { "calendar_event_location", _dbHelper.SafeGetString(reader, 23) },
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "calendar event not found!"
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
		[Route("CreateEventAttendee")]
		public IActionResult Post(EventAttendee request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"event_attendee_user_id", "calendar_event_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var eventAttendeeId = request.event_attendee_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check existence
					cmd.CommandText = "SELECT EXISTS (SELECT true FROM event_attendee WHERE event_attendee_id='" + eventAttendeeId + "')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new
						{
							status = "duplicated",
							event_attendee_id = eventAttendeeId
						});
					}

					// create attendee
					cmd.CommandText = "INSERT INTO event_attendee "
							+ "(event_attendee_id, calendar_event_id, event_attendee_user_id, create_datetime, edit_datetime, "
							+ "event_attendee_status, event_attendee_comment, event_attendee_optional, status) "
							+ "VALUES(@event_attendee_id, @calendar_event_id, @event_attendee_user_id, @create_datetime, @edit_datetime, "
							+ "@event_attendee_status, @event_attendee_comment, @event_attendee_optional, @status)";

					cmd.Parameters.AddWithValue("event_attendee_id", eventAttendeeId);
					cmd.Parameters.AddWithValue("calendar_event_id", request.calendar_event_id);
					cmd.Parameters.AddWithValue("event_attendee_user_id", request.event_attendee_user_id);
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("event_attendee_status", request.event_attendee_status ?? "");
					cmd.Parameters.AddWithValue("event_attendee_comment", request.event_attendee_comment ?? "");
					cmd.Parameters.AddWithValue("event_attendee_optional", request.event_attendee_optional ?? "");
					cmd.Parameters.AddWithValue("status", request.status ?? "active");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = "completed",
						event_attendee_id = eventAttendeeId
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
		[Route("UpdateEventAttendee")]
		public IActionResult Post(EventAttendeeUpdateRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.search_event_attendee_id))
				{
					return BadRequest(new
					{
						status = "Please provide search_event_attendee_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE event_attendee SET "
							+ "event_attendee_comment = COALESCE(@event_attendee_comment, event_attendee_comment), "
							+ "event_attendee_optional = COALESCE(@event_attendee_optional, event_attendee_optional), "
							+ "event_attendee_status = COALESCE(@event_attendee_status, event_attendee_status), "
							+ "calendar_event_id = COALESCE(@calendar_event_id, calendar_event_id), "
							+ "event_attendee_user_id = COALESCE(@event_attendee_user_id, event_attendee_user_id), "
							+ "status = COALESCE(@status, status), "
							+ "edit_datetime = @edit_datetime "
							+ "WHERE event_attendee_id='" + request.search_event_attendee_id + "'";

					cmd.Parameters.AddWithValue("event_attendee_comment", (object)request.event_attendee_comment ?? DBNull.Value);
					cmd.Parameters.AddWithValue("event_attendee_optional", (object)request.event_attendee_optional ?? DBNull.Value);
					cmd.Parameters.AddWithValue("event_attendee_status", (object)request.event_attendee_status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("calendar_event_id", (object)request.calendar_event_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("event_attendee_user_id", (object)request.event_attendee_user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					var affectedRows = cmd.ExecuteNonQuery();

					if (affectedRows == 0)
					{
						return BadRequest(new
						{
							status = "attendee not found"
						});
					}
					else
					{
						return Ok(new
						{
							status = "updated",
							event_attendee_id = request.search_event_attendee_id
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
		[Route("GetEventAttendee")]
		public IActionResult Get(EventAttendeeGetRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.event_attendee_id))
				{
					return BadRequest(new { status = "Please provide event_attendee_id" });
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT event_attendee_comment, event_attendee_id, event_attendee_optional, "
							+ "event_attendee_status, calendar_event_id, event_attendee_user_id, status "
							+ "FROM event_attendee WHERE event_attendee_id='" + request.event_attendee_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new Dictionary<string, string>
              {
                { "event_attendee_comment", _dbHelper.SafeGetString(reader, 0) },
                { "event_attendee_id", _dbHelper.SafeGetString(reader, 1) },
                { "event_attendee_optional", _dbHelper.SafeGetString(reader, 2) },
                { "event_attendee_status", _dbHelper.SafeGetString(reader, 3) },
                { "calendar_event_id", _dbHelper.SafeGetString(reader, 4) },
                { "event_attendee_user_id", _dbHelper.SafeGetString(reader, 5) },
                { "status", _dbHelper.SafeGetString(reader, 6) },
              });
						}
						else
						{
							return BadRequest(new
							{
								status = "event_attendee not found!"
							});
						}
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new { status = exception.Message });
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("FindEventAttendees")]
		public IActionResult Get(EventAttendeeFindRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.calendar_event_id)
					&& string.IsNullOrEmpty(request.event_attendee_user_id))
				{
					return BadRequest(new
					{
						status = "Please provide calendar_event_id or event_attendee_user_id to search"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = " WHERE event_attendee.status='active' AND ";

					if (!string.IsNullOrEmpty(request.calendar_event_id))
					{
						where += "calendar_event_id='" + request.calendar_event_id + "' AND ";
					}
					if (!string.IsNullOrEmpty(request.event_attendee_user_id))
					{
						where += "event_attendee_user_id='" + request.event_attendee_user_id + "' AND ";
					}

					where = where.Remove(where.Length - 5);

					cmd.CommandText = "SELECT event_attendee.event_attendee_comment, event_attendee.event_attendee_id, event_attendee.event_attendee_optional, "
							+ "event_attendee.event_attendee_status, event_attendee.calendar_event_id, event_attendee.event_attendee_user_id, event_attendee.status, "
							+ "users.user_email, users.user_firstname, users.user_lastname, users.customer_id, "
							+ "customers.customer_name, customers.customer_address1, customers.customer_address2, customers.customer_city, "
							+ "customers.customer_state, customers.customer_zip, customers.customer_country "
							+ "FROM event_attendee "
							+ "LEFT OUTER JOIN users ON users.user_id=event_attendee.event_attendee_user_id "
							+ "LEFT OUTER JOIN customers ON users.customer_id=customers.customer_id "
							+ where;

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							var address1 = _dbHelper.SafeGetString(reader, 12);
							var address2 = _dbHelper.SafeGetString(reader, 13);
							var city = _dbHelper.SafeGetString(reader, 14);
							var state = _dbHelper.SafeGetString(reader, 15);
							var zip = _dbHelper.SafeGetString(reader, 16);
							var country = _dbHelper.SafeGetString(reader, 17);
							var companyAddress = $"{address1} {address2} {city} {state} {zip} {country}";

							result.Add(new Dictionary<string, string>
							{
								{ "event_attendee_comment", _dbHelper.SafeGetString(reader, 0) },
								{ "event_attendee_id", _dbHelper.SafeGetString(reader, 1) },
								{ "event_attendee_optional", _dbHelper.SafeGetString(reader, 2) },
								{ "event_attendee_status", _dbHelper.SafeGetString(reader, 3) },
								{ "calendar_event_id", _dbHelper.SafeGetString(reader, 4) },
								{ "event_attendee_user_id", _dbHelper.SafeGetString(reader, 5) },
								{ "status", _dbHelper.SafeGetString(reader, 6) },
								{ "user_email", _dbHelper.SafeGetString(reader, 7) },
								{ "user_firstname", _dbHelper.SafeGetString(reader, 8) },
								{ "user_lastname", _dbHelper.SafeGetString(reader, 9) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 10) },
								{ "company_name", _dbHelper.SafeGetString(reader, 11) },
								{ "company_address", companyAddress.Trim() },
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

		private string __getEventType(string calendar_event_id)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT calendar_event_type FROM calendar_events WHERE calendar_event_id='{calendar_event_id}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return _dbHelper.SafeGetString(reader, 0);
					}
				}
			}

			return "";
		}

		private string __getEventProjectId(string calendar_event_id)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT project_id FROM calendar_events WHERE calendar_event_id='{calendar_event_id}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return _dbHelper.SafeGetString(reader, 0);
					}
				}
			}

			return "";
		}
	}
}
