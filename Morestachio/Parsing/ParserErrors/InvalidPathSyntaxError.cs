using System;
using System.Text;
using Morestachio.Framework.Error;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines an Error on a position within the Template.
/// </summary>
public class InvalidPathSyntaxError : IMorestachioError
{
	/// <inheritdoc />
	public CharacterLocationExtended Location { get; }

	/// <summary>
	/// Gets the token.
	/// </summary>
	/// <value>
	/// The token.
	/// </value>
	public string Token { get; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="location"></param>
	/// <param name="token"></param>
	public InvalidPathSyntaxError(CharacterLocationExtended location, string token, string helpText = null)
	{
		Location = location;
		Token = token;
		HelpText =
			$"line:char '{Location.Line}:{Location.Character}' - The path '{Token}' is not valid. Please see documentation for examples of valid paths.";
		if (helpText != null)
		{
			HelpText += "\r\n" + helpText;
		}
	}
		
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