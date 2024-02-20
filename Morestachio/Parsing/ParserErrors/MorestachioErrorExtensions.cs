namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines extension methods for <see cref="IMorestachioError"/>
/// </summary>
public static class MorestachioErrorExtensions
{
	/// <summary>
	///		Uses 
	/// </summary>
	/// <param name="error"></param>
	/// <returns></returns>
	public static string AsFormatted(this IMorestachioError error)
	{
		var sb = StringBuilderCache.Acquire();
		error.Format(sb);

		return StringBuilderCache.GetStringAndRelease(sb);
	}
}