using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace _440DocumentManagement.Helpers
{
	public class JWT
	{
		private static readonly SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.JWT_SECRET_KEY));
		private static readonly SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
		private static readonly JwtHeader header = new JwtHeader(credentials);

		static public string CreateTokenStringFromUserId(string userId)
		{
			try
			{
				var secToken = new JwtSecurityToken(header, new JwtPayload
				{
					{ "user_id", userId }
				});
				var handler = new JwtSecurityTokenHandler();

				return handler.WriteToken(secToken);
			}
			catch (Exception)
			{
				return null;
			}
		}

		static public string GetParameterFromToken(string tokenString, string parameter)
		{
			try
			{
				var handler = new JwtSecurityTokenHandler();
				var token = handler.ReadJwtToken(tokenString);

				return (string)(token.Payload[parameter]);
			}
			catch (Exception)
			{
				return null;
			}
		}

		static public string CreateExpiryTokenStringFromUserId(string userId)
		{
			try
			{
				var secToken = new JwtSecurityToken(header, new JwtPayload
				{
					{ "user_id", userId },
					{ "timestamp", new DateTime() }
				});
				var handler = new JwtSecurityTokenHandler();

				return handler.WriteToken(secToken);
			}
			catch (Exception)
			{
				return null;
			}
		}

		static public string CreateCompanyChangeTokenString(string userId, string customerId)
		{
			try
			{
				var secToken = new JwtSecurityToken(header, new JwtPayload
				{
					{ "user_id", userId },
					{ "customer_id", customerId }
				});
				var handler = new JwtSecurityTokenHandler();

				return handler.WriteToken(secToken);
			}
			catch (Exception)
			{
				return null;
			}
		}

		static public bool CheckTokenExpiration(string tokenString)
		{
			try
			{
				var handler = new JwtSecurityTokenHandler();
				var token = handler.ReadJwtToken(tokenString);
				var timestamp = (DateTime)(token.Payload["timestamp"]);

				return (new DateTime() - timestamp).TotalHours >= Constants.TOKEN_LIFETIME;
			}
			catch (Exception)
			{
				return true;
			}
		}
	}
}
