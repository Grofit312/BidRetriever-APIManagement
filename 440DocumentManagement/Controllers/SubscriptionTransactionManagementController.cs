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
	public class SubscriptionTransactionManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;

		public SubscriptionTransactionManagementController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("AddSubscriptionTransaction")]
		public IActionResult Post(SubscriptionTransaction transaction)
		{
			try
			{
				// check missing parameter
				var missingParameter = transaction.CheckRequiredParameters(new string[]
				{
					"subscription_id",
					"transaction_type",
					"transaction_amount",
					"transaction_datetime",
					"transaction_status",
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create transaction
				var transactionId = transaction.transaction_id ?? Guid.NewGuid().ToString();

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "INSERT INTO subscription_transactions (subscription_id, transaction_id, "
													+ "transaction_datetime, transaction_type, transaction_amount, transaction_status) "
													+ "VALUES(@subscription_id, @transaction_id, @transaction_datetime, @transaction_type, @transaction_amount, @transaction_status)";

					cmd.Parameters.AddWithValue("subscription_id", transaction.subscription_id);
					cmd.Parameters.AddWithValue("transaction_id", transactionId);
					cmd.Parameters.AddWithValue("transaction_datetime", DateTimeHelper.ConvertToUTCDateTime(transaction.transaction_datetime));
					cmd.Parameters.AddWithValue("transaction_type", transaction.transaction_type);
					cmd.Parameters.AddWithValue("transaction_amount", transaction.transaction_amount);
					cmd.Parameters.AddWithValue("transaction_status", transaction.transaction_status);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = "completed",
						transaction_id = transactionId
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
	}
}
