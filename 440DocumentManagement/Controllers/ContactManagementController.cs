using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ContactManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class ContactManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		public ContactManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateContact")]
		public IActionResult CreateContact(Contact request)
		{
			try
			{
				if (request != null)
				{
					DateTime _date = DateTime.UtcNow;
					request.company_id = (request.company_id == null || string.IsNullOrEmpty(request.company_id)) ? Guid.NewGuid().ToString() : request.company_id;
					request.company_name = (request.company_name == null || string.IsNullOrEmpty(request.company_name)) ? Guid.NewGuid().ToString() : request.company_name;
					request.company_office_name = (request.company_office_name == null || string.IsNullOrEmpty(request.company_office_name)) ? Guid.NewGuid().ToString() : request.company_office_name;
					request.contact_id = (request.contact_id == null || string.IsNullOrEmpty(request.contact_id)) ? Guid.NewGuid().ToString() : request.contact_id;
					request.contact_role = (request.contact_role == null || string.IsNullOrEmpty(request.contact_role)) ? "contact" : request.contact_role;
					request.contact_status = (request.contact_status == null || string.IsNullOrEmpty(request.contact_status)) ? "active" : request.contact_status;
					string created_user_id = Guid.NewGuid().ToString();
					//verify required fields
					var missingParameter = request.CheckRequiredParameters(new string[] { "contact_email", "customer_id" });


					if (missingParameter != null)
					{
						return BadRequest(new
						{
							status = missingParameter + " is required"
						});
					}
					if (!_IsExists(request.customer_id))
					{
						return BadRequest(new
						{
							status = "Please enter valid customer_id."
						});
					}

					string duplicatedContactId = _getContactIdWithEmail(request.contact_email);
					if (!string.IsNullOrEmpty(duplicatedContactId))
					{
						UpdateContact(new ContactFilter
						{
							search_contact_id = duplicatedContactId,
							contact_email = request.contact_email,
							contact_firstname = request.contact_firstname,
							contact_lastname = request.contact_lastname,
							contact_phone = request.contact_phone,
							contact_address1 = request.contact_address1,
							contact_address2 = request.contact_address2,
							contact_city = request.contact_city,
							contact_state = request.contact_state,
							contact_zip = request.contact_zip,
							contact_country = request.contact_country,
							contact_crm_id = request.contact_crm_id,
							contact_photo_id = request.contact_photo_id,
							customer_id = request.customer_id,
							contact_status = request.contact_state
						}, true);

						return Ok(new
						{
							status = "Success",
							contact_id = duplicatedContactId
						});
					}

					using (var cmd = _dbHelper.SpawnCommand())
					{
						string query = @"INSERT INTO public.customer_contacts(
	company_id, company_name, company_office_id, company_office_name, contact_address1, contact_address2, contact_city, contact_country, contact_crm_id, contact_email, contact_firstname, contact_id, contact_lastname, contact_mobile_phone, contact_password, contact_phone, contact_photo_id, contact_record_source, contact_role, contact_state, contact_status, contact_title, contact_username, contact_verification_datetime, contact_verification_level, contact_zip, create_datetime, create_user_id, customer_id, edit_datetime, edit_user_id)
	VALUES (@company_id, @company_name, @company_office_id, @company_office_name, @contact_address1, @contact_address2, @contact_city, @contact_country, @contact_crm_id, @contact_email, @contact_firstname, @contact_id, @contact_lastname, @contact_mobile_phone, @contact_password, @contact_phone, @contact_photo_id, @contact_record_source, @contact_role, @contact_state, @contact_status, @contact_title, @contact_username, @contact_verification_datetime, @contact_verification_level, @contact_zip, @create_datetime, @create_user_id, @customer_id, @edit_datetime, @edit_user_id);";
						cmd.CommandText = query;
						cmd.Parameters.AddWithValue("@company_id", request.company_id);
						cmd.Parameters.AddWithValue("@company_name", request.company_name);
						cmd.Parameters.AddWithValue("@company_office_id", (object)request.company_office_id ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@company_office_name", request.company_office_name);
						cmd.Parameters.AddWithValue("@contact_address1", (object)request.contact_address1 ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_address2", (object)request.contact_address2 ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_city", (object)request.contact_city ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_country", (object)request.contact_country ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_crm_id", (object)request.contact_crm_id ?? DBNull.Value);
						//cmd.Parameters.AddWithValue("@contact_display_name", request.contact_display_name);
						cmd.Parameters.AddWithValue("@contact_email", request.contact_email);
						cmd.Parameters.AddWithValue("@contact_id", request.contact_id);
						cmd.Parameters.AddWithValue("@contact_firstname", (object)request.contact_firstname ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_lastname", (object)request.contact_lastname ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_mobile_phone", (object)request.contact_mobile_phone ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_password", (object)request.contact_password ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_phone", (object)request.contact_phone ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_photo_id", (object)request.contact_photo_id ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_record_source", (object)request.contact_record_source ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_role", request.contact_role);
						cmd.Parameters.AddWithValue("@contact_state", (object)request.contact_state ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_status", request.contact_status);
						cmd.Parameters.AddWithValue("@contact_title", (object)request.contact_title ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_username", (object)request.contact_username ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_verification_datetime", DateTime.UtcNow);
						cmd.Parameters.AddWithValue("@contact_verification_level", (object)request.contact_verification_level ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@contact_zip", (object)request.contact_zip ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@create_datetime", _date);
						cmd.Parameters.AddWithValue("@create_user_id", created_user_id);
						cmd.Parameters.AddWithValue("@customer_id", (object)request.customer_id ?? DBNull.Value);
						cmd.Parameters.AddWithValue("@edit_datetime", _date);
						cmd.Parameters.AddWithValue("@edit_user_id", created_user_id);
						cmd.ExecuteNonQuery();
					}
					if (!string.IsNullOrEmpty(request.user_id))
					{
						using (var cmd = _dbHelper.SpawnCommand())
						{
							cmd.CommandText = "INSERT INTO public.user_contacts VALUES (@create_datetime, @user_id, @customer_contact_id, @user_contact_id);";
							cmd.Parameters.AddWithValue("@create_datetime", _date);
							cmd.Parameters.AddWithValue("@user_id", request.user_id);
							cmd.Parameters.AddWithValue("@customer_contact_id", request.contact_id);
							cmd.Parameters.AddWithValue("@user_contact_id", request.contact_id);
							cmd.ExecuteNonQuery();
						}
					}
					if (!string.IsNullOrEmpty(request.customer_office_id))
					{
						using (var cmd = _dbHelper.SpawnCommand())
						{
							cmd.CommandText = "INSERT INTO public.customer_office_contacts VALUES (@create_datetime, @customer_office_id, @customer_contact_id, @office_contact_id);";
							cmd.Parameters.AddWithValue("@create_datetime", _date);
							cmd.Parameters.AddWithValue("@customer_office_id", request.customer_office_id);
							cmd.Parameters.AddWithValue("@customer_contact_id", request.contact_id);
							cmd.Parameters.AddWithValue("@office_contact_id", request.customer_office_id);
							cmd.ExecuteNonQuery();
						}
					}
					return Ok(new
					{
						status = "Success",
						request.contact_id
					});
				}
				else
				{
					return BadRequest(new
					{
						status = "Request can't contains null"
					});
				}
			}
			catch (Exception ex)
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
		private bool _IsExists(string customer_id)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT EXISTS (SELECT true FROM customers WHERE customer_id='{customer_id}')";
				return (bool)cmd.ExecuteScalar();
			}
		}
		private string _getContactIdWithEmail(string contactEmail)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT contact_id FROM customer_contacts WHERE contact_email='{contactEmail}'";
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.HasRows && reader.Read())
					{
						return Convert.ToString(reader["contact_id"]);
					}
					else
					{
						return null;
					}
				}
			}
		}

		[HttpGet]
		[Route("FindContacts")]
		public IActionResult FindContacts(FindContact criteria)
		{
			try
			{
				List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
				string where = string.Empty;
				if (criteria != null)
				{
					string query = @"SELECT customer_contacts.contact_email, customer_contacts.contact_display_name,
customer_contacts.contact_mobile_phone, customer_contacts.contact_id, customer_contacts.contact_phone,
customer_contacts.contact_status, customer_contacts.company_id, customer_contacts.company_name,
customer_contacts.customer_id, customer_contacts.company_office_id, customer_contacts.company_office_name,
customer_contacts.contact_firstname, customer_contacts.contact_lastname, customer_contacts.contact_role,
customer_contacts.contact_title, user_contacts.user_id,customer_contacts.create_datetime, customer_contacts.edit_datetime, customer_contacts.contact_address1, customer_contacts.contact_address2, 
customer_contacts.contact_city, customer_contacts.contact_country, customer_contacts.contact_mobile_phone, customer_contacts.contact_photo_id,
customer_contacts.contact_record_source, customer_contacts.contact_state, customer_contacts.contact_zip, 
 customer_contacts.contact_verification_datetime, customer_contacts.contact_verification_level, 
customer_contacts.contact_crm_id, customer_contacts.create_user_id, customer_contacts.edit_user_id FROM customer_contacts LEFT JOIN user_contacts ON
customer_contacts.contact_id = user_contacts.customer_contact_id where customer_contacts.customer_id = @customer_id";

					//verify required fields
					var missingParameter = criteria.CheckRequiredParameters(new string[] { "customer_id" });

					if (missingParameter != null)
					{
						return BadRequest(new { status = missingParameter + " is required" });
					}
					criteria.detail_level = string.IsNullOrEmpty(criteria.detail_level) ? "basic" : criteria.detail_level;

					using (var cmd = _dbHelper.SpawnCommand())
					{
						cmd.Parameters.AddWithValue("@customer_id", criteria.customer_id);
						//Search By company_id
						if (!string.IsNullOrEmpty(criteria.company_id))
						{
							where = " and customer_contacts.company_id=@company_id";
							cmd.Parameters.AddWithValue("@company_id", criteria.company_id);
						}
						//Search By contact_lastname
						if (!string.IsNullOrEmpty(criteria.contact_lastname))
						{
							where = " and customer_contacts.contact_lastname= @contact_lastname";
							cmd.Parameters.AddWithValue("@contact_lastname", criteria.contact_lastname);
						}
						//Search By user_id
						if (!string.IsNullOrEmpty(criteria.user_id))
						{
							where = " and user_contacts.user_id= @user_id";
							cmd.Parameters.AddWithValue("@user_id", criteria.user_id);
						}
						//Search By customer_office_id
						if (!string.IsNullOrEmpty(criteria.customer_office_id))
						{
							query = string.Empty;
							query = @"SELECT customer_contacts.contact_email, customer_contacts.contact_display_name,
customer_contacts.contact_mobile_phone, customer_contacts.contact_id, customer_contacts.contact_phone,
customer_contacts.contact_status, customer_contacts.company_id, customer_contacts.company_name,
customer_contacts.customer_id, customer_contacts.company_office_id, customer_contacts.company_office_name, 
customer_contacts.contact_firstname, customer_contacts.contact_lastname, customer_contacts.contact_role,
customer_contacts.contact_title, user_contacts.user_id,customer_contacts.create_datetime, customer_contacts.edit_datetime, customer_contacts.contact_address1, customer_contacts.contact_address2, 
customer_contacts.contact_city, customer_contacts.contact_country, customer_contacts.contact_mobile_phone, customer_contacts.contact_photo_id,
customer_contacts.contact_record_source, customer_contacts.contact_state, customer_contacts.contact_zip, 
 customer_contacts.contact_verification_datetime, customer_contacts.contact_verification_level, 
customer_contacts.contact_crm_id, customer_contacts.create_user_id, customer_contacts.edit_user_id FROM customer_contacts LEFT JOIN user_contacts ON
customer_contacts.contact_id = user_contacts.customer_contact_id 
join customer_office_contacts on customer_office_contacts.customer_office_id = @customer_office_id
WHERE customer_contacts.customer_id =  @customer_id";
							cmd.Parameters.AddWithValue("@customer_office_id", criteria.customer_office_id);
						}

						query += where;

						cmd.CommandText = query;
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								result.Add(new Dictionary<string, object>
																{
																		{ "company_office_id", Convert.ToString(reader["company_office_id"]) },
																		{ "company_office_name", Convert.ToString(reader["company_office_name"]) },
																		{ "contact_firstname", Convert.ToString(reader["contact_firstname"]) },
																		{ "contact_lastname", Convert.ToString(reader["contact_lastname"]) },
																		{ "contact_role", Convert.ToString(reader["contact_role"]) },
																		{ "contact_title", Convert.ToString(reader["contact_title"]) },

																		{ "contact_email", Convert.ToString(reader["contact_email"]) },
																		{ "contact_display_name", Convert.ToString(reader["contact_display_name"]) },
																		{ "contact_id", Convert.ToString(reader["contact_id"]) },
																		{ "contact_phone", Convert.ToString(reader["contact_phone"]) },
																		{ "contact_status", Convert.ToString(reader["contact_status"]) },
																		{ "company_id", Convert.ToString(reader["company_id"]) },
																		{ "company_name", Convert.ToString(reader["company_name"]) },
																		{ "customer_id", Convert.ToString(reader["customer_id"]) },

																		{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
																		{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
																		{ "contact_address1", Convert.ToString(reader["contact_address1"]) },
																		{ "contact_address2", Convert.ToString(reader["contact_address2"]) },
																		{ "contact_city", Convert.ToString(reader["contact_city"]) },
																		{ "contact_country", Convert.ToString(reader["contact_country"]) },
																		{ "contact_mobile_phone", Convert.ToString(reader["contact_mobile_phone"]) },
																		{ "contact_photo_id", Convert.ToString(reader["contact_photo_id"]) },
																		{ "contact_record_source", Convert.ToString(reader["contact_record_source"]) },
																		{ "contact_state", Convert.ToString(reader["contact_state"]) },
																		{ "contact_zip", Convert.ToString(reader["contact_zip"]) },
									//{ "contact_service_area", reader["contact_service_area"] },
									{ "customer_name", Convert.ToString(reader["contact_firstname"])+" "+Convert.ToString(reader["contact_lastname"]) },
																		{ "contact_verification_datetime", Convert.ToString(reader["contact_verification_datetime"]) },
																		{ "contact_verification_level", Convert.ToString(reader["contact_verification_level"]) },

																		{ "contact_crm_id", Convert.ToString(reader["contact_crm_id"]) },
																		{ "create_user_id", Convert.ToString(reader["create_user_id"]) },
																		{ "edit_user_id", Convert.ToString(reader["edit_user_id"]) }
																});
							}
						}

						criteria.detail_level = criteria.detail_level.ToLower();
						switch (criteria.detail_level)
						{
							case "basic":
								foreach (var item in result)
								{
									item.Remove("create_datetime");
									item.Remove("edit_datetime");
									item.Remove("contact_address1");
									item.Remove("contact_address2");
									item.Remove("contact_city");
									item.Remove("contact_country");
									item.Remove("contact_mobile_phone ");
									item.Remove("contact_photo_id");
									item.Remove("contact_record_source");
									item.Remove("contact_state");
									item.Remove("contact_zip");
									item.Remove("customer_name");
									item.Remove("contact_verification_datetime");
									item.Remove("contact_verification_level");
									item.Remove("contact_crm_id");
									item.Remove("create_user_id");
									item.Remove("edit_user_id");
								};
								break;
							default:
								break;
						}

						return Ok(result);
					}
				}
				else
				{
					return BadRequest(new
					{
						status = "please provide details to filter!"
					});
				}
			}
			catch (Exception ex)
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
		[Route("GetContact")]
		public IActionResult GetContact(string contact_id)
		{
			List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
			try
			{
				if (!string.IsNullOrEmpty(contact_id))
				{
					using (var cmd = _dbHelper.SpawnCommand())
					{
						cmd.CommandText = @"SELECT customer_contacts.contact_email, customer_contacts.contact_display_name,
customer_contacts.contact_mobile_phone, customer_contacts.contact_id, customer_contacts.contact_phone,
customer_contacts.contact_status, customer_contacts.company_id, customer_contacts.company_name,
customer_contacts.customer_id, customer_contacts.company_office_id, customer_contacts.company_office_name,
customer_contacts.contact_firstname, customer_contacts.contact_lastname, customer_contacts.contact_role,
customer_contacts.contact_title, user_contacts.user_id,customer_contacts.create_datetime, customer_contacts.edit_datetime, customer_contacts.contact_address1, customer_contacts.contact_address2, 
customer_contacts.contact_city, customer_contacts.contact_country, customer_contacts.contact_mobile_phone, customer_contacts.contact_photo_id,
customer_contacts.contact_record_source, customer_contacts.contact_state, customer_contacts.contact_zip, 
 customer_contacts.contact_verification_datetime, customer_contacts.contact_verification_level, 
customer_contacts.contact_crm_id, customer_contacts.create_user_id, customer_contacts.edit_user_id FROM customer_contacts LEFT JOIN user_contacts ON
customer_contacts.contact_id = user_contacts.customer_contact_id where customer_contacts.contact_id = @contact_id";
						cmd.Parameters.AddWithValue("@contact_id", contact_id);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								result.Add(new Dictionary<string, object>
																{
																		{ "company_office_id", Convert.ToString(reader["company_office_id"]) },
																		{ "company_office_name", Convert.ToString(reader["company_office_name"]) },
																		{ "contact_firstname", Convert.ToString(reader["contact_firstname"]) },
																		{ "contact_lastname", Convert.ToString(reader["contact_lastname"]) },
																		{ "contact_role", Convert.ToString(reader["contact_role"]) },
																		{ "contact_title", Convert.ToString(reader["contact_title"]) },

																		{ "contact_email", Convert.ToString(reader["contact_email"]) },
																		{ "contact_display_name", Convert.ToString(reader["contact_display_name"]) },
																		{ "contact_id", Convert.ToString(reader["contact_id"]) },
																		{ "contact_phone", Convert.ToString(reader["contact_phone"]) },
																		{ "contact_status", Convert.ToString(reader["contact_status"]) },
																		{ "company_id", Convert.ToString(reader["company_id"]) },
																		{ "company_name", Convert.ToString(reader["company_name"]) },
																		{ "customer_id", Convert.ToString(reader["customer_id"]) },

																		{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
																		{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
																		{ "contact_address1", Convert.ToString(reader["contact_address1"]) },
																		{ "contact_address2", Convert.ToString(reader["contact_address2"]) },
																		{ "contact_city", Convert.ToString(reader["contact_city"]) },
																		{ "contact_country", Convert.ToString(reader["contact_country"]) },
																		{ "contact_mobile_phone", Convert.ToString(reader["contact_mobile_phone"]) },
																		{ "contact_photo_id", Convert.ToString(reader["contact_photo_id"]) },
																		{ "contact_record_source", Convert.ToString(reader["contact_record_source"]) },
																		{ "contact_state", Convert.ToString(reader["contact_state"]) },
																		{ "contact_zip", Convert.ToString(reader["contact_zip"]) },
                  //{ "contact_service_area", reader["contact_service_area"] },
                  { "contact_username", Convert.ToString(reader["contact_firstname"])+" " +Convert.ToString(reader["contact_lastname"]) },
																		{ "contact_verification_datetime", Convert.ToString(reader["contact_verification_datetime"]) },
																		{ "contact_verification_level", Convert.ToString(reader["contact_verification_level"]) },

																		{ "contact_crm_id", Convert.ToString(reader["contact_crm_id"]) },
																		{ "create_user_id", Convert.ToString(reader["create_user_id"]) },
																		{ "edit_user_id",Convert.ToString(reader["edit_user_id"]) }
																});
							}
							return Ok(result);
						}
					}
				}
				else
				{
					return BadRequest(new
					{
						status = "contact_id can't be null"
					});
				}
			}
			catch (Exception ex)
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
		[Route("UpdateContact")]
		public IActionResult UpdateContact(ContactFilter request, bool isInternal = false)
		{
			try
			{
				if (request != null)
				{
					string userId = Guid.NewGuid().ToString();
					DateTime _date = DateTime.UtcNow;

					//verify required fields
					var missingParameter = request.CheckRequiredParameters(new string[] { "search_contact_id" });

					if (missingParameter != null)
					{
						return BadRequest(new
						{
							status = missingParameter + " is required"
						});
					}
					//if(string.IsNullOrEmpty(request.search_contact_id))
					//    return BadRequest(new { status = "Please provide search_contact_id to continue update the contact." });

					request.contact_status = string.IsNullOrEmpty(request.contact_status) ? "active" : request.contact_status;
					using (var cmd = _dbHelper.SpawnCommand())
					{
						string command = @"UPDATE public.customer_contacts SET edit_datetime=@edit_datetime ";
						cmd.Parameters.AddWithValue("@edit_datetime", _date);

						if (!string.IsNullOrEmpty(request.contact_email))
						{
							command += " ,contact_email= @contact_email";
							cmd.Parameters.AddWithValue("@contact_email", request.contact_email);
						}
						if (!string.IsNullOrEmpty(request.contact_firstname))
						{
							command += " ,contact_firstname= @contact_firstname";
							cmd.Parameters.AddWithValue("@contact_firstname", request.contact_firstname);
						}
						if (!string.IsNullOrEmpty(request.contact_lastname))
						{
							command += " ,contact_lastname= @contact_lastname";
							cmd.Parameters.AddWithValue("@contact_lastname", request.contact_lastname);
						}
						if (!string.IsNullOrEmpty(request.contact_phone))
						{
							command += " ,contact_phone= @contact_phone";
							cmd.Parameters.AddWithValue("@contact_phone", request.contact_phone);
						}
						if (!string.IsNullOrEmpty(request.contact_address1))
						{
							command += " ,contact_address1= @contact_address1";
							cmd.Parameters.AddWithValue("@contact_address1", request.contact_address1);
						}
						if (!string.IsNullOrEmpty(request.contact_address2))
						{
							command += " ,contact_address2= @contact_address2";
							cmd.Parameters.AddWithValue("@contact_address2", request.contact_address2);
						}
						if (!string.IsNullOrEmpty(request.contact_city))
						{
							command += " ,contact_city= @contact_city";
							cmd.Parameters.AddWithValue("@contact_city", request.contact_city);
						}
						if (!string.IsNullOrEmpty(request.contact_state))
						{
							command += " ,contact_state= @contact_state";
							cmd.Parameters.AddWithValue("@contact_state", request.contact_state);
						}


						if (!string.IsNullOrEmpty(request.contact_zip))
						{
							command += " ,contact_zip = @contact_zip";
							cmd.Parameters.AddWithValue("@contact_zip", request.contact_zip);
						}
						if (!string.IsNullOrEmpty(request.contact_country))
						{
							command += " ,contact_country= @contact_country";
							cmd.Parameters.AddWithValue("@contact_country", request.contact_country);
						}
						if (!string.IsNullOrEmpty(request.contact_crm_id))
						{
							command += " ,contact_crm_id= @contact_crm_id";
							cmd.Parameters.AddWithValue("@contact_crm_id", request.contact_crm_id);
						}
						if (!string.IsNullOrEmpty(request.contact_photo_id))
						{
							command += " ,contact_photo_id= @contact_photo_id";
							cmd.Parameters.AddWithValue("@contact_photo_id", request.contact_photo_id);
						}
						if (!string.IsNullOrEmpty(request.customer_id))
						{
							command += " ,customer_id= @customer_id";
							cmd.Parameters.AddWithValue("@customer_id", request.customer_id);
						}
						if (!string.IsNullOrEmpty(request.contact_status))
						{
							command += " ,contact_status= @contact_status";
							cmd.Parameters.AddWithValue("@contact_status", request.contact_status);
						}
						command += " WHERE contact_id= @search_contact_id ";
						cmd.Parameters.AddWithValue("@search_contact_id", request.search_contact_id);
						cmd.CommandText = command;
						int affctedrowcount = cmd.ExecuteNonQuery();
						if (affctedrowcount == 0)
						{
							return BadRequest(new
							{
								status = "No matching record found for search_contact_id =" + request.search_contact_id
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
				}
				else
				{
					return BadRequest(new
					{
						status = "Request Can't contains null"
					});
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					status = ex.Message
				});
			}
			finally
			{
				if (!isInternal)
				{
					_dbHelper.CloseConnection();
				}
			}
		}
	}
}
