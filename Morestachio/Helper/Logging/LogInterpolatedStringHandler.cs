#if NET6_0_OR_GREATER
using System.Text;

namespace Morestachio.Helper.Logging;

#pragma warning disable CS1591
[InterpolatedStringHandler]
public readonly ref struct LogInterpolatedStringHandler
{
	// Storage for the built-up string
	readonly StringBuilder _builder;
	private readonly bool _isEnabled;

	public LogInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger)
	{
		_isEnabled = logger?.Enabled == true;

		if (!_isEnabled)
		{
			_builder = null;
			return;
		}

		_builder = StringBuilderCache.Acquire(literalLength);
	}

	public void AppendLiteral(string s)
	{
		if (!_isEnabled)
		{
			return;
		}
		_builder.Append(s);
	}

	public void AppendFormatted<T>(T t)
	{
		if (!_isEnabled)
		{
			return;
		}
		_builder.Append(t?.ToString());
	}

	public string GetFormattedText()
	{
		if (_builder is null)
		{
			return null;
		}

		return StringBuilderCache.GetStringAndRelease(_builder);
	}
}

#pragma warning restore CS1591
#endif