using System.Collections.Generic;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Helper.Logging
{
	/// <summary>
	///		Contains formatter calls for access to the ILogger interface
	/// </summary>
	public static class LoggingFormatter
	{
		/// <summary>
		///		Gets the list of all event ids that are invoked by the framework itself
		/// </summary>
		public static string[] FrameworkEventIds { get; } = new[] { FormatterObsoleteEventId, ParserEventId, TokenizerEventId, FormatterServiceId };

		/// <summary>
		///		The event ID for an obsolete formatter 
		/// </summary>
		public static readonly string FormatterObsoleteEventId = "Formatter-Obsolete";

		/// <summary>
		///		The event ID for an parser event
		/// </summary>
		public static readonly string ParserEventId = "Parser";
		
		/// <summary>
		///		The event ID for an tokenizer event
		/// </summary>
		public static readonly string TokenizerEventId = "Tokenizer";

		/// <summary>
		///		The event ID for an <see cref="MorestachioFormatterService"/> event
		/// </summary>
		public static readonly string FormatterServiceId = "FormatterService";

		/// <summary>
		///		Enables the logger
		/// </summary>
		[MorestachioGlobalFormatter("LogEnable", "Enables the logger")]
		public static void LogEnable([ExternalData] ParserOptions options)
		{
			if (options.Logger != null)
			{
				options.Logger.Enabled = true;
			}
		}
		/// <summary>
		///		Enables the logger
		/// </summary>
		[MorestachioGlobalFormatter("LogDisable", "Disables the logger")]
		public static void LogDisable([ExternalData] ParserOptions options)
		{
			if (options.Logger != null)
			{
				options.Logger.Enabled = false;
			}
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="logLevel"></param>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("Log", "Logs an Specific event")]
		public static void Log(string logLevel, string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.Log(logLevel, eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("LogWarn", "Logs an Specific event")]
		public static void LogWarn(string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.LogWarn(eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("LogCritical", "Logs an Specific event")]
		public static void LogCritical(string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.LogCritical(eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("LogDebug", "Logs an Specific event")]
		public static void LogDebug(string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.LogDebug(eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("LogError", "Logs an Specific event")]
		public static void LogError(string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.LogError(eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("LogInfo", "Logs an Specific event")]
		public static void LogInfo(string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.LogInfo(eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		[MorestachioGlobalFormatter("LogTrace", "Logs an Specific event")]
		public static void LogTrace(string eventId, string message, [ExternalData] ParserOptions options)
		{
			options.Logger?.LogTrace(eventId, message, null);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		public static void LogWarn(this ILogger logger, string eventId, string message, IDictionary<string, object> data = null)
		{
			logger?.Log("Warning", eventId, message, data);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		public static void LogCritical(this ILogger logger, string eventId, string message, IDictionary<string, object> data = null)
		{
			logger?.Log("Critical", eventId, message, data);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		public static void LogDebug(this ILogger logger, string eventId, string message, IDictionary<string, object> data = null)
		{
			logger?.Log("Debug", eventId, message, data);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		public static void LogError(this ILogger logger, string eventId, string message, IDictionary<string, object> data = null)
		{
			logger?.Log("Error", eventId, message, data);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		public static void LogInfo(this ILogger logger, string eventId, string message, IDictionary<string, object> data = null)
		{
			logger?.Log("Info", eventId, message, data);
		}

		/// <summary>
		///		Logs an Specific event
		/// </summary>
		/// <param name="eventId"></param>
		/// <param name="message"></param>
		public static void LogTrace(this ILogger logger, string eventId, string message, IDictionary<string, object> data = null)
		{
			logger?.Log("Trace", eventId, message, data);
		}
	}
}
