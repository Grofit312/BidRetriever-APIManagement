using System;
using System.Linq;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.CustomerAttribute;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Customer Attribute Management")]
	public class CustomerAttributeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		private readonly ICustomerAttributeManagementService _customerAttributeManagementService;

		public CustomerAttributeManagementController(
			ICustomerAttributeManagementService customerAttributeManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_customerAttributeManagementService = customerAttributeManagementService;
		}

		[HttpPost]
		[Route("CreateCustomerAttribute")]
		public IActionResult CreateCustomerAttribute(CustomerAttributeModel request)
		{
			try
			{
				// Check Missing Parameters
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"CustomerAttributeDatatype", "CustomerAttributeName", "CustomerAttributeSource"
				});
				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				// Check System Attribute Id
				if (request.CustomerAttributeDatatype != "jsonb" && string.IsNullOrEmpty(request.SystemAttributeId))
				{
					return BadRequest(new
					{
						status = "error",
						message = "System Attribute Id is required on that data type"
					});
				}
				if (request.CustomerAttributeDatatype != "jsonb" && !string.IsNullOrEmpty(request.SystemAttributeId))
				{
					// Check SystemAttributeId Validation
					using (var cmd = _dbHelper.SpawnCommand())
					{
						cmd.CommandText = "SELECT * from system_attributes WHERE system_attribute_id = @system_attribute_id";
						cmd.Parameters.AddWithValue("@system_attribute_id", request.SystemAttributeId);
						using (var reader = cmd.ExecuteReader())
						{
							if (!reader.HasRows)
							{
								return BadRequest(new
								{
									status = "error",
									message = "SystemAttributeId is not existed on the database. Please input valid one"
								});
							}
						}
					}
				}

				// Check Customer Attribute Name
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT format('%I', @customer_attribute_name)";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@customer_attribute_name", request.CustomerAttributeName);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							if (Convert.ToString(reader["format"]) != request.CustomerAttributeName)
							{
								return BadRequest(new
								{
									status = "error",
									message = "Customer Attribute Name format is invalid"
								});
							}
						}
						else
						{
							return BadRequest(new
							{
								status = "error",
								message = "Customer Attribute Name format is invalid"
							});
						}
					}
				}

				// Check Customer Attribute Datatype
				var validDataTypes = new string[]
				{
					"bit", "bool", "box", "bytea", "char", "cidr", "circle", "date", "decimal", "float4", "float8", "inet", "int2",
					"int4", "int8", "interval", "json", "jsonb", "line", "lseg", "macaddr", "money", "numeric", "path", "point",
					"polygon", "serial2", "serial4", "serial8", "text", "time", "timestamp", "timestamptz", "timetz", "tsquery",
					"tsvector", "txid_snapshot", "uuid", "varbit", "varchar", "xml"
				};
				if (Array.FindIndex(validDataTypes, item => item == request.CustomerAttributeDatatype) < 0)
				{
					return BadRequest(new
					{
						status = "error",
						message = "The Customer Attribute DataType is not valid data type"
					});
				}

				// Check Customer Attribute Source
				if (!string.IsNullOrEmpty(request.CustomerAttributeSource))
				{
					using (var cmd = _dbHelper.SpawnCommand())
					{
						cmd.CommandText = $"SELECT to_regclass(@customer_attribute_source) IS NOT NULL as source_validation";
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@customer_attribute_source", "public." + request.CustomerAttributeSource);
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								if (Convert.ToBoolean(reader["source_validation"]) == false)
								{
									return BadRequest(new
									{
										status = "error",
										message = "Customer Attribute Source is invalid"
									});
								}
							}
							else
							{
								return BadRequest(new
								{
									status = "error",
									message = "Customer attribute source is invalid"
								});
							}
						}
					}
				}

				// Check Default Alignment
				if (!string.IsNullOrEmpty(request.DefaultAlignment))
				{
					if (request.DefaultAlignment != "left"
						&& request.DefaultAlignment != "right"
						&& request.DefaultAlignment != "center")
					{
						return BadRequest(new
						{
							status = "error",
							message = "Default alignment is invalid"
						});
					}
				}

				var newRecordId = _customerAttributeManagementService.CreateCustomerAttribute(_dbHelper, request);
				return Ok(new
				{
					status = "completed",
					customer_attribute_id = newRecordId
				});
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
		[Route("FindCustomerAttributes")]
		public IActionResult FindCustomerAttributes(CustomerAttributeFindRequestModel request)
		{
			try
			{
				var result = _customerAttributeManagementService.FindCustomerAttributes(_dbHelper, request);
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
		[Route("UpdateCustomerAttribute")]
		public IActionResult UpdateCustomerAttribute(CustomerAttributeUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchCustomerAttributeId"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var affectedRowCount = _customerAttributeManagementService.UpdateCustomerAttribute(_dbHelper, request);
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
