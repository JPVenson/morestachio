using System;
using System.Text;
using Morestachio.Framework.Error;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines an error that occured when parsing the template that has an invalid token
/// </summary>
public class MorestachioUnopendScopeError : IMorestachioError
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
	/// </summary>
	/// <param name="location">The location.</param>
	public MorestachioUnopendScopeError(CharacterLocationExtended location, string tokenOccured, string syntaxExpected, string extra = null)
	{
		Location = location;
		HelpText = $"line:char '{Location.Line}:{Location.Character}' - An '{tokenOccured}' block is being closed, but no corresponding opening element '{syntaxExpected}' was detected.{extra}";
	}
		
	/// <inheritdoc />
	public CharacterLocationExtended Location { get; }

	/// <inheritdoc />
	public Exception GetException()
	{
		return new IndexedParseException(Location, HelpText);
	}
		
	/// <inheritdoc />
	public string HelpText { get; }

	/// <inheritdoc />
	public void Format(StringBuilder sb)
	{
		sb.Append(IndexedParseException.FormatMessage(HelpText, Location));
	}
}