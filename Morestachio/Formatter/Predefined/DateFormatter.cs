using System;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined
{
	public static class DateFormatter
	{
		[MorestachioGlobalFormatter("DaysInMonth", "Gets the Days in a given Month of the year")]
		public static int DaysInMonth(Number year, Number month)
		{
			return DateTime.DaysInMonth(year.ToInt32(null), month.ToInt32(null));
		}

		[MorestachioGlobalFormatter("DateTimeNow", "Gets a DateTime object representing the current time")]
		public static DateTime DateTimeNow()
		{
			return DateTime.Now;
		}

		[MorestachioGlobalFormatter("DateTimeOffsetNow", "Gets a DateTime object representing the current time")]
		public static DateTimeOffset DateTimeOffsetNow()
		{
			return DateTimeOffset.Now;
		}

		[MorestachioGlobalFormatter("IsLeapYear", "Gets if the given year is a leap year")]
		public static bool IsLeapYear(Number year)
		{
			return DateTime.IsLeapYear(year.ToInt32(null));
		}
		
		[MorestachioFormatter("Add", "Adds the TimeSpan to the given DateTime")]
		public static DateTime Add([SourceObject]DateTime dateTime, TimeSpan timespan)
		{
			return dateTime.Add(timespan);
		}

		[MorestachioFormatter("ToDateTime", "Converts the string representation of a date and time to its DateTime equivalent.")]
		[MorestachioGlobalFormatter("DateTime", "Converts the string representation of a date and time to its DateTime equivalent.")]
		public static DateTime Parse(string text, [ExternalData] ParserOptions parserOptions)
		{
			return DateTime.Parse(text, parserOptions.CultureInfo);
		}

		[MorestachioFormatter("ToExactDateTime", "Converts the string representation of a date and time to its DateTime equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.")]
		[MorestachioGlobalFormatter("ExactDateTime", "Converts the string representation of a date and time to its DateTime equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.")]
		public static DateTime ParseExact(string text, string format)
		{
			return DateTime.ParseExact(text, format, null);
		}

		[MorestachioFormatter("Add", "Adds the TimeSpan to the given DateTime")]
		public static DateTimeOffset Add([SourceObject]DateTimeOffset dateTime, TimeSpan timespan)
		{
			return dateTime.Add(timespan);
		}

		[MorestachioFormatter("ToDateTimeOffset", "Converts the string representation of a date and time to its DateTime equivalent.")]
		[MorestachioGlobalFormatter("DateTimeOffset", "Converts the string representation of a date and time to its DateTime equivalent.")]
		public static DateTimeOffset ParseDateTimeOffset(string text, [ExternalData] ParserOptions parserOptions)
		{
			return DateTimeOffset.Parse(text, parserOptions.CultureInfo);
		}

		[MorestachioFormatter("ToExactDateTimeOffset", "Converts the string representation of a date and time to its DateTime equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.")]
		[MorestachioGlobalFormatter("ExactDateTimeOffset", "Converts the string representation of a date and time to its DateTime equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.")]
		public static DateTimeOffset ParseExactDateTimeOffset(string text, string format)
		{
			return DateTimeOffset.ParseExact(text, format, null);
		}
	}
}