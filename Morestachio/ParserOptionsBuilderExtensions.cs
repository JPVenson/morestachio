using System.Threading.Tasks;

namespace Morestachio;

/// <summary>
///		Extension methods for fast building and parsing templates
/// </summary>
public static class ParserOptionsBuilderExtensions
{
	/// <summary>
	///		Builds the underlying ParserOptions and Parses the template
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static Task<MorestachioDocumentInfo> BuildAndParseAsync(this IParserOptionsBuilder builder)
	{
		return Parser.ParseWithOptionsAsync(builder.Build());
	}
	
	/// <summary>
	///		Builds the underlying ParserOptions and Parses the template
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static MorestachioDocumentInfo BuildAndParse(this IParserOptionsBuilder builder)
	{
		return Parser.ParseWithOptions(builder.Build());
	}
}
