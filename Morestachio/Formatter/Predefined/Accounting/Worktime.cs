using System;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined.Accounting;

/// <summary>
///		Represents an amount of time
/// </summary>
public readonly struct Worktime : IFormattable
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="timeWorked"></param>
	/// <param name="precision"></param>
	public Worktime(double timeWorked, WorktimePrecision precision)
	{
		TimeWorked = timeWorked;
		Precision = precision;
	}

	private static double GetTimeInSeconds(double value, WorktimePrecision precision)
	{
		switch (precision)
		{
			case WorktimePrecision.Seconds:
				return value;
			case WorktimePrecision.Minutes:
				return value * 60;
			case WorktimePrecision.Hours:
				return value * 60 * 60;
			case WorktimePrecision.Days:
				return value * 60 * 60 * 24;
			case WorktimePrecision.Weeks:
				return value * 60 * 60 * 24 * 7;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	/// <summary>
	///		Converts the value from one precision to another
	/// </summary>
	public static double ConvertValue(double value,
									WorktimePrecision fromPrecision,
									WorktimePrecision toPrecision)
	{
		var secounds = GetTimeInSeconds(value, fromPrecision);
		switch (toPrecision)
		{
			case WorktimePrecision.Seconds:
				return secounds;
			case WorktimePrecision.Minutes:
				return secounds / 60;
			case WorktimePrecision.Hours:
				return secounds / 60 / 60;
			case WorktimePrecision.Days:
				return secounds / 60 / 60 / 24;
			case WorktimePrecision.Weeks:
				return secounds / 60 / 60 / 24 / 7;
			default:
				throw new ArgumentOutOfRangeException(nameof(toPrecision), toPrecision, null);
		}
	}

	/// <summary>
	///		The value corresponding to the <see cref="Precision"/>
	/// </summary>
	public double TimeWorked { get; }

	/// <summary>
	///		Defines the unit that <see cref="TimeWorked"/> represents
	/// </summary>
	public WorktimePrecision Precision { get; }

	/// <summary>
	///		Adds the value of time with the precision to a new Worktime
	/// </summary>
	/// <param name="time"></param>
	/// <param name="precision"></param>
	/// <returns></returns>
	public Worktime Add(double time, WorktimePrecision precision)
	{
		var newValueInSeconds = GetTimeInSeconds(TimeWorked, Precision) + GetTimeInSeconds(time, precision);
		return new Worktime(ConvertValue(newValueInSeconds, WorktimePrecision.Seconds, Precision), Precision);
	}
#pragma warning disable  CS1591
	[MorestachioGlobalFormatter("Worktime", "Creates a new Worktime Object")]
	public static Worktime WorktimeFactory()
	{
		return new Worktime();
	}

	[MorestachioGlobalFormatter("WorktimeFromTimespan", "Creates a new Worktime Object based on the Timespan")]
	public static Worktime WorktimeFactory(TimeSpan timespan)
	{
		return new Worktime().AddSeconds((long) timespan.TotalSeconds);
	}

	[MorestachioFormatter("Add", "Adds both worktimes together")]
	public static Worktime Add([SourceObject]Worktime worktime, Worktime other)
	{
		return worktime.Add(other.TimeWorked, other.Precision);
	}

	[MorestachioFormatter("AddSeconds", "Adds the given amount of Seconds to the worktime")]
	public static Worktime AddSeconds([SourceObject]Worktime worktime, Number number)
	{
		return worktime.AddSeconds(number.ToInt64(null));
	}

	[MorestachioFormatter("AddMinutes", "Adds the given amount of Minutes to the worktime")]
	public static Worktime AddMinutes([SourceObject]Worktime worktime, Number number)
	{
		return worktime.AddMinutes(number.ToInt64(null));
	}

	[MorestachioFormatter("AddHours", "Adds the given amount of Hours to the worktime")]
	public static Worktime AddHours([SourceObject]Worktime worktime, Number number)
	{
		return worktime.AddHours(number.ToInt64(null));
	}

	[MorestachioFormatter("AddDays", "Adds the given amount of Days to the worktime")]
	public static Worktime AddDays([SourceObject]Worktime worktime, Number number)
	{
		return worktime.AddDays(number.ToInt64(null));
	}

	[MorestachioFormatter("AddWeeks", "Adds the given amount of Weeks to the worktime")]
	public static Worktime AddWeeks([SourceObject]Worktime worktime, Number number)
	{
		return worktime.AddWeeks(number.ToInt64(null));
	}

	[MorestachioFormatter("AddMonths", "Adds the given amount of Months to the worktime. As all months have different days you have to specify which month to start with.")]
	public static Worktime AddMonths([SourceObject]Worktime worktime, Number number, DateTime startingWith)
	{
		return worktime.AddMonths(number.ToInt32(null), startingWith);
	}
#pragma warning restore  CS1591
	/// <summary>
	///		Adds the amount of Seconds and returns a new object representing this time
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public Worktime AddSeconds(long value)
	{
		return Add(value, WorktimePrecision.Seconds);
	}

	/// <summary>
	///		Adds the amount of Minutes and returns a new object representing this time
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public Worktime AddMinutes(long value)
	{
		return Add(value, WorktimePrecision.Minutes);
	}

	/// <summary>
	///		Adds the amount of Hours and returns a new object representing this time
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public Worktime AddHours(long value)
	{
		return Add(value, WorktimePrecision.Hours);
	}

	/// <summary>
	///		Adds the amount of Days and returns a new object representing this time
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public Worktime AddDays(long value)
	{
		return Add(value, WorktimePrecision.Days);
	}

	/// <summary>
	///		Adds the amount of Weeks and returns a new object representing this time
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public Worktime AddWeeks(long value)
	{
		return Add(value, WorktimePrecision.Weeks);
	}

	/// <summary>
	///		Adds the amount of Months and returns a new object representing this time
	/// </summary>
	/// <param name="value"></param>
	/// <param name="startingWith">As not all months are of equal days we have to know where to start. Can be in the middle of a month</param>
	/// <returns></returns>
	public Worktime AddMonths(int value, DateTime startingWith)
	{
		var editDate = startingWith.AddMonths(value);
		var compDate = editDate - startingWith;
		return Add(compDate.TotalMinutes, WorktimePrecision.Minutes);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return ToString(null, null);
	}

	/// <inheritdoc />
	public string ToString(string format, IFormatProvider formatProvider)
	{
		var writeDecimal = format == "d";

		var ts = TimeSpan.FromSeconds(GetTimeInSeconds(TimeWorked, Precision));
		if (writeDecimal)
		{
			return Math.Round(ts.TotalMinutes / 60, 2).ToString("00.00", formatProvider);
		}

		return (Math.Abs(ts.TotalHours)).ToString("00", formatProvider) + ":" + ts.Minutes.ToString("00", formatProvider);
	}
}