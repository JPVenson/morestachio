namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines an error that occured when parsing the template that has an invalid token
/// </summary>
[Serializable]
public class MorestachioSyntaxError : MorestachioErrorBase
{
	/// <summary>
	///		Serialization constructor
	/// </summary>
	protected MorestachioSyntaxError()
	{
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="info"></param>
	/// <param name="c"></param>
	protected MorestachioSyntaxError(SerializationInfo info, StreamingContext c)
		: base(info, c)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
	/// </summary>
	/// <param name="location">The location.</param>
	public MorestachioSyntaxError(TextRange location,
								string operation,
								string tokenOccured,
								string syntaxExpected,
								string helpText = null)
		: base(location, FormatHelpText(operation, tokenOccured, syntaxExpected, helpText, location))
	{
	}

	private static string FormatHelpText(string operation,
										string tokenOccured,
										string syntaxExpected,
										string extra,
										TextRange location)
	{
		return $"line:char " +
			$"'{location.ToString()}' " +
			$"- " +
			$"The syntax to " +
			$"{operation} the '{tokenOccured}' " +
			$"block should be: '{syntaxExpected}'.{extra}";
	}
}