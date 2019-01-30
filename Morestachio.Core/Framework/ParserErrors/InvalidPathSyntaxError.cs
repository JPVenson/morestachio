using System;

namespace Morestachio.Framework
{
	/// <summary>
	///		Defines an Error on a position within the Template.
	/// </summary>
	public class InvalidPathSyntaxError : IMorestachioError
	{
		/// <inheritdoc />
		public Tokenizer.CharacterLocation Location { get; }

		/// <summary>
		/// Gets the token.
		/// </summary>
		/// <value>
		/// The token.
		/// </value>
		public string Token { get; }
		
		/// <inheritdoc />
		public InvalidPathSyntaxError(Tokenizer.CharacterLocation location, string token)
		{
			Location = location;
			Token = token;
			HelpText =
				$"The path '{Token}' on line:char '{Location.Line}:{Location.Character}' is not valid. Please see documentation for examples of valid paths.";
		}
		
		/// <inheritdoc />
		public Exception GetException()
		{
			return new IndexedParseException(Location, HelpText);
		}

		/// <inheritdoc />
		public string HelpText { get; }
	}
}