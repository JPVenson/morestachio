namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines an error that occured when parsing the template that has an invalid token
/// </summary>
[Serializable]
public class MorestachioUnclosedScopeError : MorestachioErrorBase
{
	/// <summary>
	///		Serialization constructor
	/// </summary>
	protected MorestachioUnclosedScopeError()
	{
		
	}

	/// <inheritdoc />
	protected MorestachioUnclosedScopeError(SerializationInfo info, StreamingContext c) 
		: base(info, c)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
	/// </summary>
	/// <param name="location">The location.</param>
	public MorestachioUnclosedScopeError(TextRange location,
										string tokenOccured,
										string syntaxExpected,
										string helpText = null)
		: base(location, FormatHelpText(tokenOccured, syntaxExpected, helpText, location))
	{
	}

	private static string FormatHelpText(string tokenOccured,
										string syntaxExpected,
										string helpText,
										TextRange location)
	{
		return $"line:char '{location}' - An '{tokenOccured}' block is being opened, but no corresponding opening element '{syntaxExpected}' was detected.{helpText}";
	}
}