using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Services.Interface;
using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    [OpenApiTag("Sharing Management")]
    public class SharingManagementController : Controller
    {
        private readonly IAmazonSimpleEmailService sesClient;

        private readonly IDbConnection _dbConnection;
        private readonly IDataViewManagementService _dataViewManagementService;

        private readonly DatabaseHelper _dbHelper;
        private string portalUrl = null;

        public SharingManagementController(
                IAmazonSimpleEmailService sesClient,
                IDbConnection dbConnection,
                IDataViewManagementService dataViewManagementService)
        {
            _dbHelper = new DatabaseHelper();

            _dbConnection = dbConnection;
            _dataViewManagementService = dataViewManagementService;
            this.sesClient = sesClient;
        }

        [HttpPost]
        [Route("CreateSharedProject")]
        public async Task<IActionResult> PostAsync(SharedProject request)
        {
            try
            {
                if (request.is_public)
                {
                    var missingParameter = request.CheckRequiredParameters(new string[]
                    {
                        "project_id",
                        "share_source_company_id",
                        "share_source_user_id"
                    });

                    if (missingParameter != null)
                    {
                        return BadRequest(new
                        {
                            status = $"{missingParameter} is required"
                        });
                    }
                }
                else
                {
                    var missingParameter = request.CheckRequiredParameters(new string[]
                    {
                        "project_id",
                        "share_source_company_id",
                        "share_source_user_id",
                        "share_user_id",
                        "share_company_id",
                    });

                    if (missingParameter != null)
                    {
                        return BadRequest(new
                        {
                            status = $"{missingParameter} is required"
                        });
                    }
                }

                var sharedProjectID = request.shared_project_id ?? Guid.NewGuid().ToString();
                var sourceCompanyName = request.share_source_company_name ?? __getSourceCompanyName(request.share_source_company_id);
                var sourceUserDisplayName = request.share_source_user_displayname ?? __getSourceUserDisplayName(request.share_source_user_id);
                var sourceOfficeID = request.share_source_office_id ?? __getSourceOfficeID(request.share_source_user_id);
                var status = request.status ?? "active";
                var timestamp = DateTime.UtcNow;

                if (request.is_public == false)
                {
                    var sharedOfficeID = request.share_office_id ?? __getSharedOfficeID(request.share_user_id);
                    var sharedCompanyID = request.share_company_id;
                    var projectName = __getProjectName(request.project_id);
                    var detailsLink = $"{__getCustomerPortalUrl()}/customer-portal/view-project/{request.project_id}/overview";
                    var viewerLink = $"{__getCustomerPortalUrl()}/customer-portal/view-project/{request.project_id}/overview";
                    var shareUserEmail = __getShareUserEmail(request.share_user_id);
                    var destinationLink = await __getDestinationLink(request.project_id);
                    var downloadLink = destinationLink.Replace("dl=0", "dl=1");

                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        // Check if project is already shared with the user
                        cmd.CommandText = "SELECT shared_project_id FROM shared_projects "
                                        + $"WHERE share_user_id='{request.share_user_id}' AND project_id='{request.project_id}' AND status='active'";

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var existingID = _dbHelper.SafeGetString(reader, 0);
                                return Ok(new { shared_project_id = existingID, status = "duplicated" });
                            }
                        }

                        // Create a shared project
                        cmd.CommandText = "INSERT INTO shared_projects "
                                        + "(share_user_id, share_company_id, project_id, create_datetime, edit_datetime, "
                                        + "shared_project_id, public, share_source_company_name, share_source_company_id, share_source_user_displayname, "
                                        + "share_source_user_id, status, share_office_id, share_source_office_id, share_type) "
                                        + "VALUES(@share_user_id, @share_company_id, @project_id, @create_datetime, @edit_datetime, "
                                        + "@shared_project_id, @public, @share_source_company_name, @share_source_company_id, @share_source_user_displayname, "
                                        + "@share_source_user_id, @status, @share_office_id, @share_source_office_id, @share_type) ";

                        cmd.Parameters.AddWithValue("share_user_id", request.share_user_id);
                        cmd.Parameters.AddWithValue("share_company_id", sharedCompanyID);
                        cmd.Parameters.AddWithValue("project_id", request.project_id);
                        cmd.Parameters.AddWithValue("create_datetime", timestamp);
                        cmd.Parameters.AddWithValue("edit_datetime", timestamp);
                        cmd.Parameters.AddWithValue("shared_project_id", sharedProjectID);
                        cmd.Parameters.AddWithValue("public", request.is_public);
                        cmd.Parameters.AddWithValue("share_source_company_name", sourceCompanyName);
                        cmd.Parameters.AddWithValue("share_source_company_id", request.share_source_company_id);
                        cmd.Parameters.AddWithValue("share_source_user_displayname", sourceUserDisplayName);
                        cmd.Parameters.AddWithValue("share_source_user_id", request.share_source_user_id);
                        cmd.Parameters.AddWithValue("status", status);
                        cmd.Parameters.AddWithValue("share_office_id", sharedOfficeID);
                        cmd.Parameters.AddWithValue("share_source_office_id", sourceOfficeID);
                        cmd.Parameters.AddWithValue("share_type", request.share_type ?? "");

                        cmd.ExecuteNonQuery();
                    }

                    // Send email
                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        cmd.CommandText = "SELECT template_from_address, template_subject_line, template_html "
                                        + "FROM notification_templates WHERE notification_type='NewSharedProject'";

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var fromAddress = _dbHelper.SafeGetString(reader, 0);
                                var subject = _dbHelper.SafeGetString(reader, 1);
                                var body = _dbHelper.SafeGetString(reader, 2);

                                subject = subject.Replace("#shared_project_name", projectName);
                                body = body.Replace("#shared_project_name", projectName)
                                                                         .Replace("#source_user_displayname", sourceUserDisplayName)
                                                                         .Replace("#project_viewer_link", viewerLink)
                                                                         .Replace("#destination_system_link", destinationLink)
                                                                         .Replace("#project_details_link", detailsLink)
                                                                         .Replace("#project_download_link", downloadLink);

                                await MailSender.SendEmailAsync(sesClient, new List<string> { shareUserEmail }, subject, body, fromAddress);
                            }
                        }
                    }
                }
                else
                {
                    using (var cmd = _dbHelper.SpawnCommand())
                    {
                        // Check if project is already public
                        cmd.CommandText = "SELECT shared_project_id FROM shared_projects "
                                        + "WHERE public=true AND project_id='" + request.project_id + "' AND status='active'";

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var existingID = _dbHelper.SafeGetString(reader, 0);
                                return Ok(new { shared_project_id = existingID, status = "duplicated" });
                            }
                        }

                        cmd.CommandText = "INSERT INTO shared_projects "
                                        + "(share_user_id, share_company_id, project_id, create_datetime, edit_datetime, "
                                        + "shared_project_id, public, share_source_company_name, share_source_company_id, share_source_user_displayname, "
                                        + "share_source_user_id, status, share_office_id, share_source_office_id) "
                                        + "VALUES(@share_user_id, @share_company_id, @project_id, @create_datetime, @edit_datetime, "
                                        + "@shared_project_id, @public, @share_source_company_name, @share_source_company_id, @share_source_user_displayname, "
                                        + "@share_source_user_id, @status, @share_office_id, @share_source_office_id) ";

                        cmd.Parameters.AddWithValue("share_user_id", "");
                        cmd.Parameters.AddWithValue("share_company_id", "");
                        cmd.Parameters.AddWithValue("project_id", request.project_id);
                        cmd.Parameters.AddWithValue("create_datetime", timestamp);
                        cmd.Parameters.AddWithValue("edit_datetime", timestamp);
                        cmd.Parameters.AddWithValue("shared_project_id", sharedProjectID);
                        cmd.Parameters.AddWithValue("public", request.is_public);
                        cmd.Parameters.AddWithValue("share_source_company_name", sourceCompanyName);
                        cmd.Parameters.AddWithValue("share_source_company_id", request.share_source_company_id);
                        cmd.Parameters.AddWithValue("share_source_user_displayname", sourceUserDisplayName);
                        cmd.Parameters.AddWithValue("share_source_user_id", request.share_source_user_id);
                        cmd.Parameters.AddWithValue("status", status);
                        cmd.Parameters.AddWithValue("share_office_id", "");
                        cmd.Parameters.AddWithValue("share_source_office_id", sourceOfficeID);

                        cmd.ExecuteNonQuery();
                    }
                }

                return Ok(new
                {
                    shared_project_id = sharedProjectID,
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
        [Route("FindSharedProjects")]
        public IActionResult Get(SharedProjectFindRequest request)
        {
            try
            {
                var portalUrl = __getCustomerPortalUrl();

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    var where = " WHERE 1=1 AND ";

                    where += !string.IsNullOrEmpty(request.shared_project_id) ? $"shared_projects.shared_project_id='{request.shared_project_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.is_public) ? $"shared_projects.public={request.is_public} AND " : "";
                    where += !string.IsNullOrEmpty(request.status) ? $"shared_projects.status='{request.status}' AND " : "";
                    where += !string.IsNullOrEmpty(request.share_company_id) ? $"shared_projects.share_company_id='{request.share_company_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.share_office_id) ? $"shared_projects.share_office_id='{request.share_office_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.share_user_email) ? $"(share_users.user_email=@share_user_email OR share_contacts.contact_email=@share_user_email) AND " : "";
                    where += !string.IsNullOrEmpty(request.share_user_id) ? $"shared_projects.share_user_id='{request.share_user_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.share_source_company_id) ? $"shared_projects.share_source_company_id='{request.share_source_company_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.share_source_office_id) ? $"shared_projects.share_source_office_id='{request.share_source_office_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.share_source_user_email) ? $"share_source_users.user_email=@share_source_user_email AND " : "";
                    where += !string.IsNullOrEmpty(request.share_source_user_id) ? $"shared_projects.share_source_user_id='{request.share_source_user_id}' AND " : "";
                    where += !string.IsNullOrEmpty(request.project_id) ? $"shared_projects.project_id='{request.project_id}' AND " : "";

                    where = where.Remove(where.Length - 5);

                    cmd.CommandText = "SELECT shared_projects.create_datetime, shared_projects.edit_datetime, "
                        + "projects.project_bid_datetime, shared_projects.public, projects.project_city, projects.project_displayname, "
                        + "shared_projects.project_id, projects.project_name, projects.project_number, "
                        + "projects.project_state, projects.project_timezone, "
                        + "share_companies.company_name, shared_projects.share_company_id, "
                        + "share_source_companies.customer_name, shared_projects.share_source_company_id, "
                        + "CONCAT(share_source_users.user_lastname, ', ', share_source_users.user_firstname) AS share_source_user_displayname, "
                        + "share_source_users.user_email, share_source_users.user_phone, "
                        + "CONCAT(COALESCE(share_users.user_lastname, share_contacts.contact_lastname), ', ', COALESCE(share_users.user_firstname, share_contacts.contact_firstname)) AS share_user_displayname, "
                        + "COALESCE(share_users.user_email, share_contacts.contact_email) AS share_user_email, "
                        + "COALESCE(share_user_offices.company_office_name, share_contacts.company_office_name) AS share_user_office_name, "
                        + "COALESCE(share_users.user_phone, share_contacts.contact_phone) AS share_user_phone, "
                        + "shared_projects.status, "
                        // All level
                        + "projects.project_address1, projects.project_address2, projects.project_country, "
                        + "projects.project_desc, projects.project_service_area, projects.project_type, projects.project_zip, "
                        // Admin level
                        + "shared_projects.create_user_id, shared_projects.edit_user_id, projects.project_password, "
                        + "shared_projects.shared_project_id, shared_projects.share_type "
                        + "FROM shared_projects "
                        + "LEFT JOIN projects ON projects.project_id=shared_projects.project_id "
                        + "LEFT JOIN customer_companies share_companies ON share_companies.company_id=shared_projects.share_company_id "
                        + "LEFT JOIN customers share_source_companies ON share_source_companies.customer_id=shared_projects.share_source_company_id "
                        + "LEFT JOIN users share_source_users ON share_source_users.user_id=shared_projects.share_source_user_id "
                        + "LEFT JOIN users share_users ON share_users.user_id=shared_projects.share_user_id "
                        + "LEFT JOIN customer_contacts share_contacts ON share_contacts.contact_id=shared_projects.share_user_id "
                        + "LEFT JOIN company_offices share_user_offices ON share_user_offices.company_office_id=shared_projects.share_office_id "
                        + where;

                    using (var reader = cmd.ExecuteReader())
                    {
                        var result = new List<Dictionary<string, object>> { };

                        while (reader.Read())
                        {
                            var projectID = _dbHelper.SafeGetString(reader, "project_id");
                            var sourceUrl = $"{portalUrl}/customer-portal/view-project/{projectID}/overview";

                            var item = new Dictionary<string, object>
                            {
                                { "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 0) },
                                { "edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 1) },
                                { "project_bid_datetime", _dbHelper.SafeGetDatetimeString(reader, 2) },
                                { "project_city", _dbHelper.SafeGetString(reader, "project_city") },
                                { "project_displayname", _dbHelper.SafeGetString(reader, "project_displayname") },
                                { "project_id", _dbHelper.SafeGetString(reader, "project_id") },
                                { "project_name", _dbHelper.SafeGetString(reader, "project_name") },
                                { "project_number", _dbHelper.SafeGetString(reader, "project_number") },
                                { "project_state", _dbHelper.SafeGetString(reader, "project_state") },
                                { "project_timezone", _dbHelper.SafeGetString(reader, "project_timezone") },
                                { "public", _dbHelper.SafeGetBooleanRaw(reader, 3) },
                                { "share_company_name", _dbHelper.SafeGetString(reader, "company_name") },
                                { "share_company_id", _dbHelper.SafeGetString(reader, "share_company_id") },
                                { "share_source_company_name", _dbHelper.SafeGetString(reader, "customer_name") },
                                { "share_source_company_id", _dbHelper.SafeGetString(reader, "share_source_company_id") },
                                { "share_source_user_displayname", _dbHelper.SafeGetString(reader, "share_source_user_displayname") },
                                { "share_source_user_email", _dbHelper.SafeGetString(reader, "user_email") },
                                { "share_source_user_phone", _dbHelper.SafeGetString(reader, "user_phone") },
                                { "status", _dbHelper.SafeGetString(reader, "status") },
                                { "share_source_url", sourceUrl },
                                { "share_user_email", _dbHelper.SafeGetString(reader, "share_user_email") },
                                { "share_user_phone", _dbHelper.SafeGetString(reader, "share_user_phone") },
                                { "share_user_displayname", _dbHelper.SafeGetString(reader, "share_user_displayname") },
                                { "shared_project_id", _dbHelper.SafeGetString(reader, "shared_project_id") },
                                { "share_type", _dbHelper.SafeGetString(reader, "share_type") },
								{ "share_user_office_name", _dbHelper.SafeGetString(reader, "share_user_office_name") },
                            };

                            if (request.detail_level == "all" || request.detail_level == "admin")
                            {
                                item["project_address1"] = _dbHelper.SafeGetString(reader, "project_address1");
                                item["project_address2"] = _dbHelper.SafeGetString(reader, "project_address2");
                                item["project_country"] = _dbHelper.SafeGetString(reader, "project_country");
                                item["project_desc"] = _dbHelper.SafeGetString(reader, "project_desc");
                                item["project_service_area"] = _dbHelper.SafeGetString(reader, "project_service_area");
                                item["project_type"] = _dbHelper.SafeGetString(reader, "project_type");
                                item["project_zip"] = _dbHelper.SafeGetString(reader, "project_zip");
                            }

                            if (request.detail_level == "admin")
                            {
                                item["create_user_id"] = _dbHelper.SafeGetString(reader, "create_user_id");
                                item["edit_user_id"] = _dbHelper.SafeGetString(reader, "edit_user_id");
                                item["project_password"] = _dbHelper.SafeGetString(reader, "project_password");
                            }

                            result.Add(item);
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
        [Route("UpdateSharedProject")]
        public IActionResult Post(SharedProjectUpdateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.search_shared_project_id))
                {
                    return BadRequest(new { status = "search_shared_project_id is required" });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = $"UPDATE shared_projects SET public={request.is_public}, edit_datetime=@edit_datetime, ";
                    cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

                    if (!string.IsNullOrEmpty(request.share_office_id))
                    {
                        cmd.CommandText += $"share_office_id='{request.share_office_id}', ";
                    }
                    if (!string.IsNullOrEmpty(request.share_user_id))
                    {
                        var sharedCompanyID = __getSharedCompanyID(request.share_user_id);

                        cmd.CommandText += $"share_user_id='{request.share_user_id}', share_company_id='{sharedCompanyID}', ";
                    }
                    if (!string.IsNullOrEmpty(request.status))
                    {
                        cmd.CommandText += $"status='{request.status}', ";
                    }
                    if (!string.IsNullOrEmpty(request.share_type))
                    {
                        cmd.CommandText += $"share_type='{request.share_type}', ";
                    }

                    cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 2);
                    cmd.CommandText += $" WHERE shared_project_id='{request.search_shared_project_id}'";

                    var updatedCount = cmd.ExecuteNonQuery();

                    if (updatedCount > 0)
                    {
                        return Ok(new
                        {
                            status = "updated"
                        });
                    }

                    return BadRequest(new
                    {
                        status = "failed to update"
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


        private string __getSharedCompanyID(string shared_user_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT customer_id FROM users WHERE user_id='{shared_user_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }

                    return "";
                }
            }
        }

        private string __getSharedOfficeID(string share_user_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT customer_office_id FROM users WHERE user_id='{share_user_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }
                }

                cmd.CommandText = $"SELECT company_office_id FROM customer_contacts WHERE contact_id='{share_user_id}'";

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

        public string __getSourceCompanyName(string source_company_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT customer_name FROM customers WHERE customer_id='{source_company_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }

                    return "";
                }
            }
        }

        private string __getSourceUserDisplayName(string source_user_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT user_firstname, user_lastname FROM users WHERE user_id='{source_user_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var firstName = _dbHelper.SafeGetString(reader, 0);
                        var lastName = _dbHelper.SafeGetString(reader, 1);

                        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                        {
                            return $"{lastName}, {firstName}";
                        }
                        else
                        {
                            return $"{firstName}{lastName}";
                        }
                    }

                    return "";
                }
            }
        }

        private string __getSourceOfficeID(string source_user_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT customer_office_id FROM users WHERE user_id='{source_user_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }

                    return "";
                }
            }
        }

        private string __getCustomerPortalUrl()
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT setting_value FROM system_settings WHERE system_setting_id='CUSTOMER_PORTAL_URL'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }

                    return "";
                }
            }
        }

        private string __getProjectName(string project_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT project_name FROM projects WHERE project_id='{project_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }

                    return "";
                }
            }
        }

        private string __getShareUserEmail(string user_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"SELECT user_email FROM users WHERE user_id='{user_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetString(reader, 0);
                    }
                }

                cmd.CommandText = $"SELECT contact_email FROM customer_contacts WHERE contact_id='{user_id}'";

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

        private async Task<string> __getDestinationLink(string project_id)
        {
            var response = await new ProjectManagementController(_dbConnection, _dataViewManagementService).GetAsync(new ProjectGetLinkRequest
            {
                project_id = project_id,
            }) as OkObjectResult;

            if (response != null)
            {
                var json = JsonConvert.SerializeObject(response.Value);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                return dict["url"];
            }

            return string.Empty;

        }
    }
}
