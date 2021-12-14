using System;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined;
#pragma warning disable CS1591
public static class TimeSpanFormatter
{
	[MorestachioFormatter("ToTimeSpan", "Parses a string into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpan", "Parses a string into a TimeSpan")]
	public static TimeSpan ParseTimeSpan([SourceObject]string timespan, [ExternalData] ParserOptions parserOptions)
	{
		return TimeSpan.Parse(timespan, parserOptions.CultureInfo);
	}

	[MorestachioFormatter("ToTimeSpanExact", "Converts the string representation of a time interval to its TimeSpan equivalent by using the specified format and culture-specific format information.")]
	[MorestachioGlobalFormatter("TimeSpanExact", "Converts the string representation of a time interval to its TimeSpan equivalent by using the specified format and culture-specific format information.")]
	public static TimeSpan ParseTimeSpanExact([SourceObject]string timespan, string format)
	{
		return TimeSpan.ParseExact(timespan, format, null);
	}

	[MorestachioFormatter("TimeSpanFromDays", "Parses a number of Days into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpanFromDays", "Parses a number of Days into a TimeSpan")]
	public static TimeSpan FromDays([SourceObject]Number days)
	{
		return TimeSpan.FromDays(days.ToDouble(null));
	}

	[MorestachioFormatter("TimeSpanFromHours", "Parses a number of Hours into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpanFromHours", "Parses a number of Hours into a TimeSpan")]
	public static TimeSpan FromHours([SourceObject]Number days)
	{
		return TimeSpan.FromHours(days.ToDouble(null));
	}

	[MorestachioFormatter("TimeSpanFromMilliseconds", "Parses a number of Milliseconds into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpanFromMilliseconds", "Parses a number of Milliseconds into a TimeSpan")]
	public static TimeSpan FromMilliseconds([SourceObject]Number days)
	{
		return TimeSpan.FromMilliseconds(days.ToDouble(null));
	}

	[MorestachioFormatter("TimeSpanFromMinutes", "Parses a number of Minutes into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpanFromMinutes", "Parses a number of Minutes into a TimeSpan")]
	public static TimeSpan FromMinutes([SourceObject]Number days)
	{
		return TimeSpan.FromMinutes(days.ToDouble(null));
	}

	[MorestachioFormatter("TimeSpanFromSeconds", "Parses a number of Seconds into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpanFromSeconds", "Parses a number of Seconds into a TimeSpan")]
	public static TimeSpan FromSeconds([SourceObject]Number days)
	{
		return TimeSpan.FromSeconds(days.ToDouble(null));
	}

	[MorestachioFormatter("TimeSpanFromTicks", "Parses a number of Ticks into a TimeSpan")]
	[MorestachioGlobalFormatter("TimeSpanFromTicks", "Parses a number of Ticks into a TimeSpan")]
	public static TimeSpan FromTicks([SourceObject]Number days)
	{
		return TimeSpan.FromTicks(days.ToInt64(null));
	}

	[MorestachioFormatter("Negate", "Negate the value of an TimeSpan")]
	public static TimeSpan Negate([SourceObject]TimeSpan timeSpan)
	{
		return timeSpan.Negate();
	}

	[MorestachioFormatter("Add", "Adds the TimeSpan to the given TimeSpan")]
	[MorestachioOperator(OperatorTypes.Add, "Adds the TimeSpan to the given TimeSpan")]
	public static TimeSpan Add([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan.Add(timespan);
	}

	[MorestachioFormatter("Subtract", "Subtracts the TimeSpan to the given TimeSpan")]
	[MorestachioOperator(OperatorTypes.Substract, "Subtracts the TimeSpan to the given TimeSpan")]
	public static TimeSpan Subtract([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan.Subtract(timespan);
	}

	[MorestachioFormatter("Multiply", "Multiplies the TimeSpan to the given Number")]
	[MorestachioOperator(OperatorTypes.Multiply, "Multiplies the TimeSpan to the given Number")]
	public static TimeSpan Multiply([SourceObject]TimeSpan timeSpan, Number factor)
	{
		return new TimeSpan((long)(timeSpan.Ticks * factor.ToDouble(null)));
	}

	[MorestachioFormatter("Divide", "Divides the TimeSpan to the given Number")]
	[MorestachioOperator(OperatorTypes.Divide, "Divides the TimeSpan to the given Number")]
	public static TimeSpan Divide([SourceObject]TimeSpan timeSpan, Number factor)
	{
		return new TimeSpan((long)(timeSpan.Ticks / factor.ToDouble(null)));
	}

	[MorestachioFormatter("GreaterThen", "Checks if the source TimeSpan is bigger as the other TimeSpan")]
	[MorestachioOperator(OperatorTypes.GreaterThen, "Checks if the source TimeSpan is bigger as the other TimeSpan")]
	public static bool GreaterThen([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan > timespan;
	}

	[MorestachioFormatter("LessThen", "Checks if the source TimeSpan is smaller as the other TimeSpan")]
	[MorestachioOperator(OperatorTypes.LessThen, "Checks if the source TimeSpan is bigger as the other TimeSpan")]
	public static bool LessThen([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan < timespan;
	}

	[MorestachioFormatter("GreaterOrEquals", "Checks if the source TimeSpan is bigger or equals as the other TimeSpan")]
	[MorestachioOperator(OperatorTypes.GreaterOrEquals, "Checks if the source TimeSpan is bigger as the other TimeSpan")]
	public static bool GreaterOrEquals([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan >= timespan;
	}

	[MorestachioFormatter("LessOrEquals", "Checks if the source TimeSpan is bigger or equals as the other TimeSpan")]
	[MorestachioOperator(OperatorTypes.LessOrEquals, "Checks if the source TimeSpan is bigger as the other TimeSpan")]
	public static bool LessOrEquals([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan <= timespan;
	}

	[MorestachioFormatter("Equals", "Checks if the two TimeSpan are equal to each other")]
	[MorestachioOperator(OperatorTypes.Equals, "Checks if the two TimeSpan are equal to each other")]
	public static bool Equals([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan == timespan;
	}

	[MorestachioFormatter("UnEquals", "Checks if the two TimeSpan are not equal to each other")]
	[MorestachioOperator(OperatorTypes.UnEquals, "Checks if the two TimeSpan are not equal to each other")]
	public static bool UnEquals([SourceObject]TimeSpan timeSpan, TimeSpan timespan)
	{
		return timeSpan != timespan;
	}
}