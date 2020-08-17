using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataViewFilterModel;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class DataViewFilterManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		private readonly IDataViewFilterManagementService _dataViewFilterManagementService;

		public DataViewFilterManagementController(
			IDataViewFilterManagementService dataViewFilterManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_dataViewFilterManagementService = dataViewFilterManagementService;
		}

		[HttpPost]
		[Route("CreateDataViewFilter")]
		public IActionResult CreateDataViewFilter(DataViewFilterModel newRecord)
		{
			try
			{
				var missingParameter = newRecord.CheckRequiredParameters(new string[]
				{
					"DataViewFilterName", "DataViewFilterSql", "DataViewId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}
				var newRecordId = _dataViewFilterManagementService.CreateDataViewFilter(_dbHelper, newRecord);
				return Ok(new
				{
					status = "completed",
					data_view_filter_id = newRecordId
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("FindDataViewFilters")]
		public IActionResult FindDataViewFilters(DataViewFilterFindRequestModel request)
		{
			try
			{
				var result = _dataViewFilterManagementService.FindDataViewFilters(_dbHelper, request);
				return Ok(result);
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("GetDataViewFilter")]
		public IActionResult GetDataViewFilter(DataViewFilterGetRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"DataViewFilterId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var result = _dataViewFilterManagementService.GetDataViewFilter(_dbHelper, request);
				return Ok(result);
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpPost]
		[Route("UpdateDataViewFilter")]
		public IActionResult UpdateDataViewFilter(DataViewFilterUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchDataViewFilterId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var result = _dataViewFilterManagementService.UpdateDataViewFilter(_dbHelper, request);
				return Ok(new
				{
					status = "completed",
					message = $"{result} rows are updated"
				});
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					status = "error",
					message = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}
	}
}
