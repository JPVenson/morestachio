namespace Morestachio.System.Text.Json;

/// <summary>
///		Extension method for adding the JsonValue Resolver
/// </summary>
public static class SystemTextJsonResolverExtensions
{
	///  <summary>
	/// 		Sets or Adds the Json Value resolver
	///  </summary>
	///  <param name="optionsBuilder"></param>
	///  <returns></returns>
	public static IParserOptionsBuilder WithSystemTextJsonValueResolver(this IParserOptionsBuilder optionsBuilder)
	{
		return optionsBuilder.WithValueResolver(new SystemTextJsonResolver());
	}
}