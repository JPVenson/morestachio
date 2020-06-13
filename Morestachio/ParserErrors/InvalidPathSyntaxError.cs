using System;
using System.Text;
using Morestachio.Framework;

namespace Morestachio.ParserErrors
{
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
		
		/// <inheritdoc />
		public InvalidPathSyntaxError(CharacterLocationExtended location, string token)
		{
			Location = location;
			Token = token;
			HelpText =
				$"line:char '{Location.Line}:{Location.Character}' - The path '{Token}' is not valid. Please see documentation for examples of valid paths.";
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
}