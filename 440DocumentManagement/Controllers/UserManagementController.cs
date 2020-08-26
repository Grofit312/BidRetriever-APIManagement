using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Http;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("User Management")]
	public class UserManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		private readonly IAmazonSimpleEmailService sesClient;

		public UserManagementController(IAmazonSimpleEmailService sesClient)
		{
			_dbHelper = new DatabaseHelper();

			this.sesClient = sesClient;
		}


		[HttpPost]
		[Route("RegisterUser")]
		public async System.Threading.Tasks.Task<IActionResult> PostAsync(UserRegistrationRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"user_email",
					"user_password",
					"user_firstname",
					"user_lastname",
					"customer_name"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"Please provide {missingParameter}"
					});
				}

				// check parameter validation
				if (request.user_password.Length < 4)
				{
					return BadRequest(new
					{
						status = "Password needs to be at least 4 characters"
					});
				}

				// check if email already exists
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT user_email, user_password, user_id, user_role, customer_id FROM users WHERE LOWER(user_email)=@user_email";
					cmd.Parameters.AddWithValue("user_email", request.user_email.ToLower());

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							// exists, check user_password is empty or not.
							var password = _dbHelper.SafeGetString(reader, 1);
							var userId = _dbHelper.SafeGetString(reader, 2);
							var userRole = _dbHelper.SafeGetString(reader, 3);
							var customerId = _dbHelper.SafeGetString(reader, 4);

							reader.Close();

							if (password == string.Empty)
							{
								// this is trial user or created by admin on the portal app. should update existing info.

								if (customerId == string.Empty)
								{
									// this is trial user, so should create customer
									customerId = Guid.NewGuid().ToString();
									userRole = "sys admin";

									var createCustomerResult = Post(new Customer
									{
										customer_name = request.customer_name,
										customer_admin_user_id = userId,
										customer_id = customerId,
									}, true);

									if (createCustomerResult is BadRequestObjectResult)
									{
										return createCustomerResult;
									}
								}
								else
								{
									userRole = __checkSysAdminExists(customerId) ? "user" : "sys admin";
								}

								var updateUserResult = Post(new UserUpdateRequest
								{
									search_user_email = request.user_email,
									customer_id = customerId,
									user_firstname = request.user_firstname,
									user_lastname = request.user_lastname,
									user_password = request.user_password,
									user_role = userRole,
								}, true);

								if (updateUserResult is BadRequestObjectResult)
								{
									return updateUserResult;
								}

								return Ok(new
								{
									status = "Successfully registered",
									user = new Dictionary<string, string>
									{
										{ "user_id", userId },
										{ "customer_id", customerId },
										{ "customer_name", request.customer_name },
										{ "user_firstname", request.user_firstname },
										{ "user_lastname", request.user_lastname },
										{ "user_email", request.user_email },
										{ "user_role", userRole },
										{ "token", JWT.CreateTokenStringFromUserId(userId) }
									}
								});
							}
							else
							{
								// customer already exists - cannot continue registration
								return BadRequest(new
								{
									status = "User already exists"
								});
							}
						}
						else
						{
							reader.Close();

							// not exists, should proceed registration
							var userId = Guid.NewGuid().ToString();
							var customerId = Guid.NewGuid().ToString();
							var userRole = "sys admin";

							var createUserResult = Post(new User
							{
								user_id = userId,
								user_email = request.user_email,
								user_firstname = request.user_firstname,
								user_lastname = request.user_lastname,
								user_password = request.user_password,
								user_role = userRole,
							}, true);

							if (createUserResult is BadRequestObjectResult)
							{
								return createUserResult;
							}

							var createCustomerResult = Post(new Customer
							{
								customer_name = request.customer_name,
								customer_admin_user_id = userId,
								customer_id = customerId,
							}, true);

							if (createCustomerResult is BadRequestObjectResult)
							{
								return createCustomerResult;
							}

							var addToCompanyResult = await PostAsync(new UserAddCompanyRequest
							{
								user_id = userId,
								customer_id = customerId,
							}, true);

							if (addToCompanyResult is BadRequestObjectResult)
							{
								return addToCompanyResult;
							}

							return Ok(new
							{
								status = "Successfully registered",
								user = new Dictionary<string, string>
								{
									{ "user_id", userId },
									{ "customer_id", customerId },
									{ "customer_name", request.customer_name },
									{ "user_firstname", request.user_firstname },
									{ "user_lastname", request.user_lastname },
									{ "user_email", request.user_email },
									{ "user_role", userRole },
									{ "token", JWT.CreateTokenStringFromUserId(userId) }
								}
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


		[HttpPost]
		[Route("LoginUser")]
		public IActionResult Post(UserLoginRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "user_email", "user_password" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"Please provide {missingParameter}"
					});
				}

				// login
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT users.user_id, users.customer_id, customers.customer_name, users.status, users.user_firstname, users.user_lastname, "
																					+ "users.user_email, users.user_role, users.user_password, users.customer_office_id FROM users LEFT JOIN customers ON (customers.customer_id=users.customer_id) WHERE LOWER(users.user_email)=@user_email";
					cmd.Parameters.AddWithValue("user_email", request.user_email.ToLower());

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var password = _dbHelper.SafeGetString(reader, 8);

							if (password == string.Empty)
							{
								return BadRequest(new
								{
									status = "This user is not registered yet"
								});
							}
							else
							{
								if (Hasher.Validate(request.user_password, password))
								{
									return Ok(new
									{
										status = "Successfully logged in",
										user = new Dictionary<string, string>()
										{
											{ "user_id", _dbHelper.SafeGetString(reader, 0) },
											{ "customer_id", _dbHelper.SafeGetString(reader, 1) },
											{ "customer_name", _dbHelper.SafeGetString(reader, 2) },
											{ "status", _dbHelper.SafeGetString(reader, 3) },
											{ "user_firstname", _dbHelper.SafeGetString(reader, 4) },
											{ "user_lastname", _dbHelper.SafeGetString(reader, 5) },
											{ "user_email", _dbHelper.SafeGetString(reader, 6) },
											{ "user_role", _dbHelper.SafeGetString(reader, 7) },
											{ "customer_office_id", _dbHelper.SafeGetString(reader, 9) },
											{ "token", JWT.CreateTokenStringFromUserId(_dbHelper.SafeGetString(reader, 0)) }
										}
									});
								}
								else
								{
									return BadRequest(new
									{
										status = "Password is not correct"
									});
								}
							}
						}
						else
						{
							return BadRequest(new
							{
								status = "Cannot find user"
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


		[HttpPost]
		[Route("AuthenticateUser")]
		public IActionResult Post(UserAuthenticationRequest request)
		{
			try
			{
				// check token
				if (request.token == null)
				{
					return BadRequest(new
					{
						status = "Token is missing"
					});
				}

				// authenticate
				var userId = JWT.GetParameterFromToken(request.token, "user_id");

				if (userId == null)
				{
					return BadRequest(new
					{
						status = "Token is invalid"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT users.user_id, users.customer_id, customers.customer_name, users.status, users.user_firstname, users.user_lastname, "
						+ "users.user_email, users.user_role, users.customer_office_id FROM users LEFT JOIN customers ON (customers.customer_id=users.customer_id) WHERE users.user_id='" + userId + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new
							{
								status = "Successfully authenticated",
								user = new Dictionary<string, string>()
								{
									{ "user_id", _dbHelper.SafeGetString(reader, 0) },
									{ "customer_id", _dbHelper.SafeGetString(reader, 1) },
									{ "customer_name", _dbHelper.SafeGetString(reader, 2) },
									{ "status", _dbHelper.SafeGetString(reader, 3) },
									{ "user_firstname", _dbHelper.SafeGetString(reader, 4) },
									{ "user_lastname", _dbHelper.SafeGetString(reader, 5) },
									{ "user_email", _dbHelper.SafeGetString(reader, 6) },
									{ "user_role", _dbHelper.SafeGetString(reader, 7) },
									{ "customer_office_id", _dbHelper.SafeGetString(reader, 8) },
									{ "token", JWT.CreateTokenStringFromUserId(_dbHelper.SafeGetString(reader, 0)) }
								}
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "Cannot find user"
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


		[HttpPost]
		[Route("ForgotPassword")]
		public async System.Threading.Tasks.Task<IActionResult> PostAsync(UserForgotPasswordRequest request)
		{
			try
			{
				// validation check
				if (request.user_email == null)
				{
					return BadRequest(new
					{
						status = "Please provide email"
					});
				}

				// lookup user
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT user_id, customer_id FROM users WHERE LOWER(user_email)=@user_email";
					cmd.Parameters.AddWithValue("user_email", request.user_email.ToLower());

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var userId = _dbHelper.SafeGetString(reader, 0);
							var customerId = _dbHelper.SafeGetString(reader, 1);

							if (string.IsNullOrEmpty(customerId))
							{
								return BadRequest(new
								{
									status = "Trial user should sign up first"
								});
							}

							// generate reset link and send to user via email
							var token = JWT.CreateExpiryTokenStringFromUserId(userId);

							reader.Close();

							await MailSender.SendEmailAsync(
								sesClient,
								new List<string> { request.user_email },
								"Reset Password",
								$"Click <a href='{__getCustomerPortalUrl() + "/reset-password?token=" + token}' target='_blank'>here</a> to reset your password.");

							return Ok(new
							{
								status = "Reset link has been sent to your email address"
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "User with that email doesn't exist"
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


		[HttpPost]
		[Route("ResetPassword")]
		public IActionResult Post(UserResetPasswordRequest request)
		{
			try
			{
				// validation check
				if (request.token == null)
				{
					return BadRequest(new
					{
						status = "Please provide token string"
					});
				}

				if (request.user_password == null)
				{
					return BadRequest(new
					{
						status = "Please provide new password"
					});
				}

				if (JWT.CheckTokenExpiration(request.token) == true)
				{
					return BadRequest(new
					{
						status = "Token is invalid or expired"
					});
				}

				var userId = JWT.GetParameterFromToken(request.token, "user_id");

				if (userId == null)
				{
					return BadRequest(new
					{
						status = "Token is invalid"
					});
				}

				// update user password
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"UPDATE users SET (user_password)=(@user_password) WHERE user_id='{userId}'";
					cmd.Parameters.AddWithValue("user_password", Hasher.Create(request.user_password));

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "Cannot find user"
						});
					}
					else
					{
						return Ok(new
						{
							status = "Your password has been updated"
						});
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


		[HttpPost]
		[Route("ChangeCompany")]
		public IActionResult Post(UserCompanyChangeRequest request)
		{
			try
			{
				// validation check
				if (request.token == null)
				{
					return BadRequest(new
					{
						status = "Please provide token string"
					});
				}

				var userId = JWT.GetParameterFromToken(request.token, "user_id");
				var customerId = JWT.GetParameterFromToken(request.token, "customer_id");

				if (userId == null || customerId == null)
				{
					return BadRequest(new
					{
						status = "Token is invalid"
					});
				}

				// update user's company
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"UPDATE users SET (customer_id)=(@customer_id) WHERE user_id='{userId}'";
					cmd.Parameters.AddWithValue("customer_id", customerId);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "Cannot find user"
						});
					}
					else
					{
						return Ok(new
						{
							status = "Company info has been updated"
						});
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


		[HttpPost]
		[Route("CreateUser")]
		public IActionResult Post(User user, bool isInternalRequest = false)
		{
			try
			{
				// check missing parameter
				var missingParameter = user.CheckRequiredParameters(new string[]
				{
					"user_email"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				if (user.user_password != null)
				{
					user.user_password = Hasher.Create(user.user_password);
				}

				// check if email already exists
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var userId = user.user_id ?? Guid.NewGuid().ToString();
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = "SELECT user_id, user_firstname, user_lastname, user_phone, user_address1, "
									+ "user_address2, user_city, user_state, user_zip, user_country, user_photo_id, user_username, "
									+ "user_password, user_crm_id, user_role "
									+ "from users WHERE LOWER(user_email)=@user_email";
					cmd.Parameters.AddWithValue("user_email", user.user_email.ToLower());

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read() == false)
						{
							reader.Close();

							// not exist.  just create new record
							cmd.CommandText = "INSERT INTO users ("
								+ "user_id, user_email, user_firstname, user_lastname, user_displayname, user_phone, "
								+ "user_photo_id, "
								+ "user_address1, user_address2, user_city, user_state, user_zip, "
								+ "user_country, user_crm_id, user_username, user_password, status, create_datetime, edit_datetime, user_role, customer_office_id) "
								+ "VALUES(@user_id, @user_email, @user_firstname, @user_lastname, @user_displayname, @user_phone, "
								+ "@user_photo_id, "
								+ "@user_address1, @user_address2, @user_city, @user_state, @user_zip, "
								+ "@user_country, @user_crm_id, @user_username, @user_password, "
								+ "@status, @create_datetime, @edit_datetime, @user_role, @customer_office_id)";

							var userDisplayName = string.IsNullOrEmpty(user.user_firstname) || string.IsNullOrEmpty(user.user_lastname)
									? $"{user.user_lastname}{user.user_firstname}"
									: $"{user.user_lastname}, {user.user_firstname}";

							cmd.Parameters.AddWithValue("user_id", userId);
							cmd.Parameters.AddWithValue("user_email", user.user_email);
							cmd.Parameters.AddWithValue("user_firstname", user.user_firstname ?? "");
							cmd.Parameters.AddWithValue("user_lastname", user.user_lastname ?? "");
							cmd.Parameters.AddWithValue("user_displayname", userDisplayName);
							cmd.Parameters.AddWithValue("user_phone", user.user_phone ?? "");
							cmd.Parameters.AddWithValue("user_address1", user.user_address1 ?? "");
							cmd.Parameters.AddWithValue("user_address2", user.user_address2 ?? "");
							cmd.Parameters.AddWithValue("user_city", user.user_city ?? "");
							cmd.Parameters.AddWithValue("user_state", user.user_state ?? "");
							cmd.Parameters.AddWithValue("user_zip", user.user_zip ?? "");
							cmd.Parameters.AddWithValue("user_country", user.user_country ?? "");
							cmd.Parameters.AddWithValue("user_crm_id", user.user_crm_id ?? "");
							cmd.Parameters.AddWithValue("user_username", user.user_username ?? "");
							cmd.Parameters.AddWithValue("user_password", user.user_password ?? "");
							cmd.Parameters.AddWithValue("status", user.status ?? "active");
							cmd.Parameters.AddWithValue("create_datetime", timestamp);
							cmd.Parameters.AddWithValue("edit_datetime", timestamp);
							cmd.Parameters.AddWithValue("user_photo_id", user.user_photo_id ?? "");
							cmd.Parameters.AddWithValue("user_role", user.user_role ?? "user");
							cmd.Parameters.AddWithValue("customer_office_id", user.customer_office_id ?? "");

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								user_id = userId,
								status = "completed"
							});
						}
						else
						{
							// exist, update current user if required
							var currentUserId = _dbHelper.SafeGetString(reader, 0);
							var currentFirstName = _dbHelper.SafeGetString(reader, 1);
							var currentLastName = _dbHelper.SafeGetString(reader, 2);
							var currentPhone = _dbHelper.SafeGetString(reader, 3);
							var currentAddress1 = _dbHelper.SafeGetString(reader, 4);
							var currentAddress2 = _dbHelper.SafeGetString(reader, 5);
							var currentCity = _dbHelper.SafeGetString(reader, 6);
							var currentState = _dbHelper.SafeGetString(reader, 7);
							var currentZip = _dbHelper.SafeGetString(reader, 8);
							var currentCountry = _dbHelper.SafeGetString(reader, 9);
							var currentPhotoId = _dbHelper.SafeGetString(reader, 10);
							var currentUsername = _dbHelper.SafeGetString(reader, 11);
							var currentPassword = _dbHelper.SafeGetString(reader, 12);
							var currentCrmId = _dbHelper.SafeGetString(reader, 13);
							var currentUserRole = _dbHelper.SafeGetString(reader, 14);

							reader.Close();

							var columns = "user_firstname, user_lastname, user_displayname, user_phone, user_address1, "
																			+ "user_address2, user_city, user_state, user_zip, user_country, "
																			+ "user_username, user_password, user_crm_id, user_photo_id, user_role";
							var values = "@user_firstname, @user_lastname, @user_displayname, @user_phone, @user_address1, "
																			+ "@user_address2, @user_city, @user_state, @user_zip, @user_country, "
																			+ "@user_username, @user_password, @user_crm_id, @user_photo_id, @user_role";
							cmd.CommandText = "UPDATE users SET (" + columns + ")=(" + values + ") WHERE LOWER(user_email)=@user_email";
							cmd.Parameters.AddWithValue("user_email", user.user_email.ToLower());

							var userFirstName = (currentFirstName == "" && user.user_firstname != null)
									? user.user_firstname : currentFirstName;
							var userLastName = (currentLastName == "" && user.user_lastname != null)
									? user.user_lastname : currentLastName;
							var userDisplayName = string.IsNullOrEmpty(userFirstName) || string.IsNullOrEmpty(userLastName)
									? $"{userLastName}{userFirstName}" : $"{userLastName}, {userFirstName}";

							cmd.Parameters.AddWithValue("user_firstname", userFirstName);
							cmd.Parameters.AddWithValue("user_lastname", userLastName);
							cmd.Parameters.AddWithValue("user_displayname", userDisplayName);
							cmd.Parameters.AddWithValue(
									"user_phone",
									(currentPhone == "" && user.user_phone != null) ? user.user_phone : currentPhone);
							cmd.Parameters.AddWithValue(
									"user_address1",
									(currentAddress1 == "" && user.user_address1 != null) ? user.user_address1 : currentAddress1);
							cmd.Parameters.AddWithValue(
									"user_address2",
									(currentAddress2 == "" && user.user_address2 != null) ? user.user_address2 : currentAddress2);
							cmd.Parameters.AddWithValue(
									"user_city",
									(currentCity == "" && user.user_city != null) ? user.user_city : currentCity);
							cmd.Parameters.AddWithValue(
									"user_state",
									(currentState == "" && user.user_state != null) ? user.user_state : currentState);
							cmd.Parameters.AddWithValue(
									"user_zip",
									(currentZip == "" && user.user_zip != null) ? user.user_zip : currentZip);
							cmd.Parameters.AddWithValue(
									"user_country",
									(currentCountry == "" && user.user_country != null) ? user.user_country : currentCountry);
							cmd.Parameters.AddWithValue(
									"user_crm_id",
									(currentCrmId == "" && user.user_crm_id != null) ? user.user_crm_id : currentCrmId);
							cmd.Parameters.AddWithValue(
									"user_username",
									(currentUsername == "" && user.user_username != null) ? user.user_username : currentUsername);
							cmd.Parameters.AddWithValue(
									"user_password",
									(currentPassword == "" && user.user_password != null) ? user.user_password : currentPassword);
							cmd.Parameters.AddWithValue(
									"user_photo_id",
									(currentPhotoId == "" && user.user_photo_id != null) ? user.user_photo_id : currentPhotoId);
							cmd.Parameters.AddWithValue(
									"user_role",
									(currentUserRole == "" && user.user_role != null) ? user.user_role : currentUserRole);

							cmd.ExecuteNonQuery();

							return Ok(new
							{
								user_id = currentUserId,
								status = "user already exists, updated record"
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
				if (!isInternalRequest)
				{
					_dbHelper.CloseConnection();
				}
			}
		}


		[HttpPost]
		[Route("CreateCustomer")]
		public IActionResult Post(Customer customer, bool isInternalRequest = false)
		{
			try
			{
				// check missing parameter
				var missingParameter = customer.CheckRequiredParameters(new string[] { "customer_name" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var customerId = customer.customer_id ?? Guid.NewGuid().ToString();
				var timestamp = DateTime.UtcNow;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// check project_id already exists
					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM customers WHERE customer_id='{customerId}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new { customer_id = customerId, status = "duplicated" });
					}

					if (!string.IsNullOrEmpty(customer.customer_domain))
					{
						cmd.CommandText = $"SELECT EXISTS (SELECT true from customers where customer_domain='{customer.customer_domain}')";
						if ((bool)cmd.ExecuteScalar() == true)
						{
							return BadRequest(new { status = "Customer domain is duplicated." });
						}
					}

					// create customer record
					var columns = "(customer_id, customer_name, customer_phone, customer_address1, customer_address2, "
																	+ "customer_city, customer_state, customer_zip, customer_country, customer_service_area, "
																	+ "customer_admin_user_id, customer_email, customer_photo_id, customer_duns_number, "
																	+ "company_type, company_website, customer_domain, record_source, "
																	+ "customer_crm_id, customer_timezone, status, create_datetime, edit_datetime)";
					var values = "(@customer_id, @customer_name, @customer_phone, @customer_address1, @customer_address2, "
																	+ "@customer_city, @customer_state, @customer_zip, @customer_country, @customer_service_area, "
																	+ "@customer_admin_user_id, @customer_email, @customer_photo_id, @customer_duns_number, "
																	+ "@company_type, @company_website, @customer_domain, @record_source, "
																	+ "@customer_crm_id, @customer_timezone, @status, @create_datetime, @edit_datetime)";

					cmd.CommandText = "INSERT INTO customers " + columns + " VALUES" + values;

					cmd.Parameters.AddWithValue("customer_id", customerId);
					cmd.Parameters.AddWithValue("customer_name", customer.customer_name);
					cmd.Parameters.AddWithValue("customer_admin_user_id", customer.customer_admin_user_id ?? "");
					cmd.Parameters.AddWithValue("customer_email", customer.customer_email ?? "");
					cmd.Parameters.AddWithValue("customer_photo_id", customer.customer_photo_id ?? "");
					cmd.Parameters.AddWithValue("customer_duns_number", customer.customer_duns_number ?? "");
					cmd.Parameters.AddWithValue("customer_phone", customer.customer_phone ?? "");
					cmd.Parameters.AddWithValue("customer_address1", customer.customer_address1 ?? "");
					cmd.Parameters.AddWithValue("customer_address2", customer.customer_address2 ?? "");
					cmd.Parameters.AddWithValue("customer_city", customer.customer_city ?? "");
					cmd.Parameters.AddWithValue("customer_state", customer.customer_state ?? "");
					cmd.Parameters.AddWithValue("customer_zip", customer.customer_zip ?? "");
					cmd.Parameters.AddWithValue("customer_country", customer.customer_country ?? "");
					cmd.Parameters.AddWithValue("customer_service_area", customer.customer_service_area ?? "");
					cmd.Parameters.AddWithValue("customer_crm_id", customer.customer_crm_id ?? "");
					cmd.Parameters.AddWithValue("customer_timezone", customer.customer_timezone ?? "");
					cmd.Parameters.AddWithValue("company_type", customer.company_type ?? "");
					cmd.Parameters.AddWithValue("company_website", customer.company_website ?? "");
					cmd.Parameters.AddWithValue("customer_domain", customer.customer_domain ?? "");
					cmd.Parameters.AddWithValue("record_source", customer.record_source ?? "");
					cmd.Parameters.AddWithValue("status", customer.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();
				}

				// create head office record
				var createCompanyOfficeResult = new CompanyOfficeManagementController().Post(new CompanyOffice()
				{
					customer_id = customerId,
					company_office_name = "Head Office",
					company_office_admin_user_id = customer.customer_admin_user_id,
					company_office_phone = customer.customer_phone,
					company_office_address1 = customer.customer_address1,
					company_office_address2 = customer.customer_address2,
					company_office_city = customer.customer_city,
					company_office_country = customer.customer_country,
					company_office_headoffice = true,
					company_office_service_area = customer.customer_service_area,
					company_office_state = customer.customer_state,
					company_office_zip = customer.customer_zip,
					company_office_timezone = customer.customer_timezone,
					status = "active"
				});

				if (createCompanyOfficeResult is BadRequestObjectResult)
				{
					return createCompanyOfficeResult;
				}

				// copy trialuser settings
				var trialUserSettings = new List<Dictionary<string, string>> { };

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT setting_desc, setting_group, setting_help_link, setting_id, setting_name, "
									+ "setting_sequence, setting_tooltiptext, setting_value, setting_value_data_type, status "
									+ "FROM customer_settings WHERE LOWER(customer_id)='trialuser'";

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							trialUserSettings.Add(new Dictionary<string, string>
							{
								{ "setting_desc", _dbHelper.SafeGetString(reader, 0) },
								{ "setting_group", _dbHelper.SafeGetString(reader, 1) },
								{ "setting_help_link", _dbHelper.SafeGetString(reader, 2) },
								{ "setting_id", _dbHelper.SafeGetString(reader, 3) },
								{ "setting_name", _dbHelper.SafeGetString(reader, 4) },
								{ "setting_sequence", _dbHelper.SafeGetString(reader, 5) },
								{ "setting_tooltiptext", _dbHelper.SafeGetString(reader, 6) },
								{ "setting_value", _dbHelper.SafeGetString(reader, 7) },
								{ "setting_value_data_type", _dbHelper.SafeGetString(reader, 8) },
								{ "status", _dbHelper.SafeGetString(reader, 9) },
							});
						}
					}
				}

				foreach (var setting in trialUserSettings)
				{
					using (var cmd = _dbHelper.SpawnCommand())
					{
						cmd.CommandText = "INSERT INTO customer_settings "
						+ "(create_datetime, customer_id, customer_setting_id, edit_datetime, setting_desc, "
						+ "setting_group, setting_help_link, setting_id, setting_name, setting_sequence, setting_tooltiptext, "
						+ "setting_value, setting_value_data_type, status) "
						+ "VALUES(@create_datetime, @customer_id, @customer_setting_id, @edit_datetime, @setting_desc, "
						+ "@setting_group, @setting_help_link, @setting_id, @setting_name, @setting_sequence, @setting_tooltiptext, "
						+ "@setting_value, @setting_value_data_type, @status)";

						cmd.Parameters.AddWithValue("create_datetime", timestamp);
						cmd.Parameters.AddWithValue("customer_id", customerId);
						cmd.Parameters.AddWithValue("customer_setting_id", Guid.NewGuid().ToString());
						cmd.Parameters.AddWithValue("edit_datetime", timestamp);
						cmd.Parameters.AddWithValue("setting_desc", setting["setting_desc"]);
						cmd.Parameters.AddWithValue("setting_group", setting["setting_group"]);
						cmd.Parameters.AddWithValue("setting_help_link", setting["setting_help_link"]);
						cmd.Parameters.AddWithValue("setting_id", setting["setting_id"]);
						cmd.Parameters.AddWithValue("setting_name", setting["setting_name"]);
						cmd.Parameters.AddWithValue("setting_sequence", setting["setting_sequence"]);
						cmd.Parameters.AddWithValue("setting_tooltiptext", setting["setting_tooltiptext"]);
						cmd.Parameters.AddWithValue("setting_value", setting["setting_value"]);
						cmd.Parameters.AddWithValue("setting_value_data_type", setting["setting_value_data_type"]);
						cmd.Parameters.AddWithValue("status", setting["status"]);

						cmd.ExecuteNonQuery();
					}
				}

				return Ok(new
				{
					customer_id = customerId,
					status = "completed",
				});
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
				if (!isInternalRequest)
				{
					_dbHelper.CloseConnection();
				}
			}
		}


		[HttpPost]
		[Route("AddCompanyUser")]
		public async System.Threading.Tasks.Task<IActionResult> PostAsync(UserAddCompanyRequest request, bool isInternalRequest = false)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "user_id", "customer_id" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var newCompanyName = "";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT customer_name FROM customers where customer_id='{request.customer_id}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							newCompanyName = _dbHelper.SafeGetString(reader, 0);
						}
						else
						{
							return BadRequest(new
							{
								status = "Company doesn't exist"
							});
						}
					}
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					// find user
					cmd.CommandText = $"SELECT user_email, customer_id FROM users where user_id='{request.user_id}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var userEmail = _dbHelper.SafeGetString(reader, 0);
							var currentCustomerId = _dbHelper.SafeGetString(reader, 1);
							reader.Close();

							if (currentCustomerId == string.Empty)
							{
								// add user to the company by setting his customer_id field
								cmd.CommandText = $"UPDATE users SET (customer_id)=(@customer_id) WHERE user_id='{request.user_id}'";
								cmd.Parameters.AddWithValue("customer_id", request.customer_id);
								cmd.ExecuteNonQuery();

								// update user's projects
								cmd.CommandText = $"UPDATE projects SET (project_customer_id)=(@project_customer_id) WHERE project_admin_user_id='{request.user_id}'";
								cmd.Parameters.AddWithValue("project_customer_id", request.customer_id);
								cmd.ExecuteNonQuery();

								return Ok(new { status = "Successfully added to the company" });
							}
							else
							{
								// send email to the user asking for company change
								var token = JWT.CreateCompanyChangeTokenString(request.user_id, request.customer_id);

								reader.Close();

								await MailSender.SendEmailAsync(
										sesClient,
										new List<string> { userEmail },
										"Company Change Request",
										$"Are you going to change your company to '{newCompanyName}'? Click <a href='{__getCustomerPortalUrl() + "/change-company?token=" + token}' target='_blank'>here</a> to approve the change.");

								return Ok(new
								{
									status = "Company change request has been sent"
								});
							}
						}
						else
						{
							return BadRequest(new
							{
								status = "User doesn't exist"
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
				if (!isInternalRequest)
				{
					_dbHelper.CloseConnection();
				}
			}
		}


		[HttpGet]
		[Route("GetUser")]
		public IActionResult Get(UserGetRequest request)
		{
			try
			{
				var detailLevel = (request.detail_level ?? "basic").ToLower();

				// validation check
				if (request.user_id == null && request.user_email == null && request.user_crm_id == null)
				{
					return BadRequest(new
					{
						status = "please provide at least one query parameter"
					});
				}

				if (detailLevel != "basic" && detailLevel != "all" && detailLevel != "admin")
				{
					return BadRequest(new { status = "incorrect detail_level" });
				}

				// run query
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE ";

					if (request.user_id != null)
					{
						whereString = $"{whereString}users.user_id='{request.user_id}' AND ";
					}
					if (request.user_email != null)
					{
						whereString = $"{whereString}LOWER(users.user_email)=@user_email AND ";
					}
					if (request.user_crm_id != null)
					{
						whereString = $"{whereString}users.user_crm_id='{request.user_crm_id}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText = "SELECT users.user_id, users.customer_id, customers.customer_name, users.status, "
																					+ "users.user_firstname, users.user_lastname, users.user_email, users.user_phone, "
																					+ "users.user_role, users.user_address1, users.user_address2, users.user_city, "
																					+ "users.user_state, users.user_zip, users.user_country, customers.customer_service_area, "
																					+ "users.create_datetime, users.edit_datetime, users.user_photo_id, "
																					+ "users.user_crm_id, users.create_user_id, users.edit_user_id, users.customer_office_id, users.user_password, "
																					+ "users.user_displayname "
																					+ "FROM customers RIGHT OUTER JOIN users ON users.customer_id=customers.customer_id" + whereString;

					if (!string.IsNullOrEmpty(request.user_email))
					{
						cmd.Parameters.AddWithValue("user_email", request.user_email.ToLower());
					}

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var user = new Dictionary<string, string>
							{
								{ "user_id", _dbHelper.SafeGetString(reader, 0) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 1) },
								{ "customer_name", _dbHelper.SafeGetString(reader, 2) },
								{ "status", _dbHelper.SafeGetString(reader, 3) },
								{ "user_firstname", _dbHelper.SafeGetString(reader, 4) },
								{ "user_lastname", _dbHelper.SafeGetString(reader, 5) },
								{ "user_displayname", _dbHelper.SafeGetString(reader, 24) },
								{ "user_email", _dbHelper.SafeGetString(reader, 6) },
								{ "user_phone", _dbHelper.SafeGetString(reader, 7) },
								{ "user_role", _dbHelper.SafeGetString(reader, 8) },
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								user["user_address1"] = _dbHelper.SafeGetString(reader, 9);
								user["user_address2"] = _dbHelper.SafeGetString(reader, 10);
								user["user_city"] = _dbHelper.SafeGetString(reader, 11);
								user["user_state"] = _dbHelper.SafeGetString(reader, 12);
								user["user_zip"] = _dbHelper.SafeGetString(reader, 13);
								user["user_country"] = _dbHelper.SafeGetString(reader, 14);
								user["user_service_area"] = _dbHelper.SafeGetString(reader, 15);
								user["create_datetime"] = ((DateTime)reader.GetValue(16)).ToString();
								user["edit_datetime"] = ((DateTime)reader.GetValue(17)).ToString();
								user["user_photo_id"] = _dbHelper.SafeGetString(reader, 18);
								user["customer_office_id"] = _dbHelper.SafeGetString(reader, 22);
							}

							if (detailLevel == "admin")
							{
								user["user_crm_id"] = _dbHelper.SafeGetString(reader, 19);
								user["create_user_id"] = _dbHelper.SafeGetString(reader, 20);
								user["edit_user_id"] = _dbHelper.SafeGetString(reader, 21);
								user["user_password"] = string.IsNullOrEmpty(_dbHelper.SafeGetString(reader, 22)) ? "not-existed" : "existed";
							}

							return Ok(user);
						}

						return BadRequest(new
						{
							status = "no matching user found"
						});
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
		[Route("GetCustomer")]
		public IActionResult Get(CustomerGetRequest request)
		{
			try
			{
				var detailLevel = (request.detail_level ?? "basic").ToLower();

				// validation check
				if (detailLevel != "basic" && detailLevel != "all" && detailLevel != "admin")
				{
					return BadRequest(new
					{
						status = "incorrect detail_level"
					});
				}

				if (request.customer_id == null && request.customer_crm_id == null && request.customer_domain == null)
				{
					return BadRequest(new
					{
						status = "please provide at least one query parameter"
					});
				}

				// run query
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = "";

					// check headoffice existence
					cmd.CommandText = "SELECT EXISTS (SELECT true FROM company_offices WHERE company_office_headoffice=TRUE AND customer_id='" + request.customer_id + "')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						whereString = " WHERE company_offices.company_office_headoffice=TRUE AND ";
						cmd.CommandText = "SELECT customers.customer_id, customers.status, customers.customer_name, "
										+ "company_offices.company_office_phone, customers.customer_email, company_offices.company_office_address1, company_offices.company_office_address2, "
										+ "company_offices.company_office_city, company_offices.company_office_state, company_offices.company_office_zip, company_offices.company_office_country, "
										+ "company_offices.company_office_service_area, customers.create_datetime, customers.edit_datetime, customers.customer_photo_id, "
										+ "customers.customer_logo_id, customers.full_address, company_offices.company_office_timezone, "
										+ "customers.customer_crm_id, customers.customer_subscription_level, customers.customer_admin_user_id, "
										+ "customers.customer_billing_id, customers.create_user_id, customers.edit_user_id, "
										+ "customers.company_type, customers.company_website, customers.record_source, customers.customer_domain "
										+ "FROM customers LEFT JOIN company_offices ON customers.customer_id=company_offices.customer_id ";
					}
					else
					{
						whereString = " WHERE ";
						cmd.CommandText = "SELECT customers.customer_id, customers.status, customers.customer_name, "
										+ "customers.customer_phone, customers.customer_email, customers.customer_address1, customers.customer_address2, "
										+ "customers.customer_city, customers.customer_state, customers.customer_zip, customers.customer_country, "
										+ "customers.customer_service_area, customers.create_datetime, customers.edit_datetime, customers.customer_photo_id, "
										+ "customers.customer_logo_id, customers.full_address, customers.customer_timezone, "
										+ "customers.customer_crm_id, customers.customer_subscription_level, customers.customer_admin_user_id, "
										+ "customers.customer_billing_id, customers.create_user_id, customers.edit_user_id, "
										+ "customers.company_type, customers.company_website, customers.record_source, customers.customer_domain "
										+ "FROM customers ";
					}

					if (request.customer_id != null)
					{
						whereString = $"{whereString}customers.customer_id='{request.customer_id}' AND ";
					}
					if (request.customer_crm_id != null)
					{
						whereString = $"{whereString}customer_crm_id='{request.customer_crm_id}' AND ";
					}
					if (request.customer_domain != null)
					{
						whereString = $"{whereString}customer_domain='{request.customer_domain}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					cmd.CommandText += whereString;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>>();

						while (reader.Read())
						{
							var result = new Dictionary<string, string>();

							result.Add("customer_id", _dbHelper.SafeGetString(reader, 0));
							result.Add("status", _dbHelper.SafeGetString(reader, 1));
							result.Add("customer_name", _dbHelper.SafeGetString(reader, 2));

							if (detailLevel == "all" || detailLevel == "admin")
							{
								result.Add("customer_phone", _dbHelper.SafeGetString(reader, 3));
								result.Add("customer_email", _dbHelper.SafeGetString(reader, 4));
								result.Add("customer_address1", _dbHelper.SafeGetString(reader, 5));
								result.Add("customer_address2", _dbHelper.SafeGetString(reader, 6));
								result.Add("customer_city", _dbHelper.SafeGetString(reader, 7));
								result.Add("customer_state", _dbHelper.SafeGetString(reader, 8));
								result.Add("customer_zip", _dbHelper.SafeGetString(reader, 9));
								result.Add("customer_country", _dbHelper.SafeGetString(reader, 10));
								result.Add("customer_service_area", _dbHelper.SafeGetString(reader, 11));
								result.Add("create_datetime", ((DateTime)reader.GetValue(12)).ToString());
								result.Add("edit_datetime", ((DateTime)reader.GetValue(13)).ToString());
								result.Add("customer_photo_id", _dbHelper.SafeGetString(reader, 14));
								result.Add("customer_logo_id", _dbHelper.SafeGetString(reader, 15));
								result.Add("full_address", _dbHelper.SafeGetString(reader, 16));
								result.Add("customer_timezone", _dbHelper.SafeGetString(reader, 17));
								result.Add("company_type", _dbHelper.SafeGetString(reader, 24));
								result.Add("company_website", _dbHelper.SafeGetString(reader, 25));
								result.Add("record_source", _dbHelper.SafeGetString(reader, 26));
								result.Add("customer_domain", _dbHelper.SafeGetString(reader, 27));
							}

							if (detailLevel == "admin")
							{
								result.Add("customer_crm_id", _dbHelper.SafeGetString(reader, 18));
								result.Add("customer_subscription_level", _dbHelper.SafeGetString(reader, 19));
								result.Add("customer_admin_user_id", _dbHelper.SafeGetString(reader, 20));
								result.Add("customer_billing_id", _dbHelper.SafeGetString(reader, 21));
								result.Add("create_user_id", _dbHelper.SafeGetString(reader, 22));
								result.Add("edit_user_id", _dbHelper.SafeGetString(reader, 23));
							}

							resultList.Add(result);
						}

						reader.Close();

						if (resultList.Count == 0)
						{
							return BadRequest(new
							{
								status = "no matching customer found"
							});
						}
						else if (resultList.Count == 1)
						{
							return Ok(resultList[0]);
						}
						else
						{
							return Ok(resultList);
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


		[HttpPost]
		[Route("UpdateUser")]
		public IActionResult Post(UserUpdateRequest request, bool isInternalRequest = false)
		{
			try
			{
				// validation check
				if (request.search_user_id == null
						&& request.search_user_email == null
						&& request.search_user_crm_id == null)
				{
					return BadRequest(new
					{
						status = "please provide at least one search parameter"
					});
				}

				if (request.user_password != null)
				{
					request.user_password = Hasher.Create(request.user_password);
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var whereString = " WHERE ";

					if (request.search_user_id != null)
					{
						whereString = $"{whereString}user_id='{request.search_user_id}' AND ";
					}
					if (request.search_user_email != null)
					{
						whereString = $"{whereString}LOWER(user_email)=@user_email AND ";
					}
					if (request.search_user_crm_id != null)
					{
						whereString = $"{whereString}user_crm_id='{request.search_user_crm_id}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					var queryString = "UPDATE users SET "
						+ "customer_id = COALESCE(@customer_id, customer_id), "
						+ "user_email = COALESCE(@user_email, user_email), "
						+ "user_firstname = COALESCE(@user_firstname, user_firstname), "
						+ "user_lastname = COALESCE(@user_lastname, user_lastname), "
						+ "user_phone = COALESCE(@user_phone, user_phone), "
						+ "user_address1 = COALESCE(@user_address1, user_address1), "
						+ "user_address2 = COALESCE(@user_address2, user_address2), "
						+ "user_city = COALESCE(@user_city, user_city), "
						+ "user_state = COALESCE(@user_state, user_state), "
						+ "user_zip = COALESCE(@user_zip, user_zip), "
						+ "user_country = COALESCE(@user_country, user_country), "
						+ "user_crm_id = COALESCE(@user_crm_id, user_crm_id), "
						+ "user_username = COALESCE(@user_username, user_username), "
						+ "user_password = COALESCE(@user_password, user_password), "
						+ "user_role = COALESCE(@user_role, user_role), "
						+ "user_photo_id = COALESCE(@user_photo_id, user_photo_id), "
						+ "customer_office_id = COALESCE(@customer_office_id, customer_office_id), "
						+ "status = COALESCE(@status, status), edit_datetime = @edit_datetime" + whereString;

					cmd.CommandText = queryString;

					if (!string.IsNullOrEmpty(request.search_user_email))
					{
						cmd.Parameters.AddWithValue("user_email", request.search_user_email.ToLower());
					}

					cmd.Parameters.AddWithValue("customer_id", (object)request.customer_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_email", (object)request.user_email ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_firstname", (object)request.user_firstname ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_lastname", (object)request.user_lastname ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_phone", (object)request.user_phone ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_address1", (object)request.user_address1 ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_address2", (object)request.user_address2 ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_city", (object)request.user_city ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_state", (object)request.user_state ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_zip", (object)request.user_zip ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_country", (object)request.user_country ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_crm_id", (object)request.user_crm_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_username", (object)request.user_username ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_password", (object)request.user_password ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_role", (object)request.user_role ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_photo_id", (object)request.user_photo_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_office_id", (object)request.customer_office_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching user found"
						});
					}

					cmd.CommandText = $"SELECT user_firstname, user_lastname FROM users{whereString}";
					var userFirstName = "";
					var userLastName = "";
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							userFirstName = _dbHelper.SafeGetString(reader, 0);
							userLastName = _dbHelper.SafeGetString(reader, 1);
						}
						else
						{
							return BadRequest(new
							{
								status = "No matching user found"
							});
						}
					}

					var userDisplayName = string.IsNullOrEmpty(userFirstName) || string.IsNullOrEmpty(userLastName)
							? $"{userLastName}{userFirstName}" : $"{userLastName}, {userFirstName}";
					cmd.CommandText = $"UPDATE users set user_displayname='{userDisplayName}'{whereString}";
					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "No matching user found"
						});
					}
					return Ok(new
					{
						status = "Completed"
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
				if (!isInternalRequest)
				{
					_dbHelper.CloseConnection();
				}
			}
		}


		[HttpPost]
		[Route("UpdateCustomer")]
		public IActionResult Post(CustomerUpdateRequest request)
		{
			try
			{
				// validation check
				if (request.search_customer_id == null && request.search_customer_crm_id == null)
				{
					return BadRequest(new
					{
						status = "please provide at least one search parameter"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					if (!string.IsNullOrEmpty(request.customer_domain))
					{
						var commandText = $"SELECT EXISTS (SELECT true from customers where customer_domain='{request.customer_domain}'";
						if (request.search_customer_id != null)
						{
							commandText += $" AND customer_id<>'{request.search_customer_id}')";
						}
						if (request.search_customer_crm_id != null)
						{
							commandText += $" AND customer_crm_id<>'{request.search_customer_crm_id}')";
						}
						cmd.CommandText = commandText;
						if ((bool)cmd.ExecuteScalar() == true)
						{
							return BadRequest(new { status = "Customer domain is duplicated." });
						}
					}

					var whereString = " WHERE ";

					if (request.search_customer_id != null)
					{
						whereString = $"{whereString}customer_id='{request.search_customer_id}' AND ";
					}
					if (request.search_customer_crm_id != null)
					{
						whereString = $"{whereString}customer_crm_id='{request.search_customer_crm_id}' AND ";
					}

					whereString = whereString.Remove(whereString.Length - 5);

					var queryString = "UPDATE customers SET "
						+ "customer_admin_user_id = COALESCE(@customer_admin_user_id, customer_admin_user_id), "
						+ "customer_name = COALESCE(@customer_name, customer_name), "
						+ "customer_phone = COALESCE(@customer_phone, customer_phone), "
						+ "customer_email = COALESCE(@customer_email, customer_email), "
						+ "customer_address1 = COALESCE(@customer_address1, customer_address1), "
						+ "customer_address2 = COALESCE(@customer_address2, customer_address2), "
						+ "customer_city = COALESCE(@customer_city, customer_city), "
						+ "customer_state = COALESCE(@customer_state, customer_state), "
						+ "customer_zip = COALESCE(@customer_zip, customer_zip), "
						+ "customer_country = COALESCE(@customer_country, customer_country), "
						+ "customer_service_area = COALESCE(@customer_service_area, customer_service_area), "
						+ "customer_photo_id = COALESCE(@customer_photo_id, customer_photo_id), "
						+ "customer_logo_id = COALESCE(@customer_logo_id, customer_logo_id), "
						+ "full_address = COALESCE(@full_address, full_address), "
						+ "customer_timezone = COALESCE(@customer_timezone, customer_timezone), "
						+ "customer_billing_id = COALESCE(@customer_billing_id, customer_billing_id), "
						+ "customer_duns_number = COALESCE(@customer_duns_number, customer_duns_number), "
						+ "customer_subscription_level = COALESCE(@customer_subscription_level, customer_subscription_level), "
						+ "customer_subscription_level_id = COALESCE(@customer_subscription_level_id, customer_subscription_level_id), "
						+ "customer_crm_id = COALESCE(@customer_crm_id, customer_crm_id), "
						+ "company_type = COALESCE(@company_type, company_type), "
						+ "company_website = COALESCE(@company_website, company_website), "
						+ "customer_domain = COALESCE(@customer_domain, customer_domain), "
						+ "record_source = COALESCE(@record_source, record_source), "
						+ "status = COALESCE(@status, status), edit_datetime = @edit_datetime" + whereString;

					cmd.CommandText = queryString;
					cmd.Parameters.AddWithValue("customer_admin_user_id", (object)request.customer_admin_user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_name", (object)request.customer_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_phone", (object)request.customer_phone ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_email", (object)request.customer_email ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_address1", (object)request.customer_address1 ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_address2", (object)request.customer_address2 ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_city", (object)request.customer_city ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_state", (object)request.customer_state ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_zip", (object)request.customer_zip ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_country", (object)request.customer_country ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_service_area", (object)request.customer_service_area ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_crm_id", (object)request.customer_crm_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_photo_id", (object)request.customer_photo_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_logo_id", (object)request.customer_logo_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("full_address", (object)request.full_address ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_timezone", (object)request.customer_timezone ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_billing_id", (object)request.customer_billing_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_duns_number", (object)request.customer_duns_number ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_subscription_level", (object)request.customer_subscription_level ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_subscription_level_id", (object)request.customer_subscription_level_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("company_type", (object)request.company_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue("company_website", (object)request.company_website ?? DBNull.Value);
					cmd.Parameters.AddWithValue("customer_domain", (object)request.customer_domain ?? DBNull.Value);
					cmd.Parameters.AddWithValue("record_source", (object)request.record_source ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					if (cmd.ExecuteNonQuery() == 0)
					{
						return BadRequest(new
						{
							status = "no matching customer found"
						});
					}


					// Find head office id
					cmd.CommandText = $"SELECT company_office_id FROM company_offices WHERE customer_id='{request.search_customer_id}' AND company_office_headoffice=TRUE";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var companyOfficeId = _dbHelper.SafeGetString(reader, 0);

							var companyOfficeUpdateResult = new CompanyOfficeManagementController().Post(new CompanyOfficeUpdateRequest()
							{
								company_office_admin_user_id = request.customer_admin_user_id,
								company_office_phone = request.customer_phone,
								company_office_address1 = request.customer_address1,
								company_office_address2 = request.customer_address2,
								company_office_city = request.customer_city,
								company_office_country = request.customer_country,
								company_office_service_area = request.customer_service_area,
								company_office_state = request.customer_state,
								company_office_timezone = request.customer_timezone,
								company_office_zip = request.customer_zip,
								company_office_headoffice = true,
								search_company_office_id = companyOfficeId,
							});

							if (companyOfficeUpdateResult is BadRequestObjectResult)
							{
								return companyOfficeUpdateResult;
							}
							else
							{
								return Ok(new
								{
									status = "completed"
								});
							}
						}
						else
						{
							return Ok(new
							{
								status = "completed"
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
		[Route("FindUsers")]
		public IActionResult Get(UsersFindRequest request)
		{
			try
			{
				var detailLevel = request.detail_level ?? "basic";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT users.user_id, users.customer_id, customers.customer_name, users.status, "
						+ "users.user_firstname, users.user_lastname, users.user_email, users.user_phone, "
						+ "users.user_role, users.user_address1, users.user_address2, users.user_city, "
						+ "users.user_state, users.user_zip, users.user_country, customers.customer_service_area, "
						+ "users.create_datetime, users.edit_datetime, users.user_photo_id, "
						+ "users.user_crm_id, users.create_user_id, users.edit_user_id, users.user_password, users.customer_office_id, "
						+ "users.user_displayname "
						+ "FROM users "
						+ "LEFT JOIN customers ON users.customer_id=customers.customer_id ";

					if (request.customer_id != null)
					{
						cmd.CommandText += $"WHERE users.customer_id='{request.customer_id}'";
					}

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>>();
						while (reader.Read())
						{
							var user = new Dictionary<string, string>
							{
								{ "user_id", _dbHelper.SafeGetString(reader, 0) },
								{ "customer_id", _dbHelper.SafeGetString(reader, 1) },
								{ "customer_name", _dbHelper.SafeGetString(reader, 2) },
								{ "status", _dbHelper.SafeGetString(reader, 3) },
								{ "user_firstname", _dbHelper.SafeGetString(reader, 4) },
								{ "user_lastname", _dbHelper.SafeGetString(reader, 5) },
								{ "user_displayname", _dbHelper.SafeGetString(reader, 24) },
								{ "user_email", _dbHelper.SafeGetString(reader, 6) },
								{ "user_phone", _dbHelper.SafeGetString(reader, 7) },
								{ "user_role", _dbHelper.SafeGetString(reader, 8) },
								{ "has_password", _dbHelper.SafeGetString(reader, 22) == string.Empty ? "no" : "yes" }
							};

							if (detailLevel == "all" || detailLevel == "admin")
							{
								user["user_address1"] = _dbHelper.SafeGetString(reader, 9);
								user["user_address2"] = _dbHelper.SafeGetString(reader, 10);
								user["user_city"] = _dbHelper.SafeGetString(reader, 11);
								user["user_state"] = _dbHelper.SafeGetString(reader, 12);
								user["user_zip"] = _dbHelper.SafeGetString(reader, 13);
								user["user_country"] = _dbHelper.SafeGetString(reader, 14);
								user["user_service_area"] = _dbHelper.SafeGetString(reader, 15);
								user["create_datetime"] = ((DateTime)reader.GetValue(16)).ToString();
								user["edit_datetime"] = ((DateTime)reader.GetValue(17)).ToString();
								user["user_photo_id"] = _dbHelper.SafeGetString(reader, 18);
								user["customer_office_id"] = _dbHelper.SafeGetString(reader, 23);
							}

							if (detailLevel == "admin")
							{
								user["user_crm_id"] = _dbHelper.SafeGetString(reader, 19);
								user["create_user_id"] = _dbHelper.SafeGetString(reader, 20);
								user["edit_user_id"] = _dbHelper.SafeGetString(reader, 21);
							}

							result.Add(user);
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


		[HttpGet]
		[Route("FindCustomers")]
		public IActionResult Get(CustomerFindRequest request)
		{
			try
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = " WHERE company_offices.company_office_headoffice=TRUE AND ";

					if (request.company_type != null)
					{
						where += $"customers.company_type='{request.company_type}' AND ";
					}
					if (request.customer_domain != null)
					{
						where += $"customers.customer_domain='{request.customer_domain}' AND ";
					}
					if (request.customer_name != null)
					{
						where += $"customers.customer_name='{request.customer_name}' AND ";
					}
					if (request.customer_service_area != null)
					{
						where += $"company_offices.company_office_service_area='{request.customer_service_area}' AND ";
					}
					if (request.record_source != null)
					{
						where += $"customers.record_source='{request.record_source}' AND ";
					}
					if (request.customer_state != null)
					{
						where += $"company_offices.company_office_state='{request.customer_state}' AND ";
					}
					if (request.customer_zip != null)
					{
						where += $"company_offices.company_office_zip='{request.customer_zip}' AND ";
					}
					if (request.status != null)
					{
						where += $"customers.status='{request.status}' AND ";
					}

					where = where.Remove(where.Length - 5);

					cmd.CommandText = "SELECT customers.company_type, customers.customer_id, customers.customer_name, "
									+ "customers.customer_email, company_offices.company_office_city, company_offices.company_office_phone, "
									+ "company_offices.company_office_state, customers.status, "
									+ "customers.company_website, customers.customer_domain, customers.create_datetime, company_offices.company_office_address1, "
									+ "company_offices.company_office_address2, company_offices.company_office_country, customers.customer_duns_number, "
									+ "company_offices.company_office_service_area, customers.customer_photo_id, customers.record_source, customers.customer_timezone, "
									+ "customers.edit_datetime, "
									+ "customers.customer_crm_id, customers.create_user_id, customers.edit_user_id, users.user_firstname, users.user_lastname, users.user_email "
									+ "FROM customers LEFT JOIN company_offices ON customers.customer_id=company_offices.customer_id "
									+ "LEFT JOIN users ON customers.customer_admin_user_id=users.user_id "
									+ where;

					using (var reader = cmd.ExecuteReader())
					{
						var resultList = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							var result = new Dictionary<string, string> { };

							result.Add("company_type", _dbHelper.SafeGetString(reader, 0));
							result.Add("customer_id", _dbHelper.SafeGetString(reader, 1));
							result.Add("customer_name", _dbHelper.SafeGetString(reader, 2));
							result.Add("customer_email", _dbHelper.SafeGetString(reader, 3));
							result.Add("customer_city", _dbHelper.SafeGetString(reader, 4));
							result.Add("customer_phone", _dbHelper.SafeGetString(reader, 5));
							result.Add("customer_state", _dbHelper.SafeGetString(reader, 6));
							result.Add("status", _dbHelper.SafeGetString(reader, 7));
							result.Add("customer_admin_fullname", $"{_dbHelper.SafeGetString(reader, 23)} {_dbHelper.SafeGetString(reader, 24)}");
							result.Add("customer_admin_email", _dbHelper.SafeGetString(reader, 25));

							if (request.detail_level == "admin" || request.detail_level == "all")
							{
								result.Add("company_website", _dbHelper.SafeGetString(reader, 8));
								result.Add("customer_domain", _dbHelper.SafeGetString(reader, 9));
								result.Add("create_datetime", _dbHelper.SafeGetDatetimeString(reader, 10));
								result.Add("customer_address1", _dbHelper.SafeGetString(reader, 11));
								result.Add("customer_address2", _dbHelper.SafeGetString(reader, 12));
								result.Add("customer_country", _dbHelper.SafeGetString(reader, 13));
								result.Add("customer_duns_number", _dbHelper.SafeGetString(reader, 14));
								result.Add("customer_service_area", _dbHelper.SafeGetString(reader, 15));
								result.Add("customer_photo_id", _dbHelper.SafeGetString(reader, 16));
								result.Add("record_source", _dbHelper.SafeGetString(reader, 17));
								result.Add("customer_timezone", _dbHelper.SafeGetString(reader, 18));
								result.Add("edit_datetime", _dbHelper.SafeGetDatetimeString(reader, 19));
							}

							if (request.detail_level == "admin")
							{
								result.Add("customer_crm_id", _dbHelper.SafeGetString(reader, 20));
								result.Add("create_user_id", _dbHelper.SafeGetString(reader, 21));
								result.Add("edit_user_id", _dbHelper.SafeGetString(reader, 22));
							}

							resultList.Add(result);
						}
						return Ok(resultList);
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


		[HttpPost]
		[Route("RemoveCustomerUser")]
		public IActionResult Post(UserRemoveRequest request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[] { "customer_id", "user_id" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				var userId = request.user_id;
				var customerId = request.customer_id;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"UPDATE users SET customer_id = NULL WHERE user_id='{userId}' AND customer_id='{customerId}'";

					if (cmd.ExecuteNonQuery() > 0)
					{
						return Ok(new
						{
							status = "User has been removed"
						});
					}
					else
					{
						return BadRequest(new
						{
							status = "User doesn't exist or customer_id is incorrect"
						});
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


		private bool __checkSysAdminExists(string customer_id)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT EXISTS (SELECT true FROM users WHERE customer_id='{customer_id}' AND user_role='sys admin')";
				return (bool)cmd.ExecuteScalar();
			}
		}

		private string __getCustomerPortalUrl()
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT setting_value FROM system_settings WHERE system_setting_id='CUSTOMER_PORTAL_URL'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return _dbHelper.SafeGetString(reader, 0);
					}
					else
					{
						return "";
					}
				}
			}
		}
	}
}
