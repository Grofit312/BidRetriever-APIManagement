using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ProjectRelationship;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
    [Produces("application/json")]
    [Route("api")]
		[OpenApiTag("Project Relationship Management")]
    public class ProjectRelationshipManagementController : Controller
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly IProjectRelationshipService projectRelationshipService;

        public ProjectRelationshipManagementController(
            IProjectRelationshipService projectRelationshipService)
        {
            this.projectRelationshipService = projectRelationshipService;
            _dbHelper = new DatabaseHelper();
        }
        [HttpPost]
        [Route("CreateProjectRelationship")]
        public IActionResult CreateProjectRelationship(ProjectRelationship request)
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

                // Verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[]
                {
                    "project_id",
                });
                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                string projectrelationshipid = projectRelationshipService.CreateProjectRelationship(_dbHelper, request);
                return Ok(new
                {
                    status = "Success",
                    view_id = projectrelationshipid
                });
            }
            catch (ApiException ex)
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
        [Route("FindProjectRelationships")]
        public IActionResult FindProjectRelationships(ProjectRelationshipCreteria request)
        {
            try
            {
                if (request == null || (request.company_id == null && request.contact_id == null && request.project_id == null))
                {
                    return BadRequest(new
                    {
                        status = "At least one of the parameters must be provided."
                    });
                }
                var result = projectRelationshipService.FindProjectRelationships(_dbHelper, request);
                return Ok(result);
            }
            catch (ApiException ex)
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
        [Route("GetProjectRelationship")]
        public IActionResult GetProjectRelationship(string project_relationship_id)
        {
            try
            {
                if (string.IsNullOrEmpty(project_relationship_id))
                {
                    return BadRequest(new
                    {
                        status = "project_relationship_id is missing."
                    });
                }

                var result = projectRelationshipService.GetProjectRelationship(_dbHelper, project_relationship_id);
                return Ok(result);
            }
            catch (ApiException ex)
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
        [Route("UpdateProjectRelationship")]
        public IActionResult UpdateProjectRelationship(ProjectRelationship request)
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

                // Verify required fields
                var missingParameter = request.CheckRequiredParameters(new string[]
                {
                    "project_id", "project_relationship_id"
                });
                if (missingParameter != null)
                {
                    return BadRequest(new
                    {
                        status = $"{missingParameter} is required"
                    });
                }

                var affectedRowCount = projectRelationshipService.UpdateDataView(_dbHelper, request);
                if (affectedRowCount == 0)
                {
                    return BadRequest(new
                    {
                        status = $"No matching record found for project_relationship_id = {request.project_relationship_id}"
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
            catch (ApiException ex)
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