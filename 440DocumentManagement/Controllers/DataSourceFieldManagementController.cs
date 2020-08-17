using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataSourceField;
using _440DocumentManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class DataSourceFieldManagementController : Controller
	{
		private DatabaseHelper _dbHelper;

		private IDataSourceFieldManagementService _dataSourceFieldManagementService;

		public DataSourceFieldManagementController(
			IDataSourceFieldManagementService dataSourceFieldManagementService)
		{
			_dbHelper = new DatabaseHelper();

			_dataSourceFieldManagementService = dataSourceFieldManagementService;
		}

		[HttpPost]
		[Route("CreateDataSourceField")]
		public IActionResult CreateDataSourceField(DataSourceFieldModel newRecord)
		{
			try
			{
				// Check Missing Parameters
				var missingParameter = newRecord.CheckRequiredParameters(new string[]
				{
					"CustomerAttributeId", "DataSourceFieldName", "DataSourceId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				// Check Data Source Field Name
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT format('%I', @data_source_field_name)";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@data_source_field_name", newRecord.DataSourceFieldName);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							if (Convert.ToString(reader["format"]) != newRecord.DataSourceFieldName)
							{
								return BadRequest(new
								{
									status = "error",
									message = "Data Source Field Name format is invalid"
								});
							}
						}
						else
						{
							return BadRequest(new
							{
								status = "error",
								message = "Data Source Field Name format is invalid"
							});
						}
					}
				}

				// Check Data Source Field Datatype
				var validDataTypes = new string[]
				{
					"bit", "bool", "box", "bytea", "char", "cidr", "circle", "date", "decimal", "float4", "float8", "inet", "int2",
					"int4", "int8", "interval", "json", "jsonb", "line", "lseg", "macaddr", "money", "numeric", "path", "point",
					"polygon", "serial2", "serial4", "serial8", "text", "time", "timestamp", "timestamptz", "timetz", "tsquery",
					"tsvector", "txid_snapshot", "uuid", "varbit", "varchar", "xml"
				};
				if (!string.IsNullOrEmpty(newRecord.DataSourceFieldDatatype)
					&& Array.FindIndex(validDataTypes, item => item == newRecord.DataSourceFieldDatatype) < 0)
				{
					return BadRequest(new
					{
						status = "error",
						message = "The Data Source Field DataType is not valid data type"
					});
				}

				// Check Customer Attribute Id
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT customer_attribute_id FROM customer_attributes WHERE customer_attribute_id=@customer_attribute_id";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@customer_attribute_id", newRecord.CustomerAttributeId);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							if (!reader.Read())
							{
								return BadRequest(new
								{
									status = "error",
									message = "Customer Attribute Id is invalid"
								});
							}
						}
						else
						{
							return BadRequest(new
							{
								status = "error",
								message = "Customer Attribute Id is invalid"
							});
						}
					}
				}

				// Check Duplication of Data Source Field Name on Same Data Source Id
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT data_source_field_id FROM data_source_fields WHERE data_source_id=@data_source_id AND data_source_field_name=@data_source_field_name";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@data_source_id", newRecord.DataSourceId);
					cmd.Parameters.AddWithValue("@data_source_field_name", newRecord.DataSourceFieldName);

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							if (reader.Read())
							{
								return BadRequest(new
								{
									status = "error",
									message = "The same data source field name is existed on that data source id"
								});
							}
						}
					}
				}

				var newRecordId = _dataSourceFieldManagementService.CreateDataSourceField(_dbHelper, newRecord);
				return Ok(new
				{
					status = "completed",
					data_source_field_id = newRecordId
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
		[Route("GetDataSourceField")]
		public IActionResult GetDataSourceField(DataSourceFieldGetRequestModel request)
		{
			try
			{
				var record = _dataSourceFieldManagementService.GetDataSourceField(_dbHelper, request);
				return Ok(record);
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
		[Route("UpdateDataSourceField")]
		public IActionResult UpdateDataSourceField(DataSourceFieldUpdateRequestModel request)
		{
			try
			{
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"SearchDataSourceFieldId"
				});
				if (!string.IsNullOrEmpty(missingParameter))
				{
					return BadRequest(new
					{
						status = "error",
						message = $"{missingParameter} is required"
					});
				}

				var affectedRowCount = _dataSourceFieldManagementService.UpdateDataSourceField(_dbHelper, request);
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
