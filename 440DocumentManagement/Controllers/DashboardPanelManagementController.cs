using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Models.DashboardPanel;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class DashboardPanelManagementController : Controller
	{
		private readonly IDashboardPanelManagementService _dashboardPanelManagementService;

		public DashboardPanelManagementController(
			IDashboardPanelManagementService dashboardPanelManagementService)
		{
			_dashboardPanelManagementService = dashboardPanelManagementService;
		}

		[HttpPost]
		[Route("CreateDashboardPanel")]
		[OpenApiOperation("Create a new dashboard panel record", "Create a new dashboard panel record")]
		[ProducesResponseType(typeof(DashboardPanelCreateResponseModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult CreateDashboardPanel(DashboardPanelModel newRecord)
		{
			try
			{
				if (newRecord == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Can't insert null value"
					});
				}

				// Verify the Required Fields
				var missingParameter = newRecord.CheckRequiredParameters(new string[]
				{
					"DashboardId", "PanelAnalyticDatasource", "PanelName"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var newRecordId = _dashboardPanelManagementService.CreateRecord(newRecord);
				return Ok(new DashboardPanelCreateResponseModel
				{
					Status = Constants.ApiStatus.SUCCESS,
					PanelId = newRecordId
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
		}

		[HttpGet]
		[Route("FindDashboardPanels")]
		[OpenApiOperation("Search for dashboard_panel records", "Search for dashboard_panel records")]
		[ProducesResponseType(typeof(List<DashboardPanelModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult FindDashboardPanels(DashboardPanelFindRequestModel request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Request can't be null."
					});
				}

				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[] { });
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var records = _dashboardPanelManagementService.FindRecords(request);
				return Ok(records);
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
		}

		[HttpGet]
		[Route("GetDashboardPanel")]
		[OpenApiOperation("Gets the specified dashboard_panel", "Gets the specified dashboard_panel")]
		[ProducesResponseType(typeof(DashboardPanelModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult GetDashboardPanel(DashboardPanelGetRequestModel request)
		{
			try
			{
				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[] { "PanelId" });
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var record = _dashboardPanelManagementService.GetRecord(request);
				if (record == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"The dashboard panel record with id ({request.PanelId}) is not existed."
					});
				}
				
				return Ok(record);
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
		}

		[HttpPost]
		[Route("UpdateDashboardPanel")]
		[OpenApiOperation(
			"This API call allows an application to update an existing dashboard_panel record",
			"This API call allows an application to update an existing dashboard_panel record"
		)]
		[ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult UpdateDashboardPanel(DashboardPanelUpdateRequestModel request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Request Can't contains null"
					});
				}

				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchPanelId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseResponseModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				int affectedRowCount = _dashboardPanelManagementService.UpdateRecords(request);
				if (affectedRowCount == 0)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"No matching record found for panel_id = {request.SearchPanelId}"
					});
				}
				
				return Ok(new BaseResponseModel
				{
					Status = Constants.ApiStatus.SUCCESS,
					Message = $"{affectedRowCount} records are updated successfully."
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new BaseErrorModel
				{
					Status = Constants.ApiStatus.ERROR,
					Message = ex.Message
				});
			}
		}
	}
}
