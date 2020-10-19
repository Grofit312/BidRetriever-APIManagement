using System;
using TimeZoneConverter;

namespace _440DocumentManagement.Helpers
{
	public class DateTimeHelper
	{
		static public DateTime ConvertToUTCDateTime(string originTimestamp)
		{
			try
			{
				var timestamp = DateTime.Parse(originTimestamp).ToUniversalTime();
				return timestamp;
			}
			catch (Exception)
			{
				return DateTime.UtcNow;
			}
		}

		static public TimeSpan ConvertToTimeSpan(string originTimestamp)
		{
			try
			{
				var timestamp = TimeSpan.Parse(originTimestamp);
				return timestamp;
			}
			catch (Exception)
			{
				return new TimeSpan(0, 0, 0);
			}
		}

		static public string GetDateTimeString(DateTime time)
		{
			return time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.ffffffZ");
		}

		static public string GetFormattedTimestamp(string originTimestamp, string format)
		{
			try
			{
				var timestamp = DateTime.Parse(originTimestamp).ToString(format);
				return timestamp;
			}
			catch (Exception)
			{
				return "";
			}
		}

        /**
         * Convert timestamp to specific timezone's DateTime
         */
        static public DateTime ConvertToUserTimezone(string formattedDateString, string userTimezone)
        {
            var utcDatetime = DateTime.Parse(formattedDateString).ToUniversalTime();
            var timezoneInfo = TimeZoneInfo.Utc;

            switch (userTimezone)
            {
                case "eastern":
                    timezoneInfo = TZConvert.GetTimeZoneInfo("Eastern Standard Time");
                    break;
                case "central":
                    timezoneInfo = TZConvert.GetTimeZoneInfo("Central Standard Time");
                    break;
                case "mountain":
                    timezoneInfo = TZConvert.GetTimeZoneInfo("Mountain Standard Time");
                    break;
                case "pacific":
                    timezoneInfo = TZConvert.GetTimeZoneInfo("Pacific Standard Time");
                    break;
                case "Non US Timezone":
                    timezoneInfo = TimeZoneInfo.Utc;
                    break;
                case "utc":
                    timezoneInfo = TimeZoneInfo.Utc;
                    break;
                default:
                    timezoneInfo = TimeZoneInfo.Utc;
                    break;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDatetime, timezoneInfo);
        }
    }
}
