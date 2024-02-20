using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Helper.Serialization;

/// <summary>
///		Contains helper methods for serializing <see cref="IMorestachioError"/>
/// </summary>
public static class ErrorSerializationHelper
{
	static ErrorSerializationHelper()
	{
		ErrorTypeLookup = new Dictionary<string, Type>();
		ErrorTypeLookup["InvalidPathSyntaxError"] = typeof(InvalidPathSyntaxError);
		ErrorTypeLookup["PathSyntaxError"] = typeof(MorestachioSyntaxError);
		ErrorTypeLookup["UnclosedScopeError"] = typeof(MorestachioUnclosedScopeError);
		ErrorTypeLookup["UnopendScopeError"] = typeof(MorestachioUnopendScopeError);
	}

	/// <summary>
	///		Mapping for type - serialized name
	/// </summary>
	public static IDictionary<string, Type> ErrorTypeLookup;
}