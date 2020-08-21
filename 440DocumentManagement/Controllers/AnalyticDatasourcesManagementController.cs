using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using _440DocumentManagement.Models.AnalyticDatasources;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Threading.Tasks;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api/AnalyticDatasources")]
	[OpenApiTag("Analytic Datasources Management")]
	public class AnalyticDatasourcesManagementController : Controller
	{
		private readonly IAnalyticDatasourcesManagementService _analyticDatasourcesManagementService;

		public AnalyticDatasourcesManagementController(
			IAnalyticDatasourcesManagementService analyticDatasourcesManagementService)
		{
			_analyticDatasourcesManagementService = analyticDatasourcesManagementService;
		}

		[HttpPost]
		[Route("CreateAnalyticDatasource")]
		[OpenApiOperation("Create a new Analytic Datasource record.", "Create a new Analytic Datasource record.")]
		[ProducesResponseType(typeof(AnalyticDatasourcesCreateResponseModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult CreateAnalyticDatasource(AnalyticDatasourcesModel newRecord)
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
					"AnalyticDatasourceType"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var newRecordId = _analyticDatasourcesManagementService.CreateRecord(newRecord);
				return Ok(new AnalyticDatasourcesCreateResponseModel
				{
					Status = Constants.ApiStatus.SUCCESS,
					AnalyticDatasourceId = newRecordId
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
		[Route("ExecuteAnalyticDatasource")]
		[OpenApiOperation(
			"Executes the Analytic Datasource.",
			"Executes the analytic_datasource specified and returns all information necessary to display the chart or graph in the user interface."
		)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> ExecuteAnalyticDatasource(AnalyticDatasourcesExecuteRequestModel request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = "Request can't be null"
					});
				}

				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"AnalyticDatasourceId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var analyticData = await _analyticDatasourcesManagementService.ExecuteRecord(request);
				return Ok(analyticData);
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
		[Route("FindAnalyticDatasources")]
		[OpenApiOperation(
			"Finds analytic datasources records from table",
			"<p>Returns a list of All analytic datasources available for the customer and returns all information about the datasource.</p>"
			+ "<p>This routine is used by applications that allow a user to select from all predefind.</p>"
			+ "<p>It provides them with a list of the available datasources.</p>"
			+ "<p style='color: red'>Note: default analytic_datasources will have a customer_id = 'default', so all queries should return the default analytic_datasources.</p>"
		)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status200OK)]
		public IActionResult FindAnalyticDatasources(AnalyticDatasourcesFindRequestModel request)
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
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var records = _analyticDatasourcesManagementService.FindRecords(request);
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
		[Route("GetAnalyticDatasource")]
		[OpenApiOperation(
			"Get analytic datasources record",
			"Locates the analytic_datasource specified and returns all information about the analytic_datasource including all defined fields."
		)]
		[ProducesResponseType(typeof(AnalyticDatasourcesModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult GetAnalyticDatasource(AnalyticDatasourcesGetRequestModel request)
		{
			try
			{
				// Verify the Required Fields
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"AnalyticDatasourceId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				var record = _analyticDatasourcesManagementService.GetRecord(request);
				if (record == null)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"The analytic datasources record with id ({request.AnalyticDatasourceId}) is not existed."
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

		[HttpGet]
		[Route("UpdateAnalyticDatasource")]
		[OpenApiOperation(
			"Update the analytic datasources record on database table",
			"This API call allows an application to update an existing Analytic Datasource."
		)]
		[ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseErrorModel), StatusCodes.Status400BadRequest)]
		public IActionResult UpdateAnalyticDatasource(AnalyticDatasourcesUpdateRequestModel request)
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
					"SearchAnalyticDatasourceId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new BaseResponseModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"{missingParameter} is required"
					});
				}

				int affectedRowCount = _analyticDatasourcesManagementService.UpdateRecords(request);
				if (affectedRowCount == 0)
				{
					return BadRequest(new BaseErrorModel
					{
						Status = Constants.ApiStatus.ERROR,
						Message = $"No matching record found for analytic_datasource_id = {request.SearchAnalyticDatasourceId}"
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
