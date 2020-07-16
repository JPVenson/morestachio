using System;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined
{
	public static class DateFormatter
	{
		[MorestachioFormatter("Add", "Adds the TimeSpan to the given timeSpan")]
		public static DateTime Add([SourceObject]DateTime timeSpan, TimeSpan timespan)
		{
			return timeSpan.Add(timespan);
		}

		[MorestachioFormatter("DaysInMonth", "Gets the Days in a given Month of the year")]
		public static int DaysInMonth(Number year, Number month)
		{
			return DateTime.DaysInMonth(year.ToInt32(null), month.ToInt32(null));
		}

		[MorestachioFormatter("IsLeapYear", "Gets if the given year is a leap year")]
		public static bool IsLeapYear(Number year)
		{
			return DateTime.IsLeapYear(year.ToInt32(null));
		}

		[MorestachioFormatter("ParseDateTime", "Converts the string representation of a date and time to its DateTime equivalent.")]
		public static DateTime Parse(string text, [ExternalData] ParserOptions parserOptions)
		{
			return DateTime.Parse(text, parserOptions.CultureInfo);
		}

		[MorestachioFormatter("ParseExactDateTime", "Converts the string representation of a date and time to its DateTime equivalent using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.")]
		public static DateTime ParseExact(string text, string format)
		{
			return DateTime.ParseExact(text, format, null);
		}
	}
}