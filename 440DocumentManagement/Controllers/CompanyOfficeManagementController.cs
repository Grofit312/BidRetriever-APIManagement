using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class CompanyOfficeManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public CompanyOfficeManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateCompanyOffice")]
		public IActionResult Post(CompanyOffice request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"customer_id",
					"company_office_name"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var companyOfficeId = request.company_office_id ?? Guid.NewGuid().ToString();

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check existence
					cmd.CommandText = "SELECT EXISTS (SELECT true FROM company_offices WHERE company_office_id='" + companyOfficeId + "')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new
						{
							status = "duplicated",
							company_office_id = companyOfficeId
						});
					}

					cmd.CommandText = "INSERT INTO company_offices "
							+ "(company_office_id, customer_id, company_office_name, company_office_address1, company_office_address2, "
							+ "company_office_city, company_office_state, company_office_zip, company_office_headoffice, company_office_country, "
							+ "company_office_admin_user_id, company_office_timezone, company_office_phone, company_office_service_area, status) "
							+ "VALUES(@company_office_id, @customer_id, @company_office_name, @company_office_address1, @company_office_address2, "
							+ "@company_office_city, @company_office_state, @company_office_zip, @company_office_headoffice, @company_office_country, "
							+ "@company_office_admin_user_id, @company_office_timezone, @company_office_phone, @company_office_service_area, @status)";

					cmd.Parameters.AddWithValue("company_office_id", companyOfficeId);
					cmd.Parameters.AddWithValue("customer_id", request.customer_id);
					cmd.Parameters.AddWithValue("company_office_name", request.company_office_name);
					cmd.Parameters.AddWithValue("company_office_address1", request.company_office_address1 ?? "");
					cmd.Parameters.AddWithValue("company_office_address2", request.company_office_address2 ?? "");
					cmd.Parameters.AddWithValue("company_office_city", request.company_office_city ?? "");
					cmd.Parameters.AddWithValue("company_office_state", request.company_office_state ?? "");
					cmd.Parameters.AddWithValue("company_office_zip", request.company_office_zip ?? "");
					cmd.Parameters.AddWithValue("company_office_headoffice", (object)request.company_office_headoffice ?? DBNull.Value);
					cmd.Parameters.AddWithValue("company_office_country", request.company_office_country ?? "");
					cmd.Parameters.AddWithValue("company_office_admin_user_id", request.company_office_admin_user_id ?? "");
					cmd.Parameters.AddWithValue("company_office_timezone", request.company_office_timezone ?? "");
					cmd.Parameters.AddWithValue("company_office_phone", request.company_office_phone ?? "");
					cmd.Parameters.AddWithValue("company_office_service_area", request.company_office_service_area ?? "");
					cmd.Parameters.AddWithValue("status", request.status ?? "active");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						company_office_id = companyOfficeId,
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
		[Route("UpdateCompanyOffice")]
		public IActionResult Post(CompanyOfficeUpdateRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.search_company_office_id))
				{
					return BadRequest(new
					{
						status = "Please provide search_company_office_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE company_offices SET "
							+ "company_office_address1 = COALESCE(@company_office_address1, company_office_address1), "
							+ "company_office_address2 = COALESCE(@company_office_address2, company_office_address2), "
							+ "company_office_admin_user_id = COALESCE(@company_office_admin_user_id, company_office_admin_user_id), "
							+ "company_office_city = COALESCE(@company_office_city, company_office_city), "
							+ "company_office_country = COALESCE(@company_office_country, company_office_country), "
							+ "company_office_headoffice = COALESCE(@company_office_headoffice, company_office_headoffice), "
							+ "company_office_name = COALESCE(@company_office_name, company_office_name), "
							+ "company_office_phone = COALESCE(@company_office_phone, company_office_phone), "
							+ "company_office_service_area = COALESCE(@company_office_service_area, company_office_service_area), "
							+ "company_office_state = COALESCE(@company_office_state, company_office_state), "
							+ "company_office_timezone = COALESCE(@company_office_timezone, company_office_timezone), "
							+ "company_office_zip = COALESCE(@company_office_zip, company_office_zip), "
							+ "customer_id = COALESCE(@customer_id, customer_id), "
							+ "status = COALESCE(@status, status) "
							+ "WHERE company_office_id='" + request.search_company_office_id + "'";

					cmd.Parameters.AddWithValue(
						"company_office_address1",
						(object)request.company_office_address1 ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_address2",
						(object)request.company_office_address2 ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_admin_user_id",
						(object)request.company_office_admin_user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_city",
						(object)request.company_office_city ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_country",
						(object)request.company_office_country ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_headoffice",
						(object)request.company_office_headoffice ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_name",
						(object)request.company_office_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_phone",
						(object)request.company_office_phone ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_service_area",
						(object)request.company_office_service_area ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_state",
						(object)request.company_office_state ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_timezone",
						(object)request.company_office_timezone ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"company_office_zip",
						(object)request.company_office_zip ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"customer_id",
						(object)request.customer_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
						"status",
						(object)request.status ?? DBNull.Value);

					var affectedRows = cmd.ExecuteNonQuery();

					if (affectedRows == 0)
					{
						return BadRequest(new
						{
							status = "Failed to find company office to update"
						});
					}

					if (request.company_office_headoffice)
					{
						var customerId = "";

						cmd.CommandText = "SELECT customer_id FROM company_offices WHERE company_office_id='" + request.search_company_office_id + "'";

						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								customerId = _dbHelper.SafeGetString(reader, 0);
							}
						}

						cmd.CommandText = "UPDATE customers SET "
								+ "customer_phone = COALESCE(@customer_phone, customer_phone), "
								+ "customer_address1 = COALESCE(@customer_address1, customer_address1), "
								+ "customer_address2 = COALESCE(@customer_address2, customer_address2), "
								+ "customer_city = COALESCE(@customer_city, customer_city), "
								+ "customer_state = COALESCE(@customer_state, customer_state), "
								+ "customer_zip = COALESCE(@customer_zip, customer_zip), "
								+ "customer_country = COALESCE(@customer_country, customer_country), "
								+ "customer_timezone = COALESCE(@customer_timezone, customer_timezone), "
								+ "edit_datetime = @edit_datetime "
								+ "WHERE customer_id='" + customerId + "'";

						cmd.Parameters.AddWithValue(
							"customer_phone",
							(object)request.company_office_phone ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_address1",
							(object)request.company_office_address1 ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_address2",
							(object)request.company_office_address2 ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_city",
							(object)request.company_office_city ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_state",
							(object)request.company_office_state ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_zip",
							(object)request.company_office_zip ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_country",
							(object)request.company_office_country ?? DBNull.Value);
						cmd.Parameters.AddWithValue(
							"customer_timezone",
							(object)request.company_office_timezone ?? DBNull.Value);
						cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

						cmd.ExecuteNonQuery();
					}

					if (!string.IsNullOrEmpty(request.company_office_name))
					{
						cmd.CommandText = "UPDATE projects SET project_assigned_office_name='" + request.company_office_name + "' "
								+ "WHERE project_assigned_office_id='" + request.search_company_office_id + "'";
						cmd.ExecuteNonQuery();
					}

					return Ok(new
					{
						status = "updated"
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

		[HttpGet]
		[Route("GetCompanyOffice")]
		public IActionResult Get(CompanyOfficeGetRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.company_office_id))
				{
					return BadRequest(new { status = "Please provide company_office_id" });
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT company_offices.customer_id, company_offices.status, company_office_name, company_office_city, company_office_state, "
							+ "company_office_address1, company_office_address2, company_office_country, company_office_phone, company_office_service_area, "
							+ "company_office_timezone, company_office_zip, "
							+ "company_office_admin_user_id, company_office_headoffice, users.user_firstname, users.user_lastname "
							+ "FROM company_offices LEFT JOIN users ON users.user_id=company_offices.company_office_admin_user_id WHERE company_office_id='" + request.company_office_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var result = new Dictionary<string, object>
							{
								{ "customer_id", _dbHelper.SafeGetString(reader, 0) },
								{ "status", _dbHelper.SafeGetString(reader, 1) },
								{ "company_office_name", _dbHelper.SafeGetString(reader, 2) },
								{ "company_office_city", _dbHelper.SafeGetString(reader, 3) },
								{ "company_office_state", _dbHelper.SafeGetString(reader, 4) },
							};

							if (request.detail_level == "all" || request.detail_level == "admin")
							{
								result.Add("company_office_address1", _dbHelper.SafeGetString(reader, 5));
								result.Add("company_office_address2", _dbHelper.SafeGetString(reader, 6));
								result.Add("company_office_country", _dbHelper.SafeGetString(reader, 7));
								result.Add("company_office_phone", _dbHelper.SafeGetString(reader, 8));
								result.Add("company_office_service_area", _dbHelper.SafeGetString(reader, 9));
								result.Add("company_office_timezone", _dbHelper.SafeGetString(reader, 10));
								result.Add("company_office_zip", _dbHelper.SafeGetString(reader, 11));
								result.Add(
									"company_office_admin_display_name",
									$"{_dbHelper.SafeGetString(reader, 15)}, {_dbHelper.SafeGetString(reader, 14)}".Trim().TrimStart(',').TrimEnd(','));
							}

							if (request.detail_level == "admin")
							{
								result.Add("company_office_admin_user_id", _dbHelper.SafeGetString(reader, 12));
								result.Add("company_office_headoffice", _dbHelper.SafeGetBooleanRaw(reader, 13));
							}
							return Ok(result);
						}
						else
						{
							return BadRequest(new
							{
								status = "Cannot find company office!"
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
		[Route("FindCompanyOffices")]
		public IActionResult Get(CompanyOfficeFindRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.customer_id))
				{
					return BadRequest(new
					{
						status = "Please provide customer_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT company_offices.customer_id, company_offices.status, company_office_name, company_office_city, company_office_state, "
							+ "company_office_address1, company_office_address2, company_office_country, company_office_phone, company_office_service_area, "
							+ "company_office_timezone, company_office_zip, "
							+ "company_office_admin_user_id, company_office_id, company_office_headoffice, user_firstname, user_lastname "
							+ "FROM company_offices LEFT JOIN users ON users.user_id=company_offices.company_office_admin_user_id WHERE company_offices.customer_id='" + request.customer_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, object>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								{ "customer_id", _dbHelper.SafeGetString(reader, 0) },
								{ "status", _dbHelper.SafeGetString(reader, 1) },
								{ "company_office_name", _dbHelper.SafeGetString(reader, 2) },
								{ "company_office_city", _dbHelper.SafeGetString(reader, 3) },
								{ "company_office_state", _dbHelper.SafeGetString(reader, 4) },
								{ "company_office_address1", _dbHelper.SafeGetString(reader, 5) },
								{ "company_office_address2", _dbHelper.SafeGetString(reader, 6) },
								{ "company_office_country", _dbHelper.SafeGetString(reader, 7) },
								{ "company_office_phone", _dbHelper.SafeGetString(reader, 8) },
								{ "company_office_service_area", _dbHelper.SafeGetString(reader, 9) },
								{ "company_office_timezone", _dbHelper.SafeGetString(reader, 10) },
								{ "company_office_zip", _dbHelper.SafeGetString(reader, 11) },
								{ "company_office_admin_user_id", _dbHelper.SafeGetString(reader, 12) },
								{ "company_office_id", _dbHelper.SafeGetString(reader, 13) },
								{ "company_office_headoffice", _dbHelper.SafeGetBooleanRaw(reader, 14) },
								{ "company_office_admin_display_name", $"{_dbHelper.SafeGetString(reader, 16)}, {_dbHelper.SafeGetString(reader, 15)}".Trim().TrimStart(',').TrimEnd(',') }
							});
						}
						return Ok(result);
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
	}
}
