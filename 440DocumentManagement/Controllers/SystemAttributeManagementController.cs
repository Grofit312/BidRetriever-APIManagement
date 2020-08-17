using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.SystemAttribute;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class SystemAttributeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		private readonly ISystemAttributeManagementService _systemAttributeManagementService;

		public SystemAttributeManagementController(
			ISystemAttributeManagementService systemAttributeManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_systemAttributeManagementService = systemAttributeManagementService;
		}

		[HttpPost]
		[Route("CreateSystemAttribute")]
		public IActionResult CreateSystemAttribute(SystemAttributeModel newRecord)
		{
			try
			{
				var missingParameter = newRecord.CheckRequiredParameters(new string[]
				{
					"SystemAttributeDatatype", "SystemAttributeName", "SystemAttributeSource"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var newRecordId = _systemAttributeManagementService.CreateSystemAttribute(_dbHelper, newRecord);
				return Ok(new
				{
					status = "completed",
					system_attribute_id = newRecordId
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
		[Route("FindSystemAttributes")]
		public IActionResult FindSystemAttributes(SystemAttributeFindRequestModel request)
		{
			try
			{
				var result = _systemAttributeManagementService.FindSystemAttributes(_dbHelper, request);
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
		[Route("InitializeSystemAttributes")]
		public IActionResult InitializeSystemAttributes()
		{
			try
			{
				_systemAttributeManagementService.InitializeSystemAttributes(_dbHelper);
				return Ok(new
				{
					status = "Completed"
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

		[HttpPost]
		[Route("UpdateSystemAttribute")]
		public IActionResult UpdateSystemAttribute(SystemAttributeUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchSystemAttributeId"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				int affectedRowCount = _systemAttributeManagementService.UpdateSystemAttribute(_dbHelper, request);
				return Ok(new
				{
					status = "completed",
					message = $"{affectedRowCount} rows are updated"
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
