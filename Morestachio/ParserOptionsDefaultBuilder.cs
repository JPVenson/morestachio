using System.Globalization;
using System.Text;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context.FallbackResolver;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.IO;

namespace Morestachio;

/// <summary>
///		Contains the defaults for all ParserOptions
/// </summary>
public static class ParserOptionsDefaultBuilder
{
	private static IParserOptionsBuilder _defaultBuilder;

	/// <summary>
	///		The default builder for ParserOptions. Modifications on the return value will be persisted.
	/// </summary>
	/// <returns></returns>
	public static IParserOptionsBuilder GetDefaults()
	{
		return _defaultBuilder ??= BuildDefault();
	}

	/// <summary>
	///		Gets a new Default builder instance with preset values.
	/// </summary>
	/// <returns></returns>
	public static IParserOptionsBuilder BuildDefault()
	{
		return ParserOptionsBuilder.New()
			.WithTemplate("")
			.WithStreamFactory(new ByteCounterFactory())
			.WithEncoding(Encoding.UTF8)
			.WithFormatterService(new MorestachioFormatterService(false))
			.WithNull(string.Empty)
			.WithMaxSize(0)
			.WithDisableContentEscaping(false)
			.WithTimeout(TimeSpan.Zero)
			.WithPartialStackSize(255)
			.WithCultureInfo(() => CultureInfo.CurrentUICulture)
			.WithUnmatchedTagBehavior(UnmatchedTagBehavior.ThrowError | UnmatchedTagBehavior.LogWarning)
			.WithFallbackValueResolver(new CachedReflectionTypeFallbackResolver());
	}
}