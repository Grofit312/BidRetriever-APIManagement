using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ProjectSourceManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class ProjectSourceManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public ProjectSourceManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateProjectSource")]
		public IActionResult CreateProjectSource(ProjectSource request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new
					{
						status = "Request can't be null."
					});
				}

				request.project_source_status = string.IsNullOrEmpty(request.project_source_status) ? "active" : request.project_source_status;

				// Verify required fields
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"user_id", "customer_id", "primary_project_id", "secondary_project_id"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// check if primary_project_id and secondary_project_id already created or not
				if (_IsExists(request.primary_project_id, request.secondary_project_id))
				{
					return Ok(new
					{
						status = $"A project source is already exist for this primary_project_id = {request.primary_project_id} and secondary_project_id={request.secondary_project_id}",
						data = getProjectSourceDetails(request.primary_project_id, request.secondary_project_id)
					});
				}

				var projectSourceId = Guid.NewGuid().ToString();
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var columns = "(project_source_id, primary_project_id, secondary_project_id, customer_id, create_datetime, create_userid, edit_datetime, edit_userid, project_source_status)";
					var values = "(@project_source_id, @primary_project_id, @secondary_project_id, @customer_id, @create_datetime, @create_userid, @edit_datetime, @edit_userid, @project_source_status)";

					cmd.CommandText = $"INSERT INTO project_sources{columns} VALUES{values};";
					cmd.Parameters.AddWithValue("@project_source_id", projectSourceId);
					cmd.Parameters.AddWithValue("@primary_project_id", request.primary_project_id);
					cmd.Parameters.AddWithValue("@secondary_project_id", request.secondary_project_id);
					cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
					cmd.Parameters.AddWithValue("@create_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@create_userid", request.user_id);
					cmd.Parameters.AddWithValue("@edit_datetime", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@edit_userid", request.user_id);
					cmd.Parameters.AddWithValue("@project_source_status", request.project_source_status);
					cmd.ExecuteNonQuery();
				}

				return Ok(new
				{
					status = "Success",
					projectSourceId
				});
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

		[HttpPost]
		[Route("UpdateProjectSource")]
		public IActionResult UpdateProjectSource(string project_source_id, string project_source_status)
		{
			try
			{
				DateTime dt = DateTime.Now;
				project_source_status = string.IsNullOrEmpty(project_source_status) ? "active" : project_source_status;
				//verify required fields
				if (string.IsNullOrEmpty(project_source_id))
				{
					return BadRequest(new
					{
						status = "project_source_id is required."
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					string query = string.Format(@"Update project_sources set project_source_status = @project_source_status, edit_datetime=@edit_datetime Where project_source_id= @project_source_id");

					cmd.Parameters.AddWithValue("@project_source_status", project_source_status);
					cmd.Parameters.AddWithValue("@project_source_id", project_source_id);
					cmd.Parameters.AddWithValue("@edit_datetime", dt);

					cmd.CommandText = query;
					int affctedrowcount = cmd.ExecuteNonQuery();
					if (affctedrowcount == 0)
					{
						return BadRequest(new
						{
							status = "No matching record found for project_source_id =" + project_source_id
						});
					}
					else
					{
						return Ok(new
						{
							status = "Successfully updated record."
						});
					}
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

		[HttpGet]
		[Route("FindProjectSources")]
		public IActionResult FindProjectSources(string project_id)
		{
			try
			{
				if (string.IsNullOrEmpty(project_id))
				{
					return BadRequest(new
					{
						status = "project_id is required."
					});
				}

				List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
        using (var cmd = _dbHelper.SpawnCommand())
        {
          var query =
          cmd.CommandText = "SELECT "
            + "project_sources.project_source_id, projects.project_name, projects.project_bid_datetime, "
            + "users.user_displayname AS project_admin_user_displayname, users.user_email AS project_admin_user_email, "
            + "users.user_phone AS project_admin_user_phone, projects.project_desc, "
            + "projects.project_assigned_office_name AS project_admin_office_name, customer_contacts.contact_display_name AS source_company_contact_name, "
            + "customer_contacts.contact_email AS source_company_contact_email, customer_contacts.contact_phone AS source_company_contact_phone, "
            + "customers.customer_name AS source_company_name, source_system_types.source_type_name AS project_source_sys_name, "
            + "projects.project_id AS secondary_project_id, projects.source_company_id as source_company_id, "
            + "customer_contacts.contact_id as source_contact_id "
            + "FROM project_sources "
            + "LEFT JOIN projects ON projects.project_id = project_sources.secondary_project_id "
            + "LEFT JOIN customer_contacts ON projects.source_company_contact_id = customer_contacts.contact_id "
            + "LEFT JOIN customers ON projects.source_company_id = customers.customer_id "
            + "LEFT JOIN users ON projects.project_admin_user_id = users.user_id "
            + "LEFT JOIN source_system_types ON projects.source_sys_type_id = source_system_types.source_sys_type_id "
            + "WHERE project_sources.primary_project_id = @project_id AND project_sources.project_source_status = 'active' "
            + "UNION "
            + "SELECT "
            + "project_sources.project_source_id, projects.project_name, projects.project_bid_datetime, "
            + "users.user_displayname AS project_admin_user_displayname, users.user_email AS project_admin_user_email, "
            + "users.user_phone AS project_admin_user_phone, projects.project_desc, "
            + "projects.project_assigned_office_name AS project_admin_office_name, customer_contacts.contact_display_name AS source_company_contact_name, "
            + "customer_contacts.contact_email AS source_company_contact_email, customer_contacts.contact_phone AS source_company_contact_phone, "
            + "customers.customer_name AS source_company_name, source_system_types.source_type_name AS project_source_sys_name, "
            + "projects.project_id AS secondary_project_id, projects.source_company_id as source_company_id, "
            + "customer_contacts.contact_id as source_contact_id "
            + "FROM project_sources "
            + "LEFT JOIN projects ON projects.project_id = project_sources.primary_project_id "
            + "LEFT JOIN customer_contacts ON projects.source_company_contact_id = customer_contacts.contact_id "
            + "LEFT JOIN customers ON projects.source_company_id = customers.customer_id "
            + "LEFT JOIN users ON projects.project_admin_user_id = users.user_id "
            + "LEFT JOIN source_system_types ON projects.source_sys_type_id = source_system_types.source_sys_type_id "
            + "WHERE project_sources.secondary_project_id=@project_id AND project_sources.project_source_status = 'active'";
             
          cmd.Parameters.AddWithValue("@project_id", project_id);

          //cmd.CommandText = query + where + " Order By project_name";


          using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								{ "project_source_id", _dbHelper.SafeGetString(reader, 0) },
								{ "project_name", _dbHelper.SafeGetString(reader, 1) },
								{ "project_bid_datetime", _dbHelper.SafeGetDatetimeString(reader, 2) },
								{ "project_admin_user_displayname", _dbHelper.SafeGetString(reader, 3) },
								{ "project_admin_user_email", _dbHelper.SafeGetString(reader, 4) },
								{ "project_admin_user_phone", _dbHelper.SafeGetString(reader, 5) },
								{ "project_desc", _dbHelper.SafeGetString(reader, 6) },
								{ "project_admin_office_name", _dbHelper.SafeGetString(reader, 7) },
								{ "source_company_contact_name", _dbHelper.SafeGetString(reader, 8) },
								{ "source_company_contact_email", _dbHelper.SafeGetString(reader, 9) },
								{ "source_company_contact_phone", _dbHelper.SafeGetString(reader, 10) },
								{ "source_company_name", _dbHelper.SafeGetString(reader, 11) },
								{ "project_source_sys_name", _dbHelper.SafeGetString(reader, 12) },
								{ "secondary_project_id", _dbHelper.SafeGetString(reader, 13) },
								{ "source_company_id", _dbHelper.SafeGetString(reader, 14) },
								{ "source_contact_id", _dbHelper.SafeGetString(reader, 15) }
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

		private bool _IsExists(string primary_project_id, string secondary_project_id)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT EXISTS (SELECT true FROM project_sources WHERE primary_project_id='{primary_project_id }' and secondary_project_id='{secondary_project_id}')";
				return (bool)cmd.ExecuteScalar();
			}
		}
		private List<Dictionary<string, object>> getProjectSourceDetails(string primary_project_id, string secondary_project_id)
		{
			List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT * FROM project_sources WHERE primary_project_id=@primary_project_id and secondary_project_id=@secondary_project_id";
				cmd.Parameters.AddWithValue("@primary_project_id", primary_project_id);
				cmd.Parameters.AddWithValue("@secondary_project_id", secondary_project_id);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new Dictionary<string, object>
						{
							{ "project_source_id",Convert.ToString(reader["project_source_id"]) },
							{ "primary_project_id", Convert.ToString(reader["primary_project_id"]) },
							{ "secondary_project_id", Convert.ToString(reader["secondary_project_id"]) },
							{ "customer_id", Convert.ToString(reader["customer_id"]) },
							{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
							{ "create_userid", Convert.ToString(reader["create_userid"]) },
							{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
							{ "edit_userid", Convert.ToString(reader["edit_userid"]) },
							{ "project_source_status", Convert.ToString(reader["project_source_status"]) }
						});
					}
				}
				return result;
			}
		}
	}
}
