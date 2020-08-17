using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Helpers
{
	public class MailSender
	{
		public static async Task SendEmailAsync(IAmazonSimpleEmailService sesClient, List<string> toAddresses, string subject, string body, string fromAddress = "")
		{
			var sendRequest = new SendEmailRequest
			{
				Source = string.IsNullOrEmpty(fromAddress) ? $"{Constants.BID_RETRIEVER_EMAIL_ADDRESS}" : fromAddress,
				Destination = new Destination
				{
					ToAddresses = toAddresses,
				},
				Message = new Message
				{
					Subject = new Content(subject),
					Body = new Body
					{
						Html = new Content
						{
							Charset = "UTF-8",
							Data = body,
						},
					}
				},
			};

			await sesClient.SendEmailAsync(sendRequest);
		}
	}
}
