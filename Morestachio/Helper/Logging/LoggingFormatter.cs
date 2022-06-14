using System.Collections.Generic;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Helper.Logging;

/// <summary>
///		Contains formatter calls for access to the ILogger interface
/// </summary>
[MorestachioExtensionSetup("Must be first setup by setting the ParserOptions.Logger property")]
public static class LoggingFormatter
{

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
	///		Gets the list of all event ids that are invoked by the framework itself
	/// </summary>
	public static string[] FrameworkEventIds { get; } = new[] { FormatterObsoleteEventId, ParserEventId, TokenizerEventId, FormatterServiceId };

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
	[MorestachioGlobalFormatter("Log", "Logs an Specific event")]
	public static void Log(string logLevel, string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.Log(logLevel, eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioGlobalFormatter("LogWarn", "Logs an Specific event")]
	public static void LogWarn(string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.LogWarn(eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioGlobalFormatter("LogCritical", "Logs an Specific event")]
	public static void LogCritical(string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.LogCritical(eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioGlobalFormatter("LogDebug", "Logs an Specific event")]
	public static void LogDebug(string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.LogDebug(eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioGlobalFormatter("LogError", "Logs an Specific event")]
	public static void LogError(string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.LogError(eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioGlobalFormatter("LogInfo", "Logs an Specific event")]
	public static void LogInfo(string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.LogInfo(eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	[MorestachioGlobalFormatter("LogTrace", "Logs an Specific event")]
	public static void LogTrace(string eventId, string message, [ExternalData] ParserOptions options)
	{
		options.Logger?.LogTrace(eventId, message, null);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogWarn(this ILogger logger, string eventId, string message, IDictionary<string, object> data)
	{
		logger?.Log("Warning", eventId, message, data);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogCritical(this ILogger logger, string eventId, string message, IDictionary<string, object> data)
	{
		logger?.Log("Critical", eventId, message, data);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogDebug(this ILogger logger, string eventId, string message, IDictionary<string, object> data)
	{
		logger?.Log("Debug", eventId, message, data);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogError(this ILogger logger, string eventId, string message, IDictionary<string, object> data)
	{
		logger?.Log("Error", eventId, message, data);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogInfo(this ILogger logger, string eventId, string message, IDictionary<string, object> data)
	{
		logger?.Log("Info", eventId, message, data);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogTrace(this ILogger logger, string eventId, string message, IDictionary<string, object> data)
	{
		logger?.Log("Trace", eventId, message, data);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogWarn(this ILogger logger, string eventId, string message)
	{
		logger?.Log("Warning", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogCritical(this ILogger logger, string eventId, string message)
	{
		logger?.Log("Critical", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogDebug(this ILogger logger, string eventId, string message)
	{
		logger?.Log("Debug", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogError(this ILogger logger, string eventId, string message)
	{
		logger?.Log("Error", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogInfo(this ILogger logger, string eventId, string message)
	{
		logger?.Log("Info", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogTrace(this ILogger logger, string eventId, string message)
	{
		logger?.Log("Trace", eventId, message);
	}
#if NET6_0_OR_GREATER
	

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogMessage(this ILogger logger, string loglevel, string eventId,
								[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		if (!logger.Enabled)
		{
			return;
		}

		logger.Log(loglevel, eventId, message.GetFormattedText());
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogWarn(this ILogger logger, string eventId,
								[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		logger?.LogMessage("Warning", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogCritical(this ILogger logger, string eventId,
									[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		logger?.LogMessage("Critical", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogDebug(this ILogger logger, string eventId,
								[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		logger?.LogMessage("Debug", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogError(this ILogger logger, string eventId,
								[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		logger?.LogMessage("Error", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogInfo(this ILogger logger, string eventId,
								[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		logger?.LogMessage("Info", eventId, message);
	}

	/// <summary>
	///		Logs an Specific event
	/// </summary>
	public static void LogTrace(this ILogger logger, string eventId,
								[InterpolatedStringHandlerArgument("logger")] LogInterpolatedStringHandler message)
	{
		logger?.LogMessage("Trace", eventId, message);
	}
#endif
}