namespace Morestachio.Formatter.Framework;

/// <summary>
///     Can be returned by a Formatter to control what formatter should be used
/// </summary>
public readonly struct FormatterFlow
{
	/// <summary>
	///     Return code for all formatters to skip the execution of the current formatter and try another one that could also
	///     match
	/// </summary>
	public static FormatterFlow Skip { get; } = new FormatterFlow();
}