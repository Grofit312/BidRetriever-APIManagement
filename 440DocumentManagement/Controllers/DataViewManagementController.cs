using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataView;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class DataViewManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		private readonly IDataViewManagementService _dataViewManagementService;

		public DataViewManagementController(
			IDataViewManagementService dataViewManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_dataViewManagementService = dataViewManagementService;
		}

		[HttpPost]
		[Route("CreateDataView")]
		public IActionResult CreateDataView(DataViewModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"DataFilterId", "DataSourceId", "ViewName"
				});
				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				string newViewId = _dataViewManagementService.CreateDataView(_dbHelper, request);
				return Ok(new
				{
					status = "completed",
					view_id = newViewId
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
		[Route("FindDataViews")]
		public IActionResult FindDataViews(DataViewFindRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"ViewType"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var result = _dataViewManagementService.FindDataViews(_dbHelper, request);
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
		[Route("GetDataView")]
		public IActionResult GetDataView(DataViewGetRequestModel request)
		{
			try
			{
				var missingParamter = request.CheckRequiredParameters(new string[]
				{
					"ViewId"
				});
				if (!string.IsNullOrEmpty(missingParamter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParamter} is required"
					});
				}

				var result = _dataViewManagementService.GetDataView(_dbHelper, request);
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
		[Route("UpdateDataView")]
		public IActionResult UpdateDataView(DataViewUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchViewId"
				});
				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var affectedRowCount = _dataViewManagementService.UpdateDataView(_dbHelper, request);
				if (affectedRowCount == 0)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"No matching record found for view_id = {request.SearchViewId}"
					});
				}
				else
				{
					return Ok(new
					{
						status = "completed",
						message = $"{affectedRowCount} rows are updated"
					});
				}
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
