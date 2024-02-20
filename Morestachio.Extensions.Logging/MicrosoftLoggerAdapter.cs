using System.Text;
using Microsoft.Extensions.Logging;
using ILogger = Morestachio.Helper.Logging.ILogger;

namespace Morestachio.Extensions.Logging;

public class MicrosoftLoggerAdapter : ILogger
{
	private readonly Microsoft.Extensions.Logging.ILogger _logger;
	private readonly int _eventId;
	private readonly LogLevel _defaultLogLevel;
	private readonly Func<LogEntry, string> _format;

	public MicrosoftLoggerAdapter(
		Microsoft.Extensions.Logging.ILogger logger,
		int eventId,
		LogLevel defaultLogLevel,
		Func<LogEntry, string> format
	)
	{
		_logger = logger;
		_eventId = eventId;
		_defaultLogLevel = defaultLogLevel;
		_format = format;
		Enabled = true;
	}

	/// <inheritdoc />
	public bool Enabled { get; set; }

	/// <summary>
	///		Defines an log entry
	/// </summary>
	public class LogEntry
	{
		public string Message { get; }
		public IDictionary<string, object> Data { get; }

		public LogEntry(string message, IDictionary<string, object> data)
		{
			Message = message;
			Data = data;
		}
	}

	/// <inheritdoc />
	public void Log(
		string logLevel,
		string eventId,
		string message,
		IDictionary<string, object> data
	)
	{
		if (!Enabled)
		{
			return;
		}

		_logger.Log(ParseLogLevel(logLevel), new EventId(_eventId, eventId), new LogEntry(message, data), null,
			FormatMessage);
	}

	/// <inheritdoc />
	public void Log(string logLevel, string eventId, string message)
	{
		if (!Enabled)
		{
			return;
		}

		_logger.Log(ParseLogLevel(logLevel), new EventId(_eventId, eventId), new LogEntry(message, null), null,
			FormatMessage);
	}

	private string FormatMessage(LogEntry entry, Exception arg2)
	{
		if (_format != null)
		{
			return _format(entry);
		}

		if (entry.Data?.Count is null or 0)
		{
			return entry.Message;
		}

		var sb = new StringBuilder(entry.Message);
		sb.AppendLine();

		foreach (var data in entry.Data)
		{
			sb.AppendLine($"{data.Key} - {(data.Value?.ToString() ?? "{NULL}")}");
		}

		return sb.ToString();
	}

	private LogLevel ParseLogLevel(string logLevel)
	{
		return logLevel.ToUpper() switch
		{
			"Warning" => LogLevel.Warning,
			"Critical" => LogLevel.Critical,
			"Debug" => LogLevel.Debug,
			"Error" => LogLevel.Error,
			"Info" => LogLevel.Information,
			"Trace" => LogLevel.Trace,
			_ => _defaultLogLevel
		};
	}
}