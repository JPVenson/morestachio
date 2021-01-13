using System.Collections.Generic;

namespace Morestachio.Helper.Logging
{
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
		/// <param name="logLevel"></param>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		/// <param name="data"></param>
		void Log(string logLevel, string eventId, string message, IDictionary<string, object> data);
	}
}