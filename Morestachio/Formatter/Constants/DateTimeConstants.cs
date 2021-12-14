using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Morestachio.Framework.Context.Resolver;

namespace Morestachio.Formatter.Constants;

/// <summary>
///		Gets access to DateTime
/// </summary>
public static class DateTimeConstant
{
	/// <summary>Gets a <see cref="T:System.DateTime"></see> object that is set to the current date and time on this computer, expressed as the local time.</summary>
	/// <returns>An object whose value is the current local date and time.</returns>
	[Description("Gets a DateTime object that is set to the current date and time on this computer, expressed as the local time.")]
	public static DateTime Now { get { return DateTime.Now; } }
		
	/// <summary>Gets the current date.</summary>
	/// <returns>An object that is set to today's date, with the time component set to 00:00:00.</returns>
	[Description("Gets the current date.")]
	public static DateTime Today { get { return DateTime.Today; } }

	/// <summary>Gets a <see cref="T:System.DateTime"></see> object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).</summary>
	/// <returns>An object whose value is the current UTC date and time.</returns>
	[Description("Gets a DateTime object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).")]
	public static DateTime UtcNow { get { return DateTime.UtcNow; } }

	/// <summary>Gets a <see cref="T:System.DateTime"></see> object that is set to the current date and time on this computer, expressed as the local time.</summary>
	/// <returns>An object whose value is the current local date and time.</returns>
	[Description("Gets a DateTime object that is set to the current date and time on this computer, expressed as the local time.")]
	public static DateTimeOffset OffsetNow { get { return DateTimeOffset.Now; } }
		
	/// <summary>Gets a <see cref="T:System.DateTime"></see> object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).</summary>
	/// <returns>An object whose value is the current UTC date and time.</returns>
	[Description("Gets a DateTime object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).")]
	public static DateTimeOffset OffsetUtcNow { get { return DateTimeOffset.UtcNow; } }
}