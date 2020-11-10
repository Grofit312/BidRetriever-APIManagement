using System;
using System.Collections.Generic;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Stripe;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Billing Management")]
	public class BillingManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		private readonly IAmazonSimpleEmailService sesClient;

		public BillingManagementController(IAmazonSimpleEmailService sesClient)
		{
			_dbHelper = new DatabaseHelper();

			this.sesClient = sesClient;
		}

		[HttpGet]
		[Route("FindSystemProducts")]
		public IActionResult Post(SystemProductFindRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// get all products
				var service = new ProductService(stripeApiKey);
				var options = new ProductListOptions
				{
					Limit = 10,
				};
				var products = service.List(options);

				var filteredProducts = request.product_id == null ? products.Data : products.Data.FindAll((product) =>
				{
					return product.Id == request.product_id;
				});

				var result = new List<Dictionary<string, string>> { };

				filteredProducts.ForEach(product =>
				{
					result.Add(new Dictionary<string, string>
					{
						{ "product_id", product.Id },
						{ "product_name", product.Name },
						{ "product_desc", product.Description },
						{ "product_status", (product.Active ?? true) ? "active" : "inactive" },
						{ "create_datetime", DateTimeHelper.GetDateTimeString(product.Created) },
						{ "edit_datetime", DateTimeHelper.GetDateTimeString(product.Updated) },
					});
				});
				return Ok(result);
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
		[Route("CreateCustomerSubscription")]
		public IActionResult Post(SubscriptionCreateRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"customer_id",
					"source_token",
					"user_email",
					"core_product_id",
					"license_product_id",
					"license_count"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create stripe customer
				var customerCreateOptions = new CustomerCreateOptions
				{
					Email = request.user_email,
					SourceToken = request.source_token,
				};
				var customerService = new CustomerService(stripeApiKey);
				var customer = customerService.Create(customerCreateOptions);

				// update customer_billing_id
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"UPDATE customers SET customer_billing_id='{customer.Id}' WHERE customer_id='" + request.customer_id + "'";
					cmd.Parameters.AddWithValue("customer_billing_id", customer.Id);

					var rowsAffected = cmd.ExecuteNonQuery();

					if (rowsAffected == 0)
					{
						return BadRequest(new { status = "Failed to write customer id into database" });
					}
				}

				// create subscription
				var planService = new PlanService(stripeApiKey);
				var planListOptions = new PlanListOptions
				{
					Limit = 1,
					ProductId = request.core_product_id,
				};
				var plans = planService.List(planListOptions);

				if (plans.Data.Count == 0)
				{
					return BadRequest(new
					{
						status = "Failed to find associated plan for the product"
					});
				}

				var coreSubscriptionPlan = plans.Data[0];

				planListOptions = new PlanListOptions
				{
					Limit = 1,
					ProductId = request.license_product_id,
				};

				plans = planService.List(planListOptions);

				if (plans.Data.Count == 0)
				{
					return BadRequest(new
					{
						status = "Failed to find associated plan for the product"
					});
				}

				var licenseSubscriptionPlan = plans.Data[0];

				var subscriptionService = new SubscriptionService(stripeApiKey);

				var subscription = subscriptionService.Create(new SubscriptionCreateOptions
				{
					CustomerId = customer.Id,
					Items = new List<SubscriptionItemOption>
					{
						new SubscriptionItemOption
						{
							PlanId = coreSubscriptionPlan.Id
						}
					},
				});

				if (subscription.Status == "incomplete")
				{
					return BadRequest(new
					{
						status = "Failed to charge"
					});
				}

				for (var index = 0; index < request.license_count; index++)
				{
					subscription = subscriptionService.Create(new SubscriptionCreateOptions
					{
						CustomerId = customer.Id,
						Items = new List<SubscriptionItemOption>
						{
							new SubscriptionItemOption
							{
								PlanId = licenseSubscriptionPlan.Id
							}
						},
					});

					if (subscription.Status == "incomplete")
					{
						return BadRequest(new
						{
							status = "Failed to charge"
						});
					}
				}

				return Ok(new
				{
					status = "complete",
					subscription_id = subscription.Id
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
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("UpdateBillingInfo")]
		public IActionResult Post(BillingUpdateRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"customer_id", "source_token"
				});

				if (missingParameter != null)
				{
					return BadRequest(new { status = $"{missingParameter} is required" });
				}

				var customerId = request.customer_id;
				var sourceToken = request.source_token;

				// retrieve current stripe customer id
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT customer_billing_id FROM customers WHERE customer_id='{customerId}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var billingId = _dbHelper.SafeGetString(reader, 0);

							var options = new CustomerUpdateOptions
							{
								SourceToken = sourceToken,
							};

							var service = new CustomerService(stripeApiKey);
							service.Update(billingId, options);

							return Ok(new
							{
								status = "Updated billing info"
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "Not a registered billing customer"
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
		[Route("UpdateCustomerSubscription")]
		public IActionResult Post(SubscriptionUpdateRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"customer_id", "core_product_id", "license_product_id", "license_count"
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// get customer's stripe customer id
				var stripeCustomerID = "";

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT customer_billing_id FROM customers where customer_id='" + request.customer_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							stripeCustomerID = _dbHelper.SafeGetString(reader, 0);
						}
					}
				}

				if (string.IsNullOrEmpty(stripeCustomerID))
				{
					return BadRequest(new
					{
						status = "Cannot find billing customer"
					});
				}

				// update subscription
				var planService = new PlanService(stripeApiKey);
				var planListOptions = new PlanListOptions
				{
					Limit = 1,
					ProductId = request.core_product_id,
				};
				var plans = planService.List(planListOptions);

				if (plans.Data.Count == 0)
				{
					return BadRequest(new
					{
						status = "Failed to find associated plan for the product"
					});
				}

				var coreSubscriptionPlan = plans.Data[0];

				planListOptions = new PlanListOptions
				{
					Limit = 1,
					ProductId = request.license_product_id,
				};

				plans = planService.List(planListOptions);

				if (plans.Data.Count == 0)
				{
					return BadRequest(new { status = "Failed to find associated plan for the product" });
				}

				var licenseSubscriptionPlan = plans.Data[0];

				var subscriptionService = new SubscriptionService(stripeApiKey);

				var subscriptionListOptions = new SubscriptionListOptions
				{
					Limit = 100,
					CustomerId = stripeCustomerID,
				};
				var subscriptions = subscriptionService.List(subscriptionListOptions).Data;
				var licenseSubscriptions = subscriptions.FindAll(subscription =>
				{
					return subscription.Plan.ProductId == request.license_product_id;
				});
				var coreSubscription = subscriptions.Find(subscription =>
				{
					return subscription.Plan.ProductId != request.license_product_id;
				});

				if (coreSubscription == null)
				{
					return BadRequest(new { status = "Cannot find core subscription" });
				}

				if (coreSubscription.Plan.ProductId != request.core_product_id)
				{
					var subscriptionUpdateOptions = new SubscriptionUpdateOptions
					{

						Items = new List<SubscriptionItemUpdateOption>
						{
							new SubscriptionItemUpdateOption
							{
								Id = coreSubscription.Items.Data[0].Id,
								PlanId = coreSubscriptionPlan.Id
							}
						},
						Prorate = false,
					};

					subscriptionService.Update(coreSubscription.Id, subscriptionUpdateOptions);
				}

				if (licenseSubscriptions.Count > request.license_count)
				{
					var deleteCount = licenseSubscriptions.Count - request.license_count;

					for (var index = 0; index < deleteCount; index++)
					{
						subscriptionService.Cancel(licenseSubscriptions[index].Id, new SubscriptionCancelOptions
						{
							InvoiceNow = false,
							Prorate = false,
						});
					}
				}
				else if (licenseSubscriptions.Count < request.license_count)
				{
					var createCount = request.license_count - licenseSubscriptions.Count;

					for (var index = 0; index < createCount; index++)
					{
						subscriptionService.Create(new SubscriptionCreateOptions
						{
							CustomerId = stripeCustomerID,
							Items = new List<SubscriptionItemOption>
							{
								new SubscriptionItemOption
								{
									PlanId = licenseSubscriptionPlan.Id
								}
							},
						});
					}
				}

				return Ok(new
				{
					status = "completed"
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
				_dbHelper.CloseConnection();
			}
		}


		[HttpGet]
		[Route("GetCustomerSubscription")]
		public IActionResult Get(SubscriptionGetRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// validation check
				if (request.customer_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide customer id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT customer_billing_id FROM customers WHERE customer_id='" + request.customer_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var customerBillingId = _dbHelper.SafeGetString(reader, 0);

							if (customerBillingId == string.Empty)
							{
								return BadRequest(new
								{
									status = "This is not billing customer"
								});
							}
							else
							{
								var subscriptionService = new SubscriptionService(stripeApiKey);
								var response = subscriptionService.List(new SubscriptionListOptions
								{
									Limit = 100,
									CustomerId = customerBillingId,
								});

								if (response.Data.Count == 0)
								{
									return BadRequest(new
									{
										status = "Customer is not subscribed to any plan yet"
									});
								}
								else
								{
									var subscriptions = new List<Dictionary<string, object>> { };

									response.Data.ForEach(subscription =>
									{
										subscriptions.Add(new Dictionary<string, object>
										{
											{ "customer_id", request.customer_id },
											{ "subscription_id", subscription.Id },
											{ "subscription_plan_id", subscription.Plan.Id },
											{ "subscription_product_id", subscription.Plan.ProductId },
											{ "subscription_amount", subscription.Quantity ?? 0 },
											{ "subscription_status", subscription.Status },
											{ "subscription_period_start", subscription.CurrentPeriodStart.HasValue ? DateTimeHelper.GetDateTimeString(subscription.CurrentPeriodStart.Value) : "" },
											{ "subscription_period_end", subscription.CurrentPeriodEnd.HasValue ? DateTimeHelper.GetDateTimeString(subscription.CurrentPeriodEnd.Value) : "" },
											{ "create_datetime", subscription.Created.HasValue ? DateTimeHelper.GetDateTimeString(subscription.Created.Value) : "" },
										});
									});
									return Ok(subscriptions);
								}
							}
						}
						else
						{
							return BadRequest(new { statuscode = StatusCodes.Status204NoContent, message = "Customer not found!" });
						}
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new { status = exception.Message });
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("RemoveCustomerSubscription")]
		public IActionResult Post(SubscriptionRemoveRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// validation check
				if (request.customer_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide customer_id"
					});
				}

				var customerId = request.customer_id;

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT customer_billing_id FROM customers WHERE customer_id='{customerId}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var customerBillingId = _dbHelper.SafeGetString(reader, 0);

							if (customerBillingId == string.Empty)
							{
								return BadRequest(new { status = "This is unsubscribed customer" });
							}
							else
							{
								// remove customer
								var service = new CustomerService(stripeApiKey);
								service.Delete(customerBillingId);

								// set null to customer_billing_id
								reader.Close();

								cmd.CommandText = "UPDATE customers SET customer_billing_id=NULL WHERE customer_id='" + customerId + "'";

								var rowsAffected = cmd.ExecuteNonQuery();

								if (rowsAffected == 0)
								{
									return BadRequest(new
									{
										status = "Failed to nullify the billing id field"
									});
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
						else
						{
							return BadRequest(new
							{
								status = "Customer not found"
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
		[Route("GetCardInfo")]
		public IActionResult Get(BillingInfoGetRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// validation check
				if (request.customer_id == null)
				{
					return BadRequest(new
					{
						status = "Please provide customer id"
					});
				}

				// retrieve card info
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT customer_billing_id FROM customers WHERE customer_id='" + request.customer_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var customerBillingId = _dbHelper.SafeGetString(reader, 0);

							if (customerBillingId == string.Empty)
							{
								return BadRequest(new { status = "Payment method not connected" });
							}
							else
							{
								var service = new CardService(stripeApiKey);
								var options = new CardListOptions
								{
									Limit = 1,
								};
								var cards = service.List(customerBillingId, options);
								var currentCard = cards.Data[0];

								return Ok(new
								{
									card_number = $"xxxx-xxxx-xxxx-{currentCard.Last4}",
									card_expiration_date = $"{currentCard.ExpMonth}/{currentCard.ExpYear}",
									cvc_number = "xxx",
								});
							}
						}
						else
						{
							return BadRequest(new
							{
								status = "Customer not found"
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
		[Route("ChargeCustomers")]
		public IActionResult Post(BillingChargeRequest request)
		{
			try
			{
				// retrieve stripe key
				var stripeApiKey = __getStripeApiKey();

				if (string.IsNullOrEmpty(stripeApiKey))
				{
					return BadRequest(new
					{
						status = "Stripe secret key is not defined"
					});
				}

				// validation check
				if (request.billable_month == null)
				{
					return BadRequest(new
					{
						status = "Please provide month"
					});
				}

				// retrieve billing list of the month
				var billingList = __getBillingList(request.billable_month);

				var invoiceList = new List<Invoice> { };
				var invoiceService = new InvoiceService(stripeApiKey);
				var invoiceItemService = new InvoiceItemService(stripeApiKey);

				// cumulate invoice items for each customer
				billingList.ForEach((billingItem) =>
				{
					// create invoice item
					var invoiceItemOptions = new InvoiceItemCreateOptions
					{
						Amount = (long)(billingItem.project_cost * 100),
						Currency = "usd",
						CustomerId = billingItem.customer_billing_id,
						Description = $"Project <{billingItem.project_name}> ({billingItem.project_size})",
					};

					InvoiceItem invoiceItem = invoiceItemService.Create(invoiceItemOptions);

					// create invoice if it is last invoice item for the customer
					var lastBillingItem = billingList.FindLast((item) =>
					{
						return item.customer_id == billingItem.customer_id;
					});

					if (billingItem == lastBillingItem)
					{
						var invoiceCreateOptions = new InvoiceCreateOptions
						{
							CustomerId = billingItem.customer_billing_id,
							Description = "Invoice from Bid Retriever",
						};
						Invoice invoice = invoiceService.Create(invoiceCreateOptions);
						invoice.Description = billingItem.user_email;
						invoiceList.Add(invoice);
					}
				});

				// pay invoices and send email to admins
				invoiceList.ForEach(async invoice =>
				{
					// pay invoice
					var invoicePayOptions = new InvoicePayOptions { };
					var paidInvoice = invoiceService.Pay(invoice.Id, invoicePayOptions);

					// send email
					await MailSender.SendEmailAsync(sesClient, new List<string> { invoice.Description }, "Paid Invoice", $"Download invoice <a href='{paidInvoice.InvoicePdf}' target='_blank'>here</a>.");
				});

				return Ok(new
				{
					status = $"{invoiceList.Count} invoice(s) paid"
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
				_dbHelper.CloseConnection();
			}
		}


		/**
		 * Retrieve stripe secret key from system settings table
		 * 
		 */
		private string __getStripeApiKey()
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT setting_value FROM system_settings WHERE system_setting_id='STRIPE_SECRET_KEY'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var apiKey = _dbHelper.SafeGetString(reader, 0);
						return apiKey;
					}
					else
					{
						return null;
					}
				}
			}
		}


		/**
		 * 
		 * Retrieve list of billing items for the specific month (exclude trial users)
		 * 
		 */
		private List<BillingItem> __getBillingList(string billable_month)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT customers.customer_id, customers.customer_billing_id, users.user_email,"
												+ "to_char(project_submissions.create_datetime, 'MM/YYYY'::text) AS billable_month,"
												+ "projects.project_name, count(project_documents.doc_id) AS total_new_documents,"
												+ "CASE WHEN(count(project_documents.doc_id) >= 400) THEN 'Jumbo Project'::text "
												+ "WHEN((count(project_documents.doc_id) < 400) AND(count(project_documents.doc_id) >= 150)) THEN 'Large Project'::text "
												+ "WHEN((count(project_documents.doc_id) < 150) AND(count(project_documents.doc_id) >= 50)) THEN 'Basic Project'::text "
												+ "ELSE 'Small Project'::text "
												+ "END AS project_size,"
												+ "CASE WHEN(count(project_documents.doc_id) >= 400) THEN 15.00 "
												+ "WHEN((count(project_documents.doc_id) < 400) AND(count(project_documents.doc_id) >= 150)) THEN 10.00 "
												+ "WHEN((count(project_documents.doc_id) < 150) AND(count(project_documents.doc_id) >= 50)) THEN 5.00 "
												+ "ELSE 2.50 "
												+ "END AS project_cost "
												+ "FROM(((projects "
												+ "LEFT JOIN project_submissions ON((project_submissions.project_id = projects.project_id))) "
												+ "LEFT JOIN project_documents ON((project_documents.submission_id = project_submissions.project_submission_id))) "
												+ "RIGHT JOIN users ON((projects.project_admin_user_id = users.user_id)) "
												+ "RIGHT JOIN customers ON((users.customer_id = customers.customer_id))) "
												+ $"WHERE to_char(project_submissions.create_datetime, 'MM/YYYY'::text) = '{billable_month}' AND customer_billing_id IS NOT NULL "
												+ "GROUP BY customers.customer_id, customers.customer_billing_id, users.user_email, projects.project_name, project_submissions.submitter_email, (to_char(project_submissions.create_datetime, 'MM/YYYY'::text)) "
												+ "ORDER BY users.user_email, projects.project_name ";

				using (var reader = cmd.ExecuteReader())
				{
					var result = new List<BillingItem> { };

					while (reader.Read())
					{
						result.Add(new BillingItem
						{
							customer_id = _dbHelper.SafeGetString(reader, 0),
							customer_billing_id = _dbHelper.SafeGetString(reader, 1),
							user_email = _dbHelper.SafeGetString(reader, 2),
							billable_month = _dbHelper.SafeGetString(reader, 3),
							project_name = _dbHelper.SafeGetString(reader, 4),
							total_new_documents = _dbHelper.SafeGetIntegerRaw(reader, 5),
							project_size = _dbHelper.SafeGetString(reader, 6),
							project_cost = _dbHelper.SafeGetDoubleRaw(reader, 7),
						});
					}

					return result;
				}
			}
		}
	}
}
