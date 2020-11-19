using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Models.Project;
using _440DocumentManagement.Services.Interface;
using Amazon.DynamoDBv2;
using Amazon.Lambda;
using Dropbox.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    [OpenApiTag("Project Management")]
    public class ProjectManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        private readonly IDbConnection _dbConnection;
        private readonly IDataViewManagementService _dataViewManagementService;

        public ProjectManagementController(
          IDbConnection dbConnection,
          IDataViewManagementService dataViewManagementService)
        {
            _dbHelper = new DatabaseHelper();

            _dbConnection = dbConnection;
            _dataViewManagementService = dataViewManagementService;
        }

        [HttpPost]
        [Route("SubmitProject")]
        public IActionResult Post(ProjectSubmission projectSubmission)
        {
            try
            {
                // check missing parameter
                var missingParameter = projectSubmission.CheckRequiredParameters(new string[]
                { "user_id", "submission_id", "submitter_email" });

                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                // create project
                var projectId = Guid.NewGuid().ToString();

                var createProjectResult = Post(new DLProject()
                {
                    project_admin_user_id = projectSubmission.user_id,
                    project_id = projectId,
                    project_name = projectSubmission.project_name,
                    project_number = projectSubmission.project_number,
                    project_customer_id = projectSubmission.customer_id,
                    status = projectSubmission.status,
                    project_bid_datetime = projectSubmission.project_bid_datetime,
                    project_password = projectSubmission.project_password,
                    project_type = projectSubmission.project_type,
                    source_password = projectSubmission.project_password,
                    source_token = projectSubmission.source_token,
                    source_url = projectSubmission.source_url,
                    source_username = projectSubmission.source_username,
                    source_sys_type_id = projectSubmission.source_sys_type_id,
                    project_process_status = projectSubmission.project_process_status,
                    project_process_message = projectSubmission.project_process_message,
                    source_project_id = projectSubmission.source_project_id
                }, true);

                if (createProjectResult is BadRequestObjectResult)
                {
                    return createProjectResult;
                }

                // create project_submission
                var projectSubmissionId = projectSubmission.submission_id ?? Guid.NewGuid().ToString();

                var createProjectSubmissionResult = Post(new DLProjectSubmission()
                {
                    user_id = projectSubmission.user_id,
                    submission_id = projectSubmissionId,
                    submission_name = projectSubmission.submission_name,
                    project_id = projectId,
                    submitter_email = projectSubmission.submitter_email,
                    customer_id = projectSubmission.customer_id,
                    source_url = projectSubmission.source_url,
                    source_sys_type_id = projectSubmission.source_sys_type_id,
                    username = projectSubmission.username,
                    password = projectSubmission.password,
                    inbound_email = projectSubmission.inbound_email,
                    received_datetime = projectSubmission.received_datetime,
                    project_name = projectSubmission.project_name,
                    project_number = projectSubmission.project_number,
                    status = projectSubmission.status,
                    submission_email_file_bucket = projectSubmission.submission_email_file_bucket,
                    submission_email_file_key = projectSubmission.submission_email_file_key,
                }, true);

                if (createProjectSubmissionResult is BadRequestObjectResult)
                {
                    return createProjectSubmissionResult;
                }

                // return ok
                return Ok(new
                {
                    project_submission_id = projectSubmissionId,
                    project_id = projectId,
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


        [HttpPost]
        [Route("CreateProjectDL")]
        public IActionResult Post(DLProject project, bool isInternalRequest = false)
        {
            try
            {

                var contactController = new ContactManagementController();

                // check missing parameter
                var missingParameter = project.CheckRequiredParameters(new string[] { "project_admin_user_id" });

                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                var projectId = project.project_id ?? Guid.NewGuid().ToString();
                var timestamp = DateTime.UtcNow;
                projectId = projectId.Replace("'", "''");

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    // check project_id already exists
                    cmd.CommandText = $"SELECT EXISTS (SELECT true FROM projects WHERE project_id='{projectId}')";

                    if ((bool)cmd.ExecuteScalar() == true)
                    {
                        return Ok(new
                        {
                            project_id = projectId,
                            status = "duplicated"
                        });
                    }

                    // now create project record
                    cmd.CommandText = "INSERT INTO projects (project_id, project_name, project_number, project_admin_user_id, "
                      + "project_address1, project_address2, project_city, project_state, project_zip, "
                      + "project_country, project_service_area, project_owner_name, project_desc, "
                      + "project_bid_datetime, project_type, project_customer_id, status, create_datetime, edit_datetime, "
                      + "auto_update_status, customer_source_sys_id, project_password, project_timezone, source_url, source_username, source_password, source_token, source_sys_type_id, project_notes, project_process_status, project_process_message, project_rating, "
                      + "project_award_status, project_building_type, project_contract_type, project_construction_type, project_labor_requirement, "
                      + "project_segment, project_size, project_stage, project_value, source_company_contact_id,source_company_id, source_user_id, "
                      + "project_assigned_office_id, project_assigned_office_name, project_displayname, source_project_id, num_proj_sources, bid_month)"
                      + "VALUES(@project_id, @project_name, @project_number, @project_admin_user_id, @project_address1, @project_address2, "
                      + "@project_city, @project_state, @project_zip, @project_country, @project_service_area, "
                      + "@project_owner_name, @project_desc, @project_bid_datetime, @project_type, "
                      + "@project_customer_id, @status, @create_datetime, @edit_datetime, "
                      + "@auto_update_status, @customer_source_sys_id, @project_password, @project_timezone, @source_url, @source_username, @source_password, @source_token, @source_sys_type_id, @project_notes, @project_process_status, @project_process_message, @project_rating, "
                      + "@project_award_status, @project_building_type, @project_contract_type, @project_construction_type, @project_labor_requirement, "
                      + "@project_segment, @project_size, @project_stage, @project_value, @source_company_contact_id, @source_company_id, @source_user_id, "
                      + "@project_assigned_office_id, @project_assigned_office_name, @project_displayname, @source_project_id, @num_proj_sources, @bid_month)";

                    cmd.Parameters.AddWithValue("project_id", projectId);
                    cmd.Parameters.AddWithValue("project_name", project.project_name ?? "");
                    cmd.Parameters.AddWithValue(
                        "project_displayname",
                        string.IsNullOrEmpty(project.project_displayname)
                        ? (project.project_name ?? "") : project.project_displayname);
                    cmd.Parameters.AddWithValue("project_number", project.project_number ?? "");
                    cmd.Parameters.AddWithValue("project_admin_user_id", project.project_admin_user_id);
                    cmd.Parameters.AddWithValue("project_address1", project.project_address1 ?? "");
                    cmd.Parameters.AddWithValue("project_address2", project.project_address2 ?? "");
                    cmd.Parameters.AddWithValue("project_city", project.project_city ?? "");
                    cmd.Parameters.AddWithValue("project_state", project.project_state ?? "");
                    cmd.Parameters.AddWithValue("project_zip", project.project_zip ?? "");
                    cmd.Parameters.AddWithValue("project_country", project.project_country ?? "");
                    cmd.Parameters.AddWithValue("project_service_area", project.project_service_area ?? "");
                    cmd.Parameters.AddWithValue("project_owner_name", project.project_owner_name ?? "");
                    cmd.Parameters.AddWithValue("project_desc", project.project_desc ?? "");
                    cmd.Parameters.AddWithValue("project_bid_datetime", project.project_bid_datetime != null ? (object)DateTimeHelper.ConvertToUTCDateTime(project.project_bid_datetime) : DBNull.Value);
                    cmd.Parameters.AddWithValue("bid_month", project.project_bid_datetime != null ? (object)DateTimeHelper.ConvertToUTCYearMonth(project.project_bid_datetime) : DBNull.Value);
                    cmd.Parameters.AddWithValue("project_type", project.project_type ?? "");
                    cmd.Parameters.AddWithValue("project_customer_id", project.project_customer_id ?? "");
                    cmd.Parameters.AddWithValue("status", project.status ?? "active");
                    cmd.Parameters.AddWithValue("create_datetime", timestamp);
                    cmd.Parameters.AddWithValue("edit_datetime", timestamp);
                    cmd.Parameters.AddWithValue("auto_update_status", project.auto_update_status ?? "inactive");
                    cmd.Parameters.AddWithValue("customer_source_sys_id", project.customer_source_sys_id ?? "");
                    cmd.Parameters.AddWithValue("project_password", project.project_password ?? "");
                    cmd.Parameters.AddWithValue("project_timezone", project.project_timezone ?? "");
                    cmd.Parameters.AddWithValue("source_url", project.source_url ?? "");
                    cmd.Parameters.AddWithValue("source_username", project.source_username ?? "");
                    cmd.Parameters.AddWithValue("source_password", project.source_password ?? "");
                    cmd.Parameters.AddWithValue("source_token", project.source_token ?? "");
                    cmd.Parameters.AddWithValue("source_sys_type_id", project.source_sys_type_id ?? "");
                    cmd.Parameters.AddWithValue("project_notes", project.project_notes ?? "");
                    cmd.Parameters.AddWithValue("project_process_status", project.project_process_status ?? "");
                    cmd.Parameters.AddWithValue("project_process_message", project.project_process_message ?? "");
                    cmd.Parameters.AddWithValue("project_rating", (object)project.project_rating ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_award_status", project.project_award_status ?? "");
                    cmd.Parameters.AddWithValue("project_building_type", project.project_building_type ?? "");
                    cmd.Parameters.AddWithValue("project_contract_type", project.project_contract_type ?? "");
                    cmd.Parameters.AddWithValue("project_construction_type", project.project_construction_type ?? "");
                    cmd.Parameters.AddWithValue("project_labor_requirement", project.project_labor_requirement ?? "");
                    cmd.Parameters.AddWithValue("project_segment", project.project_segment ?? "");
                    cmd.Parameters.AddWithValue("project_size", project.project_size ?? "");
                    cmd.Parameters.AddWithValue("project_stage", project.project_stage ?? "");
                    cmd.Parameters.AddWithValue("project_value", (object)project.project_value ?? DBNull.Value);
                    //cmd.Parameters.AddWithValue("source_company_id", project.source_company_id ?? project.project_customer_id);
                    cmd.Parameters.AddWithValue("source_company_contact_id", (object)project.source_company_contact_id ?? "");
                    cmd.Parameters.AddWithValue("source_company_id", project.source_company_id ?? "");
                    cmd.Parameters.AddWithValue("source_user_id", project.source_user_id ?? "");
                    cmd.Parameters.AddWithValue("project_assigned_office_id", project.project_assigned_office_id ?? "");
                    cmd.Parameters.AddWithValue("project_assigned_office_name", project.project_assigned_office_name ?? "");
                    cmd.Parameters.AddWithValue("source_project_id", project.source_project_id ?? "");
                    cmd.Parameters.AddWithValue("num_proj_sources", (object)project.num_proj_sources ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                // Create root folders in project_folders table
                var folderCreateResult = new DocumentManagementController(null, null, null).Post(new DLProjectFolder
                {
                    folder_name = "Source Files",
                    folder_type = "source_current",
                    project_id = projectId,
                });

                if (folderCreateResult is BadRequestObjectResult)
                {
                    return folderCreateResult;
                }

                folderCreateResult = new DocumentManagementController(null, null, null).Post(new DLProjectFolder
                {
                    folder_name = "Plans-All",
                    folder_type = "plans_all",
                    project_id = projectId,
                });

                if (folderCreateResult is BadRequestObjectResult)
                {
                    return folderCreateResult;
                }

                if (__getSourceFileSubmissionFolderSetting(project.project_admin_user_id) == "enabled")
                {
                    folderCreateResult = new DocumentManagementController(null, null, null).Post(new DLProjectFolder
                    {
                        folder_name = "Source Files-Submissions",
                        folder_type = "source_history",
                        project_id = projectId,
                    });

                    if (folderCreateResult is BadRequestObjectResult)
                    {
                        return folderCreateResult;
                    }
                }

                // Create project settings based on current customer setting
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    var destinationAccessToken = "";
                    var destinationAccessToken2 = "";
                    var destinationUrl = "";
                    var destinationRootPath = "";
                    var destinationTypeId = "";
                    var destinationUsername = "";
                    var destinationPassword = "";
                    var destinationId = "";
                    var userEmail = "";
                    var userLastName = "";
                    var customerName = "";
                    var destinationPath = "";

                    cmd.CommandText = "SELECT customer_destinations.destination_access_token, customer_destinations.destination_url, "
                      + "customer_destinations.destination_root_path, customer_destinations.destination_type_id, "
                      + "customer_destinations.destination_username, customer_destinations.destination_password, "
                      + "users.user_email, users.user_lastname, customers.customer_name,"
                      + "customer_destinations.destination_id, customers.customer_id, "
                      + "customer_destinations.destination_access_token_2 "
                      + "FROM users LEFT JOIN customers ON users.customer_id=customers.customer_id "
                      + "LEFT JOIN customer_destinations on customer_destinations.customer_id=COALESCE(customers.customer_id, 'TrialUser') "
                      + "WHERE users.user_id='" + project.project_admin_user_id + "'";

                    var destinationSetting = new Dictionary<string, string> { };
                    var customerId = "";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            destinationSetting = new Dictionary<string, string>
                            {
                                { "destination_access_token", _dbHelper.SafeGetString(reader, 0) },
                                { "destination_access_token_2", _dbHelper.SafeGetString(reader, 11) },
                                { "destination_url", _dbHelper.SafeGetString(reader, 1) },
                                { "destination_root_path", _dbHelper.SafeGetString(reader, 2) },
                                { "destination_type_id", _dbHelper.SafeGetString(reader, 3) },
                                { "destination_username", _dbHelper.SafeGetString(reader, 4) },
                                { "destination_password", _dbHelper.SafeGetString(reader, 5) },
                                { "user_email", _dbHelper.SafeGetString(reader, 6) },
                                { "user_lastname", _dbHelper.SafeGetString(reader, 7) },
                                { "customer_name", _dbHelper.SafeGetString(reader, 8) },
                                { "destination_id", _dbHelper.SafeGetString(reader, 9) }
                            };

                            customerId = _dbHelper.SafeGetString(reader, 10);
                        }
                    }

                    destinationAccessToken = destinationSetting["destination_access_token"];
                    destinationAccessToken2 = destinationSetting["destination_access_token_2"];
                    destinationUrl = destinationSetting["destination_url"];
                    destinationRootPath = destinationSetting["destination_root_path"];
                    destinationTypeId = destinationSetting["destination_type_id"];
                    destinationUsername = destinationSetting["destination_username"];
                    destinationPassword = destinationSetting["destination_password"];
                    destinationId = destinationSetting["destination_id"];
                    userEmail = destinationSetting["user_email"];
                    userLastName = destinationSetting["user_lastname"];
                    customerName = destinationSetting["customer_name"];

                    if (string.IsNullOrEmpty(destinationTypeId) || string.IsNullOrEmpty(destinationRootPath) || string.IsNullOrEmpty(destinationAccessToken))
                    {
                        cmd.CommandText = "SELECT destination_access_token, destination_url, "
                          + "destination_root_path, destination_type_id, "
                          + "destination_username, destination_password, destination_id "
                                        + "FROM customer_destinations "
                          + "WHERE customer_id='TrialUser'";

                        using (var trialSettingReader = cmd.ExecuteReader())
                        {
                            if (trialSettingReader.Read())
                            {
                                destinationAccessToken = _dbHelper.SafeGetString(trialSettingReader, 0);
                                destinationUrl = _dbHelper.SafeGetString(trialSettingReader, 1);
                                destinationRootPath = _dbHelper.SafeGetString(trialSettingReader, 2);
                                destinationTypeId = _dbHelper.SafeGetString(trialSettingReader, 3);
                                destinationUsername = _dbHelper.SafeGetString(trialSettingReader, 4);
                                destinationPassword = _dbHelper.SafeGetString(trialSettingReader, 5);
                                destinationId = _dbHelper.SafeGetString(trialSettingReader, 6);
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    status = "Failed to locate destination info"
                                });
                            }
                        }
                    }

                    // replacement in destination root path
                    destinationRootPath = destinationRootPath.Replace("<project_admin_user_email>", userEmail);
                    destinationRootPath = destinationRootPath.Replace("<customer_name>", customerName);
                    destinationRootPath = destinationRootPath.Replace("<project_admin_user_lastname>", userLastName);
                    destinationRootPath = destinationRootPath.Replace("<project_create_date>", timestamp.ToString("yyyy-MM-dd"));
                    destinationRootPath = destinationRootPath.Replace("<project_create_year>", timestamp.ToString("yyyy"));
                    destinationRootPath = destinationRootPath.Replace("<project_create_year_month>", timestamp.ToString("yyyy-MM"));

                    if (string.IsNullOrEmpty(project.project_name))
                    {
                        destinationPath = $"/{ValidationHelper.ValidateDestinationPath(destinationRootPath)}";
                    }
                    else
                    {
                        destinationPath = $"/{ValidationHelper.ValidateDestinationPath(destinationRootPath)}/{ValidationHelper.ValidateProjectName(project.project_name)}";
                    }

                    // save settings

                    new ProjectSettingManagementController().Post(new ProjectSetting
                    {
                        project_id = projectId,
                        setting_value = destinationPath,
                        setting_name = "PROJECT_DESTINATION_PATH",
                        setting_value_data_type = "string",
                    });

                    new ProjectSettingManagementController().Post(new ProjectSetting
                    {
                        project_id = projectId,
                        setting_value = destinationTypeId,
                        setting_name = "PROJECT_DESTINATION_TYPE_ID",
                        setting_value_data_type = "string",
                    });

                    new ProjectSettingManagementController().Post(new ProjectSetting()
                    {
                        project_id = projectId,
                        setting_value = destinationId,
                        setting_name = "PROJECT_DESTINATION_ID",
                        setting_value_data_type = "string"
                    });

                    if (destinationUsername != string.Empty)
                    {
                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = destinationUsername,
                            setting_name = "PROJECT_DESTINATION_USERNAME",
                            setting_value_data_type = "string",
                        });
                    }

                    if (destinationPassword != string.Empty)
                    {
                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = destinationPassword,
                            setting_name = "PROJECT_DESTINATION_PASSWORD",
                            setting_value_data_type = "string",
                        });
                    }

                    if (destinationAccessToken != string.Empty)
                    {
                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = destinationAccessToken,
                            setting_name = "PROJECT_DESTINATION_TOKEN",
                            setting_value_data_type = "string",
                        });
                    }

                    if (destinationAccessToken2 != string.Empty)
                    {
                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = destinationAccessToken2,
                            setting_name = "PROJECT_DESTINATION_TOKEN_2",
                            setting_value_data_type = "string",
                        });
                    }

                    if (destinationUrl != string.Empty)
                    {
                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = destinationUrl,
                            setting_name = "PROJECT_DESTINATION_URL",
                            setting_value_data_type = "string",
                        });
                    }

                    // Copy document settings
                    cmd.CommandText = $"SELECT setting_id, setting_value FROM customer_settings WHERE customer_id='{customerId}'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        var customerSettings = new List<Dictionary<string, string>> { };

                        while (reader.Read())
                        {
                            customerSettings.Add(new Dictionary<string, string>
                            {
                                { "setting_id", _dbHelper.SafeGetString(reader, 0) },
                                { "setting_value", _dbHelper.SafeGetString(reader, 1) },
                            });
                        }

                        var revisioningType = customerSettings.Find(setting => setting["setting_id"] == "revisioning_type");
                        var planFileNaming = customerSettings.Find(setting => setting["setting_id"] == "plan_file_naming");
                        var sourceFileSubmissionFolder = customerSettings.Find(setting => setting["setting_id"] == "source_file_submission_folder");
                        var currentPlansFolder = customerSettings.Find(setting => setting["setting_id"] == "current_plans_folder");
                        var allPlansFolder = customerSettings.Find(setting => setting["setting_id"] == "all_plans_folder");
                        var allPlansSubmissionFolder = customerSettings.Find(setting => setting["setting_id"] == "all_plans_submission_folder");
                        var comparisonPlansFolder = customerSettings.Find(setting => setting["setting_id"] == "comparison_plans_folder");
                        var disciplinePlansFolder = customerSettings.Find(setting => setting["setting_id"] == "discipline_plans_folder");
                        var rasterPlansFolder = customerSettings.Find(setting => setting["setting_id"] == "raster_plans_folder");
                        var rasterPlansOutputType = customerSettings.Find(setting => setting["setting_id"] == "raster_plans_output_type");

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = revisioningType == null ? "Submission Date" : revisioningType["setting_value"],
                            setting_name = "PROJECT_REVISIONING_TYPE",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = planFileNaming == null ? "<doc_num>__<doc_name>__<doc_revision>" : planFileNaming["setting_value"],
                            setting_name = "PROJECT_PLAN_FILE_NAMING",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = sourceFileSubmissionFolder == null ? "disabled" : sourceFileSubmissionFolder["setting_value"],
                            setting_name = "PROJECT_SOURCE_FILE_SUBMISSION_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = currentPlansFolder == null ? "enabled" : currentPlansFolder["setting_value"],
                            setting_name = "PROJECT_CURRENT_PLANS_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = allPlansFolder == null ? "enabled" : allPlansFolder["setting_value"],
                            setting_name = "PROJECT_ALL_PLANS_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = allPlansSubmissionFolder == null ? "disabled" : allPlansSubmissionFolder["setting_value"],
                            setting_name = "PROJECT_ALL_PLANS_SUBMISSION_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = comparisonPlansFolder == null ? "separate_comparison_folder" : comparisonPlansFolder["setting_value"],
                            setting_name = "PROJECT_COMPARISON_PLANS_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = disciplinePlansFolder == null ? "disabled" : disciplinePlansFolder["setting_value"],
                            setting_name = "PROJECT_DISCIPLINE_PLANS_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = rasterPlansFolder == null ? "disabled" : rasterPlansFolder["setting_value"],
                            setting_name = "PROJECT_RASTER_PLANS_FOLDER",
                            setting_value_data_type = "string",
                        });

                        new ProjectSettingManagementController().Post(new ProjectSetting
                        {
                            project_id = projectId,
                            setting_value = rasterPlansOutputType == null ? "disabled" : rasterPlansOutputType["setting_value"],
                            setting_name = "PROJECT_RASTER_PLANS_OUTPUT_TYPE",
                            setting_value_data_type = "string",
                        });
                    }
                }

                // create project events
                if (!string.IsNullOrEmpty(project.project_bid_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_bid_datetime,
                        "Bid Due Date",
                        "project_bid_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_award_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_award_datetime,
                        "Award Date",
                        "project_award_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_contract_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_contract_datetime,
                        "Contract Date",
                        "project_contract_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_expected_award_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_expected_award_datetime,
                        "Expected Award Date",
                        "project_expected_award_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_expected_contract_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_expected_contract_datetime,
                        "Expected Contract Date",
                        "project_expected_contract_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_prebid_mtg_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_prebid_mtg_datetime,
                        "Prebid Meeting Date",
                        "project_prebid_mtg_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_start_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_start_datetime,
                        "Start Date",
                        "project_start_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_complete_datetime))
                {
                    __createProjectEvent(projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_complete_datetime,
                        "Finish Date",
                        "project_complete_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_work_start_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_work_start_datetime,
                        "Work Start Date",
                        "project_work_start_datetime");
                }
                if (!string.IsNullOrEmpty(project.project_work_end_datetime))
                {
                    __createProjectEvent(
                        projectId,
                        project.project_name ?? "",
                        project.project_admin_user_id,
                        project.project_customer_id,
                        project.project_work_end_datetime,
                        "Work Finish Date",
                        "project_work_end_datetime");
                }

                return Ok(new
                {
                    project_id = projectId,
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
                if (isInternalRequest == false)
                {
                    _dbHelper.CloseConnection();
                }
            }
        }


        [HttpPost]
        [Route("CreateProjectSubmissionDL")]
        public IActionResult Post(DLProjectSubmission projectSubmission, bool isInternalRequest = false)
        {
            try
            {
                // check missing parameter
                var missingParameter = projectSubmission.CheckRequiredParameters(new string[] { "user_id", "submitter_email" });

                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                var projectSubmissionId = projectSubmission.submission_id ?? Guid.NewGuid().ToString();
                var timestamp = DateTime.UtcNow;

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    // check project_id already exists
                    cmd.CommandText = $"SELECT EXISTS (SELECT true FROM project_submissions WHERE project_submission_id='{projectSubmissionId}')";

                    if ((bool)cmd.ExecuteScalar() == true)
                    {
                        return Ok(new { project_submission_id = projectSubmissionId, status = "duplicated" });
                    }

                    // create project_submissions record
                    var columns = "(project_submission_id, user_id, customer_id, submitter_email, source_url, project_id, project_name, "
                                            + "source_sys_type_id, username, password, inbound_email, received_datetime, "
                                            + "project_number, status, create_datetime, edit_datetime, user_timezone, submission_name, submission_process_status, submission_process_message, "
                                            + "submission_email_file_bucket, submission_email_file_key, submission_type)";
                    var values = "(@project_submission_id, @user_id, @customer_id, @submitter_email, @source_url, @project_id, @project_name, "
                                            + "@source_sys_type_id, @username, @password, @inbound_email, @received_datetime, "
                                            + "@project_number, @status, @create_datetime, @edit_datetime, @user_timezone, @submission_name, @submission_process_status, @submission_process_message, "
                                            + "@submission_email_file_bucket, @submission_email_file_key, @submission_type)";

                    cmd.CommandText = $"INSERT INTO project_submissions {columns} VALUES{values}";

                    cmd.Parameters.AddWithValue("project_submission_id", projectSubmissionId);
                    cmd.Parameters.AddWithValue("user_id", projectSubmission.user_id);
                    cmd.Parameters.AddWithValue("customer_id", projectSubmission.customer_id ?? "");
                    cmd.Parameters.AddWithValue("submitter_email", projectSubmission.submitter_email);
                    cmd.Parameters.AddWithValue("source_url", projectSubmission.source_url ?? "");
                    cmd.Parameters.AddWithValue("project_id", projectSubmission.project_id ?? "");
                    cmd.Parameters.AddWithValue("project_name", projectSubmission.project_name ?? "");
                    cmd.Parameters.AddWithValue("source_sys_type_id", projectSubmission.source_sys_type_id ?? "");
                    cmd.Parameters.AddWithValue("username", projectSubmission.username ?? "");
                    cmd.Parameters.AddWithValue("password", projectSubmission.password ?? "");
                    cmd.Parameters.AddWithValue("inbound_email", projectSubmission.inbound_email ?? "");
                    cmd.Parameters.AddWithValue(
                        "received_datetime",
                        projectSubmission.received_datetime != null
                        ? (object)DateTimeHelper.ConvertToUTCDateTime(projectSubmission.received_datetime)
                        : DBNull.Value);
                    cmd.Parameters.AddWithValue("project_number", projectSubmission.project_number ?? "");
                    cmd.Parameters.AddWithValue("status", projectSubmission.status ?? "active");
                    cmd.Parameters.AddWithValue("create_datetime", timestamp);
                    cmd.Parameters.AddWithValue("edit_datetime", timestamp);
                    cmd.Parameters.AddWithValue("user_timezone", projectSubmission.user_timezone ?? "");
                    cmd.Parameters.AddWithValue("submission_name", projectSubmission.submission_name ?? "");
                    cmd.Parameters.AddWithValue("submission_process_status", projectSubmission.submission_process_status ?? "");
                    cmd.Parameters.AddWithValue("submission_process_message", projectSubmission.submission_process_message ?? "");
                    cmd.Parameters.AddWithValue("submission_email_file_bucket", projectSubmission.submission_email_file_bucket ?? "");
                    cmd.Parameters.AddWithValue("submission_email_file_key", projectSubmission.submission_email_file_key ?? "");
                    cmd.Parameters.AddWithValue("submission_type", projectSubmission.submission_type ?? "");

                    cmd.ExecuteNonQuery();

                    return Ok(new
                    {
                        project_submission_id = projectSubmissionId,
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
                if (isInternalRequest == false)
                {
                    _dbHelper.CloseConnection();
                }
            }
        }


        [HttpGet]
        [Route("FindProjects")]
        public IActionResult Get(ProjectFindRequest request)
        {
            try
            {
                var detailLevel = request.detail_level ?? "basic";

                // validation check
                if (request.project_id == null
                    && request.source_url == null
                    && request.user_email == null
                    && request.user_id == null
                    && request.customer_id == null
                    && request.auto_update_status == null
                    && request.status == null
                    && request.project_timezone == null
                    && request.project_process_status == null
                    && request.project_assigned_office_id == null
                    && request.source_company_domain == null
                    && request.source_company_id == null
                    && request.source_system_url == null
                    && request.view_id == null
                    && request.source_project_id == null)
                {
                    return BadRequest(new
                    {
                        status = "please provide at least one query parameter"
                    });
                }

                if (detailLevel != "basic"
                    && detailLevel != "all"
                    && detailLevel != "admin"
                    && detailLevel != "compact")
                {
                    return BadRequest(new
                    {
                        status = "incorrect detail_level"
                    });
                }

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    // build query string
                    var whereString = " WHERE ";
                    if (!string.IsNullOrEmpty(request.auto_update_status))
                    {
                        whereString += "projects.auto_update_status=@auto_update_status AND ";
                        cmd.Parameters.AddWithValue("@auto_update_status", request.auto_update_status);
                    }
                    if (!string.IsNullOrEmpty(request.customer_id))
                    {
                        whereString += "users.customer_id=@customer_id AND ";
                        cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
                    }
                    whereString += request.source_url != null ? $"projects.source_url LIKE '%{request.source_url}%' AND " : "";
                    if (!string.IsNullOrEmpty(request.project_assigned_office_id))
                    {
                        whereString += "projects.project_assigned_office_id=@project_assigned_office_id AND ";
                        cmd.Parameters.AddWithValue("@project_assigned_office_id", request.project_assigned_office_id);
                    }
                    if (!string.IsNullOrEmpty(request.project_id))
                    {
                        whereString += "projects.project_id=@project_id AND ";
                        cmd.Parameters.AddWithValue("@project_id", request.project_id);
                    }
                    if (!string.IsNullOrEmpty(request.project_process_status))
                    {
                        whereString += "projects.project_process_status=@project_process_status AND ";
                        cmd.Parameters.AddWithValue("@project_process_status", request.project_process_status);
                    }
                    if (!string.IsNullOrEmpty(request.project_timezone))
                    {
                        whereString += "projects.project_timezone=@project_timezone AND ";
                        cmd.Parameters.AddWithValue("@project_timezone", request.project_timezone);
                    }
                    if (!string.IsNullOrEmpty(request.source_company_domain))
                    {
                        whereString += "projects.source_company_domain=@source_company_domain AND ";
                        cmd.Parameters.AddWithValue("@source_company_domain", request.source_company_domain);
                    }
                    if (!string.IsNullOrEmpty(request.source_company_id))
                    {
                        whereString += "projects.source_company_id=@source_company_id AND ";
                        cmd.Parameters.AddWithValue("@source_company_id", request.source_company_id);
                    }
                    if (!string.IsNullOrEmpty(request.status))
                    {
                        whereString += "projects.status=@status AND ";
                        cmd.Parameters.AddWithValue("@status", request.status);
                    }
                    if (!string.IsNullOrEmpty(request.user_email))
                    {
                        whereString += "LOWER(users.user_email)=@user_email AND ";
                        cmd.Parameters.AddWithValue("@user_email", request.user_email.ToLower());
                    }
                    if (!string.IsNullOrEmpty(request.user_id))
                    {
                        whereString += "projects.project_admin_user_id=@user_id AND ";
                        cmd.Parameters.AddWithValue("@user_id", request.user_id);
                    }
                    if (!string.IsNullOrEmpty(request.source_project_id))
                    {
                        whereString += "projects.source_project_id=@source_project_id AND ";
                        cmd.Parameters.AddWithValue("@source_project_id", request.source_project_id);
                    }
                    whereString = whereString.Remove(whereString.Length - 5);

                    if (request.detail_level == "compact")
                    {
                        cmd.CommandText = "SELECT users.user_firstname, users.user_lastname,"
                            + "projects.project_bid_datetime, projects.project_displayname, projects.project_id, "
                            + "projects.project_name, projects.project_number, projects.create_datetime, projects.project_timezone "
                            + "FROM projects "
                            + "LEFT JOIN users ON users.user_id=projects.project_admin_user_id "
                            + whereString;

                        // Execute Query


                        using (var reader = cmd.ExecuteReader())
                        {
                            var resultList = new List<Dictionary<string, object>>();

                            while (reader.Read())
                            {
                                resultList.Add(new Dictionary<string, object>
                                {
                                    { "project_admin_user_fullname", $"{_dbHelper.SafeGetString(reader, 0)} {_dbHelper.SafeGetString(reader, 1)}" },
                                    { "project_bid_datetime", _dbHelper.SafeGetDatetimeString(reader, 2) },
                                    { "project_displayname", _dbHelper.SafeGetString(reader, 3) },
                                    { "project_id", _dbHelper.SafeGetString(reader, 4) },
                                    { "project_name", _dbHelper.SafeGetString(reader, 5) },
                                    { "project_number", _dbHelper.SafeGetString(reader, 6) },
                                    { "create_datetime", _dbHelper.SafeGetDatetimeString(reader, 7) },
                                    { "project_timezone", _dbHelper.SafeGetString(reader, 8) }
                                });
                            }

                            reader.Close();
                            return Ok(resultList);
                        }
                    }

                    whereString = whereString + " Order By project_name";
                    cmd.CommandText = "SELECT projects.create_datetime, users.customer_id, customers.customer_name, "
                        + "projects.edit_datetime, users.user_email, users.user_firstname, users.user_lastname, "
                        + "users.user_phone, projects.project_id, projects.project_name, projects.project_admin_user_id, "
                        + "projects.status, projects.auto_update_status, projects.customer_source_sys_id, "
                        + "projects.project_timezone, projects.source_url, projects.source_username, projects.source_password, projects.source_token, projects.source_sys_type_id,  "
                        + "projects.project_number, projects.project_desc, projects.project_address1, projects.project_address2, "
                        + "projects.project_city, projects.project_state, projects.project_zip, projects.project_country, "
                        + "projects.project_service_area, projects.project_bid_datetime, projects.project_type, "
                        + "projects.create_user_id, projects.edit_user_id, projects.project_password, projects.project_notes, source_system_types.source_type_name, "
                        + "projects.project_process_status, projects.project_process_message, projects.project_rating, "
                        + "projects.project_contract_type, projects.project_stage, projects.project_segment, projects.project_building_type, projects.project_labor_requirement, "
                        + "projects.project_value, projects.project_size, projects.project_construction_type, projects.project_award_status, "
                        + "projects.project_assigned_office_id, projects.project_assigned_office_name, "
                        + "projects.project_displayname, ps.project_submission_id as original_submission_id, projects.source_project_id, projects.num_proj_sources, "
                        + "cast(extract(epoch FROM (projects.project_bid_datetime - now())) as int) as time_till_bid "
                        + "FROM projects "
                        + "LEFT JOIN users ON users.user_id=projects.project_admin_user_id "
                        + "LEFT JOIN customers ON users.customer_id=customers.customer_id "
                        + "LEFT JOIN source_system_types ON source_system_types.source_sys_type_id=projects.source_sys_type_id "
                        + "INNER JOIN LATERAL (SELECT project_submission_id FROM project_submissions WHERE project_submissions.project_id=projects.project_id ORDER BY create_datetime ASC LIMIT 1) ps ON TRUE "
                        + whereString;


                    // execute query
                    using (var reader = cmd.ExecuteReader())
                    {
                        var resultList = new List<Dictionary<string, object>>();

                        while (reader.Read())
                        {
                            var result = new Dictionary<string, object>();

                            result.Add("auto_update_status", _dbHelper.SafeGetString(reader, 12));
                            result.Add("create_datetime", _dbHelper.SafeGetDatetimeString(reader, 0));
                            result.Add("customer_name", _dbHelper.SafeGetString(reader, 2));
                            result.Add("edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 3));
                            result.Add("project_admin_user_email", _dbHelper.SafeGetString(reader, 4));
                            result.Add(
                                "project_admin_user_fullname",
                                $"{_dbHelper.SafeGetString(reader, 5)} {_dbHelper.SafeGetString(reader, 6)}");
                            result.Add("project_admin_user_phone", _dbHelper.SafeGetString(reader, 7));
                            result.Add("project_assigned_office_name", _dbHelper.SafeGetString(reader, 49));
                            result.Add("project_bid_datetime", _dbHelper.SafeGetDatetimeString(reader, 29));
                            result.Add("project_city", _dbHelper.SafeGetString(reader, 24));
                            result.Add("customer_id", _dbHelper.SafeGetString(reader, 1));
                            result.Add("project_displayname", _dbHelper.SafeGetString(reader, 50));
                            result.Add("project_id", _dbHelper.SafeGetString(reader, 8));
                            result.Add("project_name", _dbHelper.SafeGetString(reader, 9));
                            result.Add("project_notes", _dbHelper.SafeGetString(reader, 34));
                            result.Add("project_number", _dbHelper.SafeGetString(reader, 20));
                            result.Add("project_process_status", _dbHelper.SafeGetString(reader, 36));
                            result.Add("project_process_message", _dbHelper.SafeGetString(reader, 37));
                            result.Add("project_rating", _dbHelper.SafeGetInteger(reader, 38));
                            result.Add("project_state", _dbHelper.SafeGetString(reader, 25));
                            result.Add("project_timezone", _dbHelper.SafeGetString(reader, 14));
                            result.Add("project_admin_user_id", _dbHelper.SafeGetString(reader, 10));
                            result.Add("customer_source_sys_id", _dbHelper.SafeGetString(reader, 13));
                            result.Add("source_password", _dbHelper.SafeGetString(reader, 17));
                            result.Add("source_sys_type_id", _dbHelper.SafeGetString(reader, 19));
                            result.Add("source_sys_type_name", _dbHelper.SafeGetString(reader, 35));
                            result.Add("source_token", _dbHelper.SafeGetString(reader, 18));
                            result.Add("source_url", _dbHelper.SafeGetString(reader, 15));
                            result.Add("source_username", _dbHelper.SafeGetString(reader, 16));
                            result.Add("status", _dbHelper.SafeGetString(reader, 11));
                            result.Add("project_contract_type", _dbHelper.SafeGetString(reader, 39));
                            result.Add("project_stage", _dbHelper.SafeGetString(reader, 40));
                            result.Add("project_segment", _dbHelper.SafeGetString(reader, 41));
                            result.Add("project_building_type", _dbHelper.SafeGetString(reader, 42));
                            result.Add("project_labor_requirement", _dbHelper.SafeGetString(reader, 43));
                            result.Add("project_value", _dbHelper.SafeGetIntegerRaw(reader, 44));
                            result.Add("project_size", _dbHelper.SafeGetString(reader, 45));
                            result.Add("project_construction_type", _dbHelper.SafeGetString(reader, 46));
                            result.Add("project_award_status", _dbHelper.SafeGetString(reader, 47));
                            result.Add("source_project_id", _dbHelper.SafeGetString(reader, 52));
                            result.Add("num_proj_sources", _dbHelper.SafeGetIntegerRaw(reader, 53));
                            result.Add("time_till_bid", _dbHelper.SafeGetIntegerRaw(reader, 54));
                            result["last_change_date"] = result["edit_datetime"];

                            if (detailLevel == "admin" || detailLevel == "all")
                            {
                                result.Add("project_address1", _dbHelper.SafeGetString(reader, 22));
                                result.Add("project_address2", _dbHelper.SafeGetString(reader, 23));
                                result.Add("project_assigned_office_id", _dbHelper.SafeGetString(reader, 48));
                                result.Add("project_country", _dbHelper.SafeGetString(reader, 27));
                                result.Add("project_desc", _dbHelper.SafeGetString(reader, 21));
                                result.Add("project_type", _dbHelper.SafeGetString(reader, 30));
                                result.Add("project_zip", _dbHelper.SafeGetString(reader, 26));
                                result.Add("project_service_area", _dbHelper.SafeGetString(reader, 28));
                            }

                            if (detailLevel == "admin")
                            {
                                result.Add("create_user_id", _dbHelper.SafeGetString(reader, 31));
                                result.Add("edit_user_id", _dbHelper.SafeGetString(reader, 32));
                                result.Add("project_password", _dbHelper.SafeGetString(reader, 33));
                                result.Add("original_submission_id", _dbHelper.SafeGetString(reader, 51));
                            }

                            resultList.Add(result);
                        }

                        reader.Close();

                        resultList.ForEach(result =>
                        {
                            var sourceInfo = __getProjectSourceInfo(result["project_id"] as string);

                            result.Add("source_company_id", sourceInfo["source_company_id"]);
                            result.Add("source_company_name", sourceInfo["source_company_name"]);
                            result.Add("source_user_id", sourceInfo["source_user_id"]);
                            result.Add("source_contact_email", sourceInfo["source_contact_email"]);
                            result.Add("source_contact_phone", sourceInfo["source_contact_phone"]);
                            result.Add("contact_firstname", sourceInfo["contact_firstname"]);
                            result.Add("contact_lastname", sourceInfo["contact_lastname"]);
                        });
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

        [HttpGet]
        [Route("FindProjects2")]
        public IActionResult FindProjects2(ProjectFindRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.view_id))
                {
                    request.status = request.status ?? "active";
                    return Get(request);
                }

                // Run DataView Action
                var dataView = _dataViewManagementService.GetDataViewDetails(_dbHelper, request.view_id);
                var sourceColumnsList = dataView.ViewSource.DataSourceFields.Where(
                  (field) => field.DataSourceFieldName.ToLower().StartsWith("source_")).ToList();
                dataView.ViewSource.DataSourceFields = dataView.ViewSource.DataSourceFields.Where(
                  (field) => !field.DataSourceFieldName.ToLower().StartsWith("source_")).ToList();
                var requiredTableNames = new List<string>();

                var resultList = new List<Dictionary<string, object>>();
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    var whereString = " WHERE ";
                    if (!string.IsNullOrEmpty(request.auto_update_status))
                    {
                        whereString += "projects.auto_update_status=@auto_update_status AND ";
                        cmd.Parameters.AddWithValue("@auto_update_status", request.auto_update_status);
                    }
                    if (!string.IsNullOrEmpty(request.customer_id))
                    {
                        whereString += "users.customer_id=@customer_id AND ";
                        cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
                        requiredTableNames.Add("users");
                    }
                    whereString += request.source_url != null ? $"projects.source_url LIKE '%{request.source_url}%' AND " : "";
                    if (!string.IsNullOrEmpty(request.project_assigned_office_id))
                    {
                        whereString += "projects.project_assigned_office_id=@project_assigned_office_id AND ";
                        cmd.Parameters.AddWithValue("@project_assigned_office_id", request.project_assigned_office_id);
                    }
                    if (!string.IsNullOrEmpty(request.project_id))
                    {
                        whereString += "projects.project_id=@project_id AND ";
                        cmd.Parameters.AddWithValue("@project_id", request.project_id);
                    }
                    if (!string.IsNullOrEmpty(request.project_process_status))
                    {
                        whereString += "projects.project_process_status=@project_process_status AND ";
                        cmd.Parameters.AddWithValue("@project_process_status", request.project_process_status);
                    }
                    if (!string.IsNullOrEmpty(request.project_timezone))
                    {
                        whereString += "projects.project_timezone=@project_timezone AND ";
                        cmd.Parameters.AddWithValue("@project_timezone", request.project_timezone);
                    }
                    if (!string.IsNullOrEmpty(request.source_company_domain))
                    {
                        whereString += "projects.source_company_domain=@source_company_domain AND ";
                        cmd.Parameters.AddWithValue("@source_company_domain", request.source_company_domain);
                    }
                    if (!string.IsNullOrEmpty(request.source_company_id))
                    {
                        whereString += "projects.source_company_id=@source_company_id AND ";
                        cmd.Parameters.AddWithValue("@source_company_id", request.source_company_id);
                    }
                    if (!string.IsNullOrEmpty(request.status))
                    {
                        whereString += "projects.status=@status AND ";
                        cmd.Parameters.AddWithValue("@status", request.status);
                    }
                    if (!string.IsNullOrEmpty(request.user_email))
                    {
                        whereString += "LOWER(users.user_email)=@user_email AND ";
                        cmd.Parameters.AddWithValue("@user_email", request.user_email.ToLower());
                        if (requiredTableNames.IndexOf("users") == -1)
                        {
                            requiredTableNames.Add("users");
                        }
                    }
                    if (!string.IsNullOrEmpty(request.user_id))
                    {
                        whereString += "projects.project_admin_user_id=@user_id AND ";
                        cmd.Parameters.AddWithValue("@user_id", request.user_id);
                    }
                    whereString = whereString.Remove(whereString.Length - 5);

                    var query = _dataViewManagementService.GenerateQuery(dataView, "projects", requiredTableNames) + whereString;
                    if (!string.IsNullOrEmpty(dataView.ViewFilter.DataViewFilterSql))
                    {
                        if (dataView.ViewFilter.DataViewFilterSql.Contains("@office_id")
                          && !cmd.Parameters.TryGetValue("@office_id", out _))
                        {
                            using (var cmdUser = _dbHelper.SpawnCommand())
                            {
                                cmdUser.CommandText = "SELECT customer_office_id FROM users where user_id=@user_id";
                                cmdUser.Parameters.AddWithValue("@user_id", request.user_id);
                                using (var reader = cmdUser.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        if (reader.Read())
                                        {
                                            cmd.Parameters.AddWithValue("@office_id", reader["customer_office_id"]);
                                        }
                                        else
                                        {
                                            cmd.Parameters.AddWithValue("@office_id", DBNull.Value);
                                        }
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@office_id", DBNull.Value);
                                    }
                                }
                            }
                        }
                        if (dataView.ViewFilter.DataViewFilterSql.Contains("@user_id")
                          && !cmd.Parameters.TryGetValue("@user_id", out _))
                        {
                            cmd.Parameters.AddWithValue("@user_id", (object)request.user_id ?? DBNull.Value);
                        }

                        var comfortableFilterSql = ApiExtension.FilterDateRange(dataView.ViewFilter.DataViewFilterSql, cmd);
                        query += $" AND ({comfortableFilterSql})";
                    }
                    cmd.CommandText = query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        var readerColumns = new List<string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            readerColumns.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            var eachRow = new Dictionary<string, object>();
                            readerColumns.ForEach((columnName) =>
                            {
                                eachRow.Add(columnName, ApiExtension.GetString(reader[columnName]));
                            });

                            if (eachRow["project_bid_datetime"] == null || string.IsNullOrEmpty(eachRow["project_bid_datetime"] as string))
                            {
                                eachRow["time_till_bid"] = 0;
                            }
                            else
                            {
                                var bidDateTime = DateTimeHelper.ConvertToUTCDateTime(eachRow["project_bid_datetime"] as string);
                                eachRow["time_till_bid"] = Convert.ToInt32(bidDateTime.Subtract(DateTime.UtcNow).TotalSeconds);
                            }

                            resultList.Add(eachRow);
                        }
                    }

                    // Check if source columns are existed
                    if (sourceColumnsList.Count > 0)
                    {
                        resultList.ForEach((result) =>
                        {
                            var sourceInfo = __getProjectSourceInfo(result["project_id"].ToString());
                            sourceColumnsList.ForEach((sourceColumnField) =>
                            {
                                result.Add(
                                    sourceColumnField.DataSourceFieldName,
                                    sourceInfo.ContainsKey(sourceColumnField.DataSourceFieldName) ?
                                    sourceInfo[sourceColumnField.DataSourceFieldName] : "");
                            });
                        });
                    }
                }
                return Ok(resultList);
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
        [Route("UpdateProject")]
        public IActionResult Post(ProjectUpdateRequest request)
        {
            try
            {
                // validation check
                if (request.search_project_id == null)
                {
                    return BadRequest(new { status = "search_project_id is required" });
                }

                // update
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    var originProjectName = "";

                    cmd.CommandText = $"SELECT project_name FROM projects WHERE project_id='{request.search_project_id}'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            originProjectName = _dbHelper.SafeGetString(reader, 0);
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                status = "no matching project found"
                            });
                        }
                    }

                    var isRemovingBidDatetime = !string.IsNullOrEmpty(request.project_bid_datetime) && request.project_bid_datetime == "NULL";
                    var whereString = $" WHERE project_id='{request.search_project_id}'";
                    var queryString = "UPDATE projects SET "
                      + "project_name = COALESCE(@project_name, project_name), "
                      + "project_displayname = COALESCE(@project_displayname, project_displayname), "
                      + "project_address1 = COALESCE(@project_address1, project_address1), "
                      + "project_address2 = COALESCE(@project_address2, project_address2), "
                      + "project_city = COALESCE(@project_city, project_city), "
                      + "project_state = COALESCE(@project_state, project_state), "
                      + "project_zip = COALESCE(@project_zip, project_zip), "
                      + "project_country = COALESCE(@project_country, project_country), "
                      + "project_service_area = COALESCE(@project_service_area, project_service_area), "
                      + "project_number = COALESCE(@project_number, project_number), "
                      + "project_owner_name = COALESCE(@project_owner_name, project_owner_name), "
                      + "project_desc = COALESCE(@project_desc, project_desc), "
                      + (isRemovingBidDatetime ? "project_bid_datetime = NULL, " : "project_bid_datetime = COALESCE(@project_bid_datetime, project_bid_datetime), ")
                      + (isRemovingBidDatetime ? "bid_month = NULL, " : "bid_month = COALESCE(@bid_month, bid_month), ")
                      + "project_type = COALESCE(@project_type, project_type), "
                      + "status = COALESCE(@status, status), "
                      + "auto_update_status = COALESCE(@auto_update_status, auto_update_status), "
                      + "customer_source_sys_id = COALESCE(@customer_source_sys_id, customer_source_sys_id), "
                      + "project_password = COALESCE(@project_password, project_password), "
                      + "project_timezone = COALESCE(@project_timezone, project_timezone), "
                      + "source_url = COALESCE(@source_url, source_url), "
                      + "source_username = COALESCE(@source_username, source_username), "
                      + "source_password = COALESCE(@source_password, source_password), "
                      + "source_token = COALESCE(@source_token, source_token), "
                      + "source_sys_type_id = COALESCE(@source_sys_type_id, source_sys_type_id), "
                      + "project_notes = COALESCE(@project_notes, project_notes), "
                      + "project_process_status = COALESCE(@project_process_status, project_process_status), "
                      + "project_process_message = COALESCE(@project_process_message, project_process_message), "
                      + "project_rating = COALESCE(@project_rating, project_rating), "
                      + "project_contract_type = COALESCE(@project_contract_type, project_contract_type), "
                      + "project_stage = COALESCE(@project_stage, project_stage), "
                      + "project_segment = COALESCE(@project_segment, project_segment), "
                      + "project_building_type = COALESCE(@project_building_type, project_building_type), "
                      + "project_labor_requirement = COALESCE(@project_labor_requirement, project_labor_requirement), "
                      + "project_value = COALESCE(@project_value, project_value), "
                      + "project_size = COALESCE(@project_size, project_size), "
                      + "project_construction_type = COALESCE(@project_construction_type, project_construction_type), "
                      + "project_award_status = COALESCE(@project_award_status, project_award_status), "
                      + "source_company_id = COALESCE(@source_company_id, source_company_id), "
                      + "source_user_id = COALESCE(@source_user_id, source_user_id), "
                      + "source_company_contact_id = COALESCE(@source_company_contact_id, source_company_contact_id), "
                      + "project_assigned_office_id = COALESCE(@project_assigned_office_id, project_assigned_office_id), "
                      + "project_assigned_office_name = COALESCE(@project_assigned_office_name, project_assigned_office_name), "
                      + "source_project_id = COALESCE(@source_project_id, source_project_id), "
                      + "num_proj_sources = COALESCE(@num_proj_sources, num_proj_sources), "
                      + "edit_datetime = @edit_datetime";

                    if (true == true) // check if api_key has admin access
                    {
                        queryString = queryString + ", "
                          + "project_parent_id = COALESCE(@project_parent_id, project_parent_id), "
                          + "project_admin_user_id = COALESCE(@project_admin_user_id, project_admin_user_id), "
                          + "project_customer_id = COALESCE(@project_customer_id, project_customer_id)";
                    }

                    queryString = queryString + whereString;

                    cmd.CommandText = queryString;
                    cmd.Parameters.AddWithValue("project_name", (object)request.project_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_displayname", (object)request.project_displayname ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_address1", (object)request.project_address1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_address2", (object)request.project_address2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_city", (object)request.project_city ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_state", (object)request.project_state ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_zip", (object)request.project_zip ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_country", (object)request.project_country ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_service_area", (object)request.project_service_area ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_number", (object)request.project_number ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_owner_name", (object)request.project_owner_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_desc", (object)request.project_desc ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_type", (object)request.project_type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("project_parent_id", (object)request.project_parent_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_admin_user_id", (object)request.project_admin_user_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_customer_id", (object)request.project_customer_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("auto_update_status", (object)request.auto_update_status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("customer_source_sys_id", (object)request.customer_source_sys_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_password", (object)request.project_password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_timezone", (object)request.project_timezone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_url", (object)request.source_url ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_username", (object)request.source_username ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_password", (object)request.source_password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_token", (object)request.source_token ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_sys_type_id", (object)request.source_sys_type_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_notes", (object)request.project_notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_process_status", (object)request.project_process_status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_process_message", (object)request.project_process_message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_rating", (object)request.project_rating ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_contract_type", (object)request.project_contract_type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_stage", (object)request.project_stage ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_segment", (object)request.project_segment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_building_type", (object)request.project_building_type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_labor_requirement", (object)request.project_labor_requirement ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_value", (object)request.project_value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_size", (object)request.project_size ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_construction_type", (object)request.project_construction_type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_award_status", (object)request.project_award_status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_company_id", (object)request.source_company_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_company_contact_id", (object)request.source_company_contact_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_user_id", (object)request.source_user_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_assigned_office_id", (object)request.project_assigned_office_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_assigned_office_name", (object)request.project_assigned_office_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_project_id", (object)request.source_project_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("num_proj_sources", (object)request.num_proj_sources ?? DBNull.Value);

                    if (!isRemovingBidDatetime)
                    {
                        cmd.Parameters.AddWithValue("project_bid_datetime",
                        request.project_bid_datetime != null
                        ? (object)DateTimeHelper.ConvertToUTCDateTime(request.project_bid_datetime)
                        : DBNull.Value);

                        cmd.Parameters.AddWithValue("bid_month",
                        request.project_bid_datetime != null
                        ? (object)DateTimeHelper.ConvertToUTCYearMonth(request.project_bid_datetime)
                        : DBNull.Value);
                    }

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        return BadRequest(new
                        {
                            status = "no matching project found"
                        });
                    }

                    if (isRemovingBidDatetime)
                    {
                        cmd.CommandText = $"SELECT calendar_event_id FROM calendar_events WHERE project_id='{request.search_project_id}' AND calendar_event_type='project_bid_datetime' AND status='active'";

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var calendarEventId = _dbHelper.SafeGetString(reader, 0);

                                var calendarEventUpdateResult = new CalendarEventManagementController().Post(new CalendarEventUpdateRequest
                                {
                                    search_calendar_event_id = calendarEventId,
                                    status = "deleted",
                                });

                                if (calendarEventUpdateResult is BadRequestObjectResult)
                                {
                                    return calendarEventUpdateResult;
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(request.project_bid_datetime))
                    {
                        cmd.CommandText = $"SELECT calendar_event_id FROM calendar_events WHERE project_id='{request.search_project_id}' AND calendar_event_type='project_bid_datetime' AND status='active'";

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var calendarEventId = _dbHelper.SafeGetString(reader, 0);

                                reader.Close();

                                var calendarEventUpdateResult = new CalendarEventManagementController().Post(new CalendarEventUpdateRequest
                                {
                                    search_calendar_event_id = calendarEventId,
                                    calendar_event_start_datetime = request.project_bid_datetime,
                                    calendar_event_end_datetime = request.project_bid_datetime,
                                });

                                if (calendarEventUpdateResult is BadRequestObjectResult)
                                {
                                    return calendarEventUpdateResult;
                                }
                            }
                            else
                            {
                                reader.Close();

                                cmd.CommandText = "SELECT project_name, project_admin_user_id, project_customer_id FROM projects WHERE project_id='" + request.search_project_id + "'";

                                using (var projectReader = cmd.ExecuteReader())
                                {
                                    if (projectReader.Read())
                                    {
                                        __createProjectEvent(request.search_project_id,
                                            _dbHelper.SafeGetString(projectReader, 0),
                                            _dbHelper.SafeGetString(projectReader, 1),
                                            _dbHelper.SafeGetString(projectReader, 2),
                                            request.project_bid_datetime,
                                            "Bid Due Date",
                                            "project_bid_datetime");
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(request.project_name) && !originProjectName.Equals(request.project_name))
                    {
                        var destinationPath = "";

                        cmd.CommandText = $"SELECT setting_value FROM project_settings WHERE setting_name='PROJECT_DESTINATION_PATH' AND project_id='{request.search_project_id}'";

                        using(var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                destinationPath = _dbHelper.SafeGetString(reader, 0);

                                if (string.IsNullOrEmpty(originProjectName))
                                {
                                    destinationPath += $"/{ValidationHelper.ValidateProjectName(request.project_name)}";
                                }
                                else
                                {
                                    destinationPath = destinationPath.Replace(ValidationHelper.ValidateProjectName(originProjectName), ValidationHelper.ValidateProjectName(request.project_name));
                                }
                            }
                        }

                        cmd.CommandText = $"UPDATE project_settings SET setting_value='{destinationPath}' WHERE project_id='{request.search_project_id}' AND setting_name='PROJECT_DESTINATION_PATH'";
                        cmd.ExecuteNonQuery();
                    }

                    return Ok(new
                    {
                        status = "completed"
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
        [Route("UpdateProjectSubmission")]
        public IActionResult Post(ProjectSubmissionUpdateRequest request)
        {
            try
            {
                // validation check
                if (request.search_project_submission_id == null)
                {
                    return BadRequest(new
                    {
                        status = "search_project_submission_id is required"
                    });
                }

                // update
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    var whereString = $" WHERE project_submission_id='{request.search_project_submission_id}'";
                    var queryString = "UPDATE project_submissions SET "
                      + "project_name = COALESCE(@project_name, project_name), "
                      + "project_id = COALESCE(@project_id, project_id), "
                      + "username = COALESCE(@username, username), "
                      + "password = COALESCE(@password, password), "
                      + "project_number = COALESCE(@project_number, project_number), "
                      + "status = COALESCE(@status, status), "
                      + "submission_name = COALESCE(@submission_name, submission_name), "
                      + "submission_process_status = COALESCE(@submission_process_status, submission_process_status), "
                      + "submission_process_message = COALESCE(@submission_process_message, submission_process_message), "
                      + "source_url = COALESCE(@source_url, source_url), "
                      + "submission_email_file_bucket = COALESCE(@submission_email_file_bucket, submission_email_file_bucket), "
                      + "submission_email_file_key = COALESCE(@submission_email_file_key, submission_email_file_key), "
                      + "submission_type = COALESCE(@submission_type, submission_type), "
                      + "edit_datetime = @edit_datetime";

                    if (true == true) // check if api_key has admin access
                    {
                        queryString = queryString + ", "
                          + "customer_id = COALESCE(@customer_id, customer_id), "
                          + "source_sys_type_id = COALESCE(@source_sys_type_id, source_sys_type_id)";
                    }

                    queryString = queryString + whereString;

                    cmd.CommandText = queryString;
                    cmd.Parameters.AddWithValue("project_name", (object)request.project_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_id", (object)request.project_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("username", (object)request.username ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("password", (object)request.password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("project_number", (object)request.project_number ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("submission_name", (object)request.submission_name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("customer_id", (object)request.customer_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_sys_type_id", (object)request.source_sys_type_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("submission_process_status", (object)request.submission_process_status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("submission_process_message", (object)request.submission_process_message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("source_url", (object)request.source_url ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("submission_email_file_bucket", (object)request.submission_email_file_bucket ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("submission_email_file_key", (object)request.submission_email_file_key ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("submission_type", (object)request.submission_type ?? DBNull.Value);

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        return BadRequest(new
                        {
                            status = "no matching project submission found"
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
        [Route("GetProject")]
        public IActionResult Get(ProjectGetRequest request)
        {
            try
            {
                // validation check
                if (request.project_id == null)
                {
                    return BadRequest(new { status = "project_id is required" });
                }

                // get project
                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = "SELECT projects.create_datetime, projects.edit_datetime, projects.project_address1, "
                      + "projects.project_address2, projects.project_bid_datetime, projects.project_city, projects.project_country, "
                      + "projects.project_desc, projects.project_id, projects.project_name, projects.project_number, projects.project_service_area, "
                      + "projects.project_state, projects.project_type, projects.project_zip, projects.status, projects.auto_update_status, projects.customer_source_sys_id, projects.project_timezone, projects.source_url, projects.source_sys_type_id, "
                      + "users.user_id, users.customer_id, "
                      + "projects.create_user_id, projects.edit_user_id, projects.project_password, projects.source_username, projects.source_password, projects.source_token, projects.project_notes, "
                      + "projects.project_process_status, projects.project_process_message, project_rating, "
                      + "projects.project_contract_type, projects.project_stage, projects.project_segment, projects.project_building_type, projects.project_labor_requirement, "
                      + "projects.project_value, projects.project_size, projects.project_construction_type, projects.project_award_status, source_system_types.source_type_name, users.user_email, "
                      + "projects.project_assigned_office_id, projects.project_assigned_office_name, "
                      + "projects.project_displayname, projects.num_proj_sources, "
                      + "cast(extract(epoch FROM (projects.project_bid_datetime - now())) as int) as time_till_bid, projects.source_company_contact_id  "
                      + "FROM projects "
                      + "LEFT JOIN users ON projects.project_admin_user_id=users.user_id "
                      + "LEFT JOIN source_system_types ON source_system_types.source_sys_type_id=projects.source_sys_type_id "
                      + "WHERE projects.project_id='" + request.project_id + "'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var detailLevel = request.detail_level ?? "basic";
                            var result = new Dictionary<string, object>();

                            result.Add("create_datetime", _dbHelper.SafeGetDatetimeString(reader, 0));
                            result.Add("edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 1));
                            result.Add("project_address1", _dbHelper.SafeGetString(reader, 2));
                            result.Add("project_address2", _dbHelper.SafeGetString(reader, 3));
                            result.Add("project_bid_datetime", _dbHelper.SafeGetDatetimeString(reader, 4));
                            result.Add("project_city", _dbHelper.SafeGetString(reader, 5));
                            result.Add("project_country", _dbHelper.SafeGetString(reader, 6));
                            result.Add("project_desc", _dbHelper.SafeGetString(reader, 7));
                            result.Add("project_id", _dbHelper.SafeGetString(reader, 8));
                            result.Add("project_name", _dbHelper.SafeGetString(reader, 9));
                            result.Add("project_number", _dbHelper.SafeGetString(reader, 10));
                            result.Add("project_service_area", _dbHelper.SafeGetString(reader, 11));
                            result.Add("project_state", _dbHelper.SafeGetString(reader, 12));
                            result.Add("project_type", _dbHelper.SafeGetString(reader, 13));
                            result.Add("project_zip", _dbHelper.SafeGetString(reader, 14));
                            result.Add("status", _dbHelper.SafeGetString(reader, 15));
                            result.Add("auto_update_status", _dbHelper.SafeGetString(reader, 16));
                            result.Add("customer_source_sys_id", _dbHelper.SafeGetString(reader, 17));
                            result.Add("project_timezone", _dbHelper.SafeGetString(reader, 18));
                            result.Add("source_url", _dbHelper.SafeGetString(reader, 19));
                            result.Add("source_sys_type_id", _dbHelper.SafeGetString(reader, 20));
                            result.Add("project_notes", _dbHelper.SafeGetString(reader, 29));
                            result.Add("project_process_status", _dbHelper.SafeGetString(reader, 30));
                            result.Add("project_process_message", _dbHelper.SafeGetString(reader, 31));
                            result.Add("project_rating", _dbHelper.SafeGetInteger(reader, 32));
                            result.Add("project_contract_type", _dbHelper.SafeGetString(reader, 33));
                            result.Add("project_stage", _dbHelper.SafeGetString(reader, 34));
                            result.Add("project_segment", _dbHelper.SafeGetString(reader, 35));
                            result.Add("project_building_type", _dbHelper.SafeGetString(reader, 36));
                            result.Add("project_labor_requirement", _dbHelper.SafeGetString(reader, 37));
                            result.Add("project_value", _dbHelper.SafeGetIntegerRaw(reader, 38));
                            result.Add("project_size", _dbHelper.SafeGetString(reader, 39));
                            result.Add("project_construction_type", _dbHelper.SafeGetString(reader, 40));
                            result.Add("project_award_status", _dbHelper.SafeGetString(reader, 41));
                            result.Add("source_sys_type_name", _dbHelper.SafeGetString(reader, 42));
                            result.Add("project_assigned_office_id", _dbHelper.SafeGetString(reader, 44));
                            result.Add("project_assigned_office_name", _dbHelper.SafeGetString(reader, 45));
                            result.Add("project_displayname", _dbHelper.SafeGetString(reader, 46));
                            result.Add("num_proj_sources", _dbHelper.SafeGetIntegerRaw(reader, 47));
                            result.Add("time_till_bid", _dbHelper.SafeGetIntegerRaw(reader, 48));
                            result.Add("source_company_contact_id", _dbHelper.SafeGetString(reader, 49));


                            if (detailLevel == "admin")
                            {
                                result.Add("project_admin_user_id", _dbHelper.SafeGetString(reader, 21));
                                result.Add("project_admin_user_email", _dbHelper.SafeGetString(reader, 43));
                                // Note: Should deprecate those fields - to be consistent with API design
                                result.Add("user_id", _dbHelper.SafeGetString(reader, 21));
                                result.Add("user_email", _dbHelper.SafeGetString(reader, 43));
                                // ***
                                result.Add("customer_id", _dbHelper.SafeGetString(reader, 22));
                                result.Add("create_user_id", _dbHelper.SafeGetString(reader, 23));
                                result.Add("edit_user_id", _dbHelper.SafeGetString(reader, 24));
                                result.Add("project_password", _dbHelper.SafeGetString(reader, 25));
                                result.Add("source_username", _dbHelper.SafeGetString(reader, 26));
                                result.Add("source_password", _dbHelper.SafeGetString(reader, 27));
                                result.Add("source_token", _dbHelper.SafeGetString(reader, 28));
                            }

                            reader.Close();

                            var sourceInfo = __getProjectSourceInfo(request.project_id);

                            result.Add("source_company_id", sourceInfo["source_company_id"]);
                            result.Add("source_company_name", sourceInfo["source_company_name"]);
                            result.Add("source_user_id", sourceInfo["source_user_id"]);
                            result.Add("source_contact_email", sourceInfo["source_contact_email"]);
                            result.Add("source_contact_phone", sourceInfo["source_contact_phone"]);
                            result.Add("contact_firstname", sourceInfo["contact_firstname"]);
                            result.Add("contact_lastname", sourceInfo["contact_lastname"]);
                            result.Add("contact_fullname", $"{sourceInfo["contact_firstname"]} {sourceInfo["contact_lastname"]}");


                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                status = "no matching project found!"
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
        [Route("GetPublishedLink")]
        public async System.Threading.Tasks.Task<IActionResult> GetAsync(ProjectGetLinkRequest request)
        {
            try
            {
                if (request.project_id == null)
                {
                    return BadRequest(new
                    {
                        status = "Please provide project id"
                    });
                }

                var destinationTypeId = "";
                var accessToken = "";
                var destinationPath = "";
                var sourceFileSubmissionEnabled = false;

                using (var cmd = _dbHelper.SpawnCommand())
                {
                    cmd.CommandText = $"SELECT setting_name, setting_value FROM project_settings WHERE project_id='{request.project_id}'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var settingName = _dbHelper.SafeGetString(reader, 0);
                            var settingValue = _dbHelper.SafeGetString(reader, 1);

                            if (settingName == "PROJECT_DESTINATION_PATH")
                            {
                                destinationPath = settingValue;
                            }
                            else if (settingName == "PROJECT_DESTINATION_TYPE_ID")
                            {
                                destinationTypeId = settingValue;
                            }
                            else if (settingName == "PROJECT_DESTINATION_TOKEN")
                            {
                                accessToken = settingValue;
                            }
                            else if (settingName == "PROJECT_SOURCE_FILE_SUBMISSION_FOLDER")
                            {
                                sourceFileSubmissionEnabled = settingValue == "enabled";
                            }
                        }

                        if (destinationTypeId == string.Empty)
                        {
                            return BadRequest(new
                            {
                                status = "Failed to locate project settings."
                            });
                        }

                        if (destinationTypeId != "dropbox")
                        {
                            return BadRequest(new
                            {
                                status = "Currently, We only support dropbox destinations"
                            });
                        }

                        if (accessToken == string.Empty)
                        {
                            return BadRequest(new
                            {
                                status = "Access token is not found."
                            });
                        }
                    }
                }

                if (!string.IsNullOrEmpty(request.submission_id)) {
                    var sourceCurrentFolderName = DocumentManagementController.__getRootFolderName("source_current");
                    var sourceHistoryFolderName = DocumentManagementController.__getRootFolderName("source_history");

                    if (sourceFileSubmissionEnabled)
                    {
                        using (var cmd = _dbHelper.SpawnCommand())
                        {
                            cmd.CommandText = $"SELECT submission_name FROM project_submissions WHERE project_submission_id='{request.submission_id}'";

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var submissionName = _dbHelper.SafeGetString(reader, 0);

                                    destinationPath += $"/{sourceHistoryFolderName}/{submissionName}";
                                }
                                else
                                {
                                    return BadRequest(new
                                    {
                                        status = "Cannot locate submission",
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        destinationPath += $"/{sourceCurrentFolderName}";
                    }
                }
                else if (!string.IsNullOrEmpty(request.folder_id))
                {
                    var folderNames = new List<string> { };
                    var folderId = request.folder_id;

                    while (!string.IsNullOrEmpty(folderId))
                    {
                        using (var cmd = _dbHelper.SpawnCommand())
                        {
                            cmd.CommandText = $"SELECT folder_name, parent_folder_id FROM project_folders WHERE folder_id='{folderId}'";

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    folderNames.Add(_dbHelper.SafeGetString(reader, 0));
                                    folderId = _dbHelper.SafeGetString(reader, 1);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    folderNames.Reverse();

                    if (folderNames.Count > 0)
                    {
                        destinationPath += $"/{String.Join('/', folderNames)}";
                    }
                }

                using (var dbx = new DropboxClient(accessToken))
                {
                    try
                    {
                        var link = await dbx.Sharing.ListSharedLinksAsync(destinationPath, null, true);
                        Dropbox.Api.Sharing.SharedLinkMetadata existingLink = null;

                        for (var index = 0; index < link.Links.Count; index++)
                        {
                            if (link.Links[index].PathLower == destinationPath.ToLower())
                            {
                                existingLink = link.Links[index];
                                break;
                            }
                        }

                        if (existingLink == null)
                        {
                            var result = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(destinationPath);
                            return Ok(new { url = result.Url });
                        }
                        else
                        {
                            string url = link.Links[0].Url;
                            return Ok(new { url });
                        }
                    }
                    catch (Exception)
                    {
                        return BadRequest(new
                        {
                            status = "This project is no longer available at the published destination"
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
        [Route("FindProjectSubmissions")]
        public IActionResult Get(ProjectSubmissionFindRequest request)
        {
            _dbHelper.CloseConnection();
            try
            {
                var detailLevel = request.detail_level ?? "basic";
                var submissionProcessStatus = request.submission_process_status ?? "all";

                using (var conn = _dbConnection)
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var whereString = " WHERE projects.status='active' AND ";

                        if (request.project_id != null)
                        {
                            whereString += $"project_submissions.project_id='{request.project_id}' AND ";
                        }

                        if (request.user_id != null)
                        {
                            whereString += $"projects.project_admin_user_id='{request.user_id}' AND ";
                        }

                        if (request.customer_id != null)
                        {
                            whereString += $"projects.project_customer_id='{request.customer_id}' AND ";
                        }

                        if (request.office_id != null)
                        {
                            whereString += $"projects.project_assigned_office_id='{request.office_id}' AND ";
                        }

                        if (submissionProcessStatus == "open")
                        {
                            whereString += "LOWER(project_submissions.submission_process_status)!='completed' AND LOWER(project_submissions.submission_process_status)!='deleted' AND ";
                        }
                        else if (submissionProcessStatus == "queued")
                        {
                            whereString += "(project_submissions.submission_process_status IS NULL OR LOWER(project_submissions.submission_process_status)='queued') AND ";
                        }
                        else if (submissionProcessStatus == "processing")
                        {
                            whereString += "LOWER(project_submissions.submission_process_status)='processing' AND ";
                        }
                        else if (submissionProcessStatus == "errored")
                        {
                            whereString += "(project_submissions.submission_process_status='Processing Paused' OR LOWER(project_submissions.submission_process_status)='errored') AND ";
                        }
                        else if (submissionProcessStatus == "completed")
                        {
                            whereString += "LOWER(project_submissions.submission_process_status)='completed' AND ";
                        }
                        else if (submissionProcessStatus == "deleted")
                        {
                            whereString += "LOWER(project_submissions.submission_process_status)='deleted' AND ";
                        }

                        whereString = whereString.Remove(whereString.Length - 5);

                        cmd.CommandText = "SELECT project_submissions.project_name, project_submissions.received_datetime, projects.source_sys_type_id, source_system_types.source_type_name, "
                          + "project_submissions.status, project_submissions.submitter_email, project_submissions.project_submission_id, project_submissions.submission_name, "
                          + "projects.project_admin_user_id, project_submissions.create_user_id, project_submissions.edit_user_id, project_submissions.submission_process_status, project_submissions.submission_process_message,  "
                          + "COUNT(project_documents.doc_id) AS submission_file_count, "
                          + "COUNT(project_documents.doc_id) FILTER (WHERE project_documents.process_status='queued' or project_documents.process_status='processing') AS submission_pending_file_count, "
                          + "COUNT(project_documents.doc_id) FILTER (WHERE project_documents.doc_type LIKE '%_single_sheet_plan') AS submission_plan_count, "
                          + "project_submissions.source_url, project_submissions.project_id, project_submissions.edit_datetime, "
                          + "project_submissions.submission_email_file_bucket, project_submissions.submission_email_file_key, project_submissions.submission_type, "
                                        + "customer_companies.company_name "
                                        + "FROM project_submissions "
                          + "LEFT JOIN projects on project_submissions.project_id=projects.project_id "
                          + "LEFT JOIN source_system_types on projects.source_sys_type_id=source_system_types.source_sys_type_id "
                                        + "LEFT JOIN customer_companies ON projects.source_company_id = customer_companies.company_id "
                          + "LEFT JOIN project_documents ON project_submissions.project_submission_id=project_documents.submission_id "
                          + whereString
                          + "GROUP BY project_submissions.project_submission_id, projects.source_sys_type_id, source_system_types.source_type_name, projects.project_admin_user_id, customer_companies.company_name "
                          + "ORDER BY project_submissions.create_datetime DESC LIMIT 1000";

                        var resultList = new List<Dictionary<string, string>>();

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var result = new Dictionary<string, string> { };

                                result.Add("project_name", _dbHelper.SafeGetString(reader, 0));
                                result.Add("received_datetime", _dbHelper.SafeGetDatetimeString(reader, 1));
                                result.Add("source_sys_name", _dbHelper.SafeGetString(reader, 3));
                                result.Add("submitter_email", _dbHelper.SafeGetString(reader, 5));
                                result.Add("submission_id", _dbHelper.SafeGetString(reader, 6));
                                result.Add("submission_name", _dbHelper.SafeGetString(reader, 7));
                                result.Add("submission_process_status", _dbHelper.SafeGetString(reader, 11));
                                result.Add("submission_process_message", _dbHelper.SafeGetString(reader, 12));
                                result.Add("submission_file_count", _dbHelper.SafeGetInteger(reader, 13));
                                result.Add("submission_pending_file_count", _dbHelper.SafeGetInteger(reader, 14));
                                result.Add("submission_plan_count", _dbHelper.SafeGetInteger(reader, 15));
                                result.Add("source_url", _dbHelper.SafeGetString(reader, 16));
                                result.Add("project_id", _dbHelper.SafeGetString(reader, 17));
                                result.Add("edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 18));
                                result.Add("submission_email_file_bucket", _dbHelper.SafeGetString(reader, 19));
                                result.Add("submission_email_file_key", _dbHelper.SafeGetString(reader, 20));
                                result.Add("submission_type", _dbHelper.SafeGetString(reader, 21));
                                result.Add("source_company_name", _dbHelper.SafeGetString(reader, 22));

                                if (detailLevel == "all" || detailLevel == "admin")
                                {
                                    result.Add("source_sys_type_id", _dbHelper.SafeGetString(reader, 2));
                                    result.Add("status", _dbHelper.SafeGetString(reader, 4));
                                    result.Add("user_id", _dbHelper.SafeGetString(reader, 8));
                                }

                                if (detailLevel == "admin")
                                {
                                    result.Add("create_user_id", _dbHelper.SafeGetString(reader, 9));
                                    result.Add("edit_user_id", _dbHelper.SafeGetString(reader, 10));
                                }

                                resultList.Add(result);
                            }
                            return Ok(resultList);
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
        [Route("DeleteSubmission")]
        public IActionResult Post(ProjectSubmissionDeleteRequest request)
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
                    cmd.CommandText = $"DELETE FROM project_submissions WHERE project_submission_id='{request.submission_id}'";
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


        private bool __createProjectEvent(string project_id, string project_name, string project_admin_user_id, string customer_id, string event_datetime, string event_name, string event_type)
        {
            var createEventResult = new CalendarEventManagementController().Post(new CalendarEvent
            {
                calendar_event_company_id = customer_id,
                calendar_event_name = $"{project_name} - {event_name}",
                calendar_event_organizer_company_id = customer_id,
                calendar_event_organizer_user_id = project_admin_user_id,
                calendar_event_start_datetime = event_datetime,
                calendar_event_status = "scheduled",
                calendar_event_type = event_type,
                project_id = project_id,
            });

            if (createEventResult is BadRequestObjectResult)
            {
                return false;
            }

            return true;
        }

        private string __lastChangeDate(string project_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT MAX(create_datetime) FROM project_documents "
                                                + $"WHERE project_id='{project_id}' "
                                                + "GROUP BY project_id";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return _dbHelper.SafeGetDatetimeString(reader, 0);
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        private Dictionary<string, string> __getProjectSourceInfo(string project_id)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT "
                  + "projects.source_company_id, customer_companies.company_name, "
                  + "projects.source_user_id, customer_contacts.contact_email, customer_contacts.contact_phone, customer_contacts.contact_firstname, customer_contacts.contact_lastname "
                  + "FROM projects "
                  + "LEFT JOIN customer_companies ON customer_companies.company_id = projects.source_company_id "
                  + "LEFT JOIN customer_contacts ON customer_contacts.contact_id = projects.source_company_contact_id "
                  + $"WHERE projects.project_id='{project_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Dictionary<string, string>
                        {
                            { "source_company_id", _dbHelper.SafeGetString(reader, 0) },
                            { "source_company_name", _dbHelper.SafeGetString(reader, 1) },
                            { "source_user_id", _dbHelper.SafeGetString(reader, 2) },
                            { "source_contact_email", _dbHelper.SafeGetString(reader, 3) },
                            { "source_contact_phone", _dbHelper.SafeGetString(reader, 4) },
                            { "contact_firstname", _dbHelper.SafeGetString(reader, 5) },
                            { "contact_lastname", _dbHelper.SafeGetString(reader, 6) },
                            { "contact_fullname", $"{_dbHelper.SafeGetString(reader, 5)} {_dbHelper.SafeGetString(reader, 6)}" },
                        };
                    }
                    else
                    {
                        return new Dictionary<string, string>
                        {
                            { "source_company_id", string.Empty },
                            { "source_company_name", string.Empty },
                            { "source_user_id", string.Empty },
                            { "source_contact_email", string.Empty },
                            { "source_contact_phone", string.Empty },
                            { "contact_firstname", string.Empty },
                            { "contact_lastname", string.Empty },
                            { "contact_fullname",string.Empty}
                        };
                    }
                }
            }
        }

        private string __getSourceFileSubmissionFolderSetting(string userId)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT setting_value FROM customer_settings "
                        + "LEFT JOIN customers ON customers.customer_id=customer_settings.customer_id "
                        + "LEFT JOIN users ON users.customer_id=customers.customer_id "
                        + $"WHERE setting_id='source_file_submission_folder' AND users.user_id='{userId}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var settingValue = _dbHelper.SafeGetString(reader, 0);

                        return settingValue;
                    }

                    return "disabled";
                }
            }
        }

        private bool IsEmailExist(string email)
        {
            using (var cmd = _dbHelper.SpawnCommand())
            {
                cmd.CommandText = $"select * from customer_companies where company_email={email}";
                return (bool)cmd.ExecuteScalar();
            }
        }
    }
}
