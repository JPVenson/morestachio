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
		return ParserOptionsBuilder.NewPristine()
			.WithTemplate(DefaultTemplate)
			.WithStreamFactory(DefaultStreamFactory)
			.WithEncoding(DefaultEncoding)
			.WithFormatterService(DefaultFormatterService)
			.WithNull(EmptyTemplate)
			.WithMaxSize(0)
			.WithDisableContentEscaping(false)
			.WithTimeout(() => TimeSpan.Zero)
			.WithPartialStackSize(() => 255)
			.WithCultureInfo(DefaultCulture)
			.WithUnmatchedTagBehavior(() => UnmatchedTagBehavior.ThrowError | UnmatchedTagBehavior.LogWarning)
			.WithFallbackValueResolver(DefaultFallbackResolver);
	}

	private static IFallbackValueResolver DefaultFallbackResolver()
	{
		return new CachedReflectionTypeFallbackResolver();
	}

	private static CultureInfo DefaultCulture()
	{
		return CultureInfo.CurrentUICulture;
	}

	private static string EmptyTemplate()
	{
		return string.Empty;
	}

	private static IMorestachioFormatterService DefaultFormatterService()
	{
		return new MorestachioFormatterService(false);
	}

	private static Encoding DefaultEncoding()
	{
		return Encoding.UTF8;
	}

	private static ByteCounterFactory DefaultStreamFactory()
	{
		return new ByteCounterFactory();
	}

	private static string DefaultTemplate()
	{
		return "";
	}
}