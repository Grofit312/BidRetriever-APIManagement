using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataSource;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Data Source Management")]
	public class DataSourceManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		private readonly IDataSourceManagementService _dataSourceManagementService;

		public DataSourceManagementController(
			IDataSourceManagementService dataSourceManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_dataSourceManagementService = dataSourceManagementService;
		}

		[HttpPost]
		[Route("CreateDataSource")]
		public IActionResult CreateDataSource(DataSourceModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"DataSourceName", "DataSourceBaseQuery"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var newDataSourceId = _dataSourceManagementService.CreateDataSource(_dbHelper, request);
				return Ok(new
				{
					status = "completed",
					data_source_id = newDataSourceId
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
		[Route("FindDataSources")]
		public IActionResult FindDataSources(DataSourceFindRequestModel request)
		{
			try
			{
				var resultList = _dataSourceManagementService.FindDataSources(_dbHelper, request);
				return Ok(resultList);
			}
			catch (Exception ex)
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
		[Route("GetDataSource")]
		public IActionResult GetDataSource(DataSourceGetRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"DataSourceId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var result = _dataSourceManagementService.GetDataSource(_dbHelper, request);
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
		[Route("UpdateDataSource")]
		public IActionResult UpdateDataSource(DataSourceUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchDataSourceId"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				int affectedRowCount = _dataSourceManagementService.UpdateDataSource(_dbHelper, request);
				if (affectedRowCount == 0)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"No matching record found for data_source_id = {request.SearchDataSourceId}"
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
