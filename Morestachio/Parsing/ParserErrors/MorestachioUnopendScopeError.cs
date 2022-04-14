namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines an error that occured when parsing the template that has an invalid token
/// </summary>
[Serializable]
public class MorestachioUnopendScopeError : MorestachioErrorBase
{
	/// <summary>
	///		Serialization constructor
	/// </summary>
	protected MorestachioUnopendScopeError()
	{
		
	}

	/// <inheritdoc />
	protected MorestachioUnopendScopeError(SerializationInfo info, StreamingContext c) 
		: base(info, c)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
	/// </summary>
	/// <param name="location">The location.</param>
	public MorestachioUnopendScopeError(CharacterLocationExtended location, 
										string tokenOccured, 
										string syntaxExpected, 
										string extra = null)
		: base(location, FormatHelpText(tokenOccured, syntaxExpected, extra, location))
	{
	}

	private static string FormatHelpText(string tokenOccured, 
										string syntaxExpected, 
										string extra,
										CharacterLocationExtended location)
	{
		return $"line:char '{location.Line}:{location.Character}' - An '{tokenOccured}' block is being closed, but no corresponding opening element '{syntaxExpected}' was detected.{extra}";
	}
}