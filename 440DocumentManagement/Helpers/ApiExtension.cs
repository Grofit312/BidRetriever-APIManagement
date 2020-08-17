using _440DocumentManagement.Models.ApiDatabase;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace _440DocumentManagement.Helpers
{
	public static class ApiExtension
	{
		public static int UNDEFINED_INT = int.MinValue;
		public static short UNDEFINED_SHORT = short.MinValue;
		public static string UNDEFINED_STRING = " ";
		public static DateTime UNDEFINED_DATETIME = default(DateTime);

		public static void AddWithValue<T>(this IDbCommand command, string name, T value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.Value = value;
			command.Parameters.Add(parameter);
		}

		public static string GetDateTimeString(object o)
		{
			try
			{
				return DateTimeHelper.GetDateTimeString(Convert.ToDateTime(o));
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string GetString(object o)
		{
			try
			{
				return Convert.ToString(o);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static string FilterDateRange(string filterSql, NpgsqlCommand cmd)
		{
			if (filterSql.ToLower().Contains("=@today"))
			{
				filterSql = Regex.Replace(filterSql, "=@today",
					" BETWEEN SYMMETRIC DATE_TRUNC('day', CURRENT_DATE) AND DATE_TRUNC('day', CURRENT_DATE + interval '1 day') - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("@today"))
			{
				filterSql = Regex.Replace(filterSql, "@today",
					"DATE_TRUNC('day', CURRENT_DATE)",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@tomorrow"))
			{
				filterSql = Regex.Replace(filterSql, "=@tomorrow",
					" BETWEEN SYMMETRIC DATE_TRUNC('day', CURRENT_DATE + interval '1 day') AND DATE_TRUNC('day', CURRENT_DATE + interval '2 day') - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("@tomorrow"))
			{
				filterSql = Regex.Replace(filterSql, "@tomorrow",
					"DATE_TRUNC('day', CURRENT_DATE + interval '1 day')",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@yesterday"))
			{
				filterSql = Regex.Replace(filterSql, "=@yesterday",
					" BETWEEN SYMMETRIC DATE_TRUNC('day', CURRENT_DATE - interval '1 day') AND DATE_TRUNC('day', CURRENT_DATE) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("@yesterday"))
			{
				filterSql = Regex.Replace(filterSql, "@yesterday",
					"DATE_TRUNC('day', CURRENT_DATE - interval '1 day')",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@thisweek"))
			{
				filterSql = Regex.Replace(filterSql, "=@thisweek",
					" BETWEEN SYMMETRIC NOW()::DATE-EXTRACT(DOW FROM NOW())::INTEGER AND NOW()::DATE-EXTRACT(DOW FROM NOW())::INTEGER+7 - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@thismonth"))
			{
				filterSql = Regex.Replace(filterSql, "=@thismonth",
					" BETWEEN SYMMETRIC DATE_TRUNC('month', CURRENT_DATE) AND DATE_TRUNC('month', CURRENT_DATE + interval '1' month) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@thisyear"))
			{
				filterSql = Regex.Replace(filterSql, "=@thisyear",
					" BETWEEN SYMMETRIC DATE_TRUNC('year', CURRENT_DATE) AND DATE_TRUNC('year', CURRENT_DATE + interval '1' year) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@thisquarter"))
			{
				filterSql = Regex.Replace(filterSql, "=@thisquarter",
					" BETWEEN SYMMETRIC DATE_TRUNC('quarter', CURRENT_DATE) AND DATE_TRUNC('quarter', CURRENT_DATE + interval '3' month) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@lastweek"))
			{
				filterSql = Regex.Replace(filterSql, "=@lastweek",
					" BETWEEN SYMMETRIC NOW()::DATE-EXTRACT(DOW FROM NOW())::INTEGER-7 AND NOW()::DATE-EXTRACT(DOW FROM NOW())::INTEGER - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@lastmonth"))
			{
				filterSql = Regex.Replace(filterSql, "=@lastmonth",
					" BETWEEN SYMMETRIC DATE_TRUNC('month', CURRENT_DATE - interval '1' month) AND DATE_TRUNC('month', CURRENT_DATE) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@lastyear"))
			{
				filterSql = Regex.Replace(filterSql, "=@lastyear",
					" BETWEEN SYMMETRIC DATE_TRUNC('year', CURRENT_DATE - interval '1' year) AND DATE_TRUNC('year', CURRENT_DATE) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}
			if (filterSql.ToLower().Contains("=@lastquarter"))
			{
				filterSql = Regex.Replace(filterSql, "=@lastquarter",
					" BETWEEN SYMMETRIC DATE_TRUNC('quarter', CURRENT_DATE - interval '3' month) AND DATE_TRUNC('quarter', CURRENT_DATE) - interval '1 microsecond'",
					RegexOptions.IgnoreCase);
			}

			// LastXDays
			var lastXDaysMatchGroup = new Regex("=@last(.*)days").Match(filterSql.ToLower()).Groups;
			if (lastXDaysMatchGroup.Count > 1)
			{
				int lastDays = 0;
				if (Int32.TryParse(lastXDaysMatchGroup[1].ToString(), out lastDays))
				{
					filterSql = Regex.Replace(filterSql, "=@last(.*)days",
						$" BETWEEN SYMMETRIC DATE_TRUNC('day', CURRENT_DATE - interval '{lastDays - 1}' day) AND DATE_TRUNC('day', CURRENT_DATE + interval '1 day') - interval '1 microsecond'",
						RegexOptions.IgnoreCase);
				}
				else
				{
					filterSql = Regex.Replace(filterSql, "=@last(.*)days", "(true)", RegexOptions.IgnoreCase);
				}
			}
			// NextXDays
			var nextXDaysMatchGroup = new Regex("=@next(.*)days").Match(filterSql.ToLower()).Groups;
			if (nextXDaysMatchGroup.Count > 1)
			{
				int nextDays = 0;
				if (Int32.TryParse(nextXDaysMatchGroup[1].ToString(), out nextDays))
				{
					filterSql = Regex.Replace(filterSql, "=@next(.*)days",
						 $" BETWEEN SYMMETRIC DATE_TRUNC('day', CURRENT_DATE) AND DATE_TRUNC('day', CURRENT_DATE + interval '{nextDays}' day) - interval '1 microsecond'",
						 RegexOptions.IgnoreCase);
				}
				else
				{
					filterSql = Regex.Replace(filterSql, "=@next(.*)days", "(true)", RegexOptions.IgnoreCase);
				}
			}
			return filterSql;
		}
	}
}
