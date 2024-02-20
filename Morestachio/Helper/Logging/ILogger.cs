using System.Collections.Generic;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Helper.Logging;

/// <summary>
///		Represents a type used to perform logging.
/// </summary>
public interface ILogger
{
	/// <summary>
	///		Sets whenever the logger should accept more logging entries
	/// </summary>
	bool Enabled { get; set; }

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioFormatter("Log", "Logs an specific event")]
	[Obsolete("The data argument is obsolete and should be embedded into the message by the caller.")]
	void Log(string logLevel,
			string eventId,
			string message,
			IDictionary<string, object> data);

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioFormatter("Log", "Logs an specific event")]
	void Log(string logLevel, string eventId, string message);
}