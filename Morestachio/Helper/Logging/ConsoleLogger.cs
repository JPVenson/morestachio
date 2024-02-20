namespace Morestachio.Helper.Logging;

/// <summary>
///     Logs events to the console
/// </summary>
public class ConsoleLogger : ILogger
{
	/// <summary>
	///     ctor
	/// </summary>
	public ConsoleLogger()
	{
		FormatMessage = (level, eventId, message) =>
			$"{DateTime.Now:O}\t{level}\t{eventId}\t\"{message}\"";
		FormatDataEntry = (key, value) => $"\t{key} \t- {value}";

		LogLevelColorMap = new Dictionary<string, ConsoleColor>
		{
			{ "Warning", ConsoleColor.Yellow },
			{ "Critical", ConsoleColor.DarkRed },
			{ "Debug", ConsoleColor.White },
			{ "Error", ConsoleColor.Red },
			{ "Info", ConsoleColor.Blue },
			{ "Trace", ConsoleColor.DarkGreen },
			{ "*", ConsoleColor.White }
		};
		Enabled = true;
	}

	/// <summary>
	///     Delegate function for composing the event data
	/// </summary>
	public Func<string, string, string, string> FormatMessage { get; set; }

	/// <summary>
	///     Delegate function for composing the data dictionary of an event
	/// </summary>
	public Func<string, object, string> FormatDataEntry { get; set; }

	/// <summary>
	///     Map for event to color mapping. Clear it to disable coloring. Use "*" to define a default value.
	/// </summary>
	public IDictionary<string, ConsoleColor> LogLevelColorMap { get; }

	/// <inheritdoc />
	public bool Enabled { get; set; }

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

		ConsoleColor? coloringValue = null;
		ConsoleColor? preColoringValue = null;

		if (LogLevelColorMap.TryGetValue(logLevel, out var coloring))
		{
			coloringValue = coloring;
		}
		else if (LogLevelColorMap.TryGetValue("*", out var coloringDefault))
		{
			coloringValue = coloringDefault;
		}

		try
		{
			if (coloringValue.HasValue)
			{
				preColoringValue = Console.ForegroundColor;
				Console.ForegroundColor = coloringValue.Value;
			}

			Console.WriteLine(FormatMessage(logLevel, eventId, message));

			foreach (var entry in data ?? Enumerable.Empty<KeyValuePair<string, object>>())
			{
				Console.WriteLine(FormatDataEntry(entry.Key, entry.Value));
			}
		}
		finally
		{
			if (preColoringValue.HasValue)
			{
				Console.ForegroundColor = preColoringValue.Value;
			}
		}
	}

	/// <inheritdoc />
	public void Log(string logLevel, string eventId, string message)
	{
		Log(logLevel, eventId, message, null);
	}
}