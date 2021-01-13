using System.Collections.Generic;

namespace Morestachio.Helper.Logging
{
	/// <summary>
	///		Invokes a list of loggers
	/// </summary>
	public class ListLogger : List<ILogger>, ILogger
	{
		/// <inheritdoc />
		public bool Enabled { get; set; }

		/// <inheritdoc />
		public void Log(string logLevel, string eventId, string message, IDictionary<string, object> data)
		{
			if (!Enabled)
			{
				return;
			}

			foreach (var logger in this)
			{
				logger.Log(logLevel, eventId, message, data);
			}
		}
	}
}