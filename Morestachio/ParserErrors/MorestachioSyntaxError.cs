using System;
using Morestachio.Framework;

namespace Morestachio.ParserErrors
{
	/// <summary>
	///		Defines an error that occured when parsing the template that has an invalid token
	/// </summary>
	public class MorestachioSyntaxError : IMorestachioError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
		/// </summary>
		/// <param name="location">The location.</param>
		public MorestachioSyntaxError(CharacterLocationExtended location, string operation, string tokenOccured, string syntaxExpected, string extra = null)
		{
			Location = location;
			HelpText = $"line:char " +
			           $"'{Location.Line}:{Location.Character}' " +
			           $"- " +
			           $"The syntax to " +
			           $"{operation} the '{tokenOccured}' " +
			           $"block should be: '{syntaxExpected}'.{extra}";
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
	}
}