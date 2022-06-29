using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MsILogger = Microsoft.Extensions.Logging.ILogger;
using ILogger = Morestachio.Helper.Logging.ILogger;

namespace Morestachio.Extensions.Logging
{
	/// <summary>
	///		Defines extension methods for adding logging support for <see cref="ILogger"/>
	/// </summary>
	public static class LoggerExtensions
	{
		/// <summary>
		///		Adds an Adapter for <see cref="MsILogger"/> to <see cref="ILogger"/>
		/// </summary>
		public static IParserOptionsBuilder WithMicrosoftLogger(this IParserOptionsBuilder builder,
																MsILogger logger,
																int eventId,
																LogLevel defaultLogLevel,
																Func<MicrosoftLoggerAdapter.LogEntry, string> format)
		{
			return builder.WithLogger(() => new MicrosoftLoggerAdapter(logger, eventId, defaultLogLevel, format));
		}

		/// <summary>
		///		Adds an Adapter for <see cref="MsILogger"/> to <see cref="ILogger"/>
		/// </summary>
		public static IParserOptionsBuilder WithMicrosoftLogger(this IParserOptionsBuilder builder,
																MsILogger logger,
																int eventId,
																LogLevel defaultLogLevel)
		{
			return builder.WithMicrosoftLogger(logger, eventId, defaultLogLevel, null);
		}

		/// <summary>
		///		Adds an Adapter for <see cref="MsILogger"/> to <see cref="ILogger"/>
		/// </summary>
		public static IParserOptionsBuilder WithMicrosoftLogger(this IParserOptionsBuilder builder,
																MsILogger logger,
																int eventId)
		{
			return builder.WithMicrosoftLogger(logger, eventId, LogLevel.Debug);
		}
	}
}