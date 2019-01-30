using System;

namespace Morestachio.Framework
{
	/// <summary>
	///		Defines a Error while parsing a Template
	/// </summary>
	public interface IMorestachioError
	{
		/// <summary>
		///		The location within the Template where the error occured
		/// </summary>
		Tokenizer.CharacterLocation Location { get; }

		/// <summary>
		/// Gets the exception.
		/// </summary>
		/// <returns></returns>
		Exception GetException();

		/// <summary>
		///		Gets a string that describes the Error
		/// </summary>
		string HelpText { get; }
	}

	/// <summary>
	///		Defines an error that occured when parsing the template that has an invalid token
	/// </summary>
	public class MorestachioUnopendScopeError : IMorestachioError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
		/// </summary>
		/// <param name="location">The location.</param>
		public MorestachioUnopendScopeError(Tokenizer.CharacterLocation location, string tokenOccured, string syntaxExpected, string extra = null)
		{
			Location = location;
			HelpText = $"An '{tokenOccured}' block is being closed, but no corresponding opening element '{syntaxExpected}' was detected.{extra}";
		}
		
		/// <inheritdoc />
		public Tokenizer.CharacterLocation Location { get; }

		/// <inheritdoc />
		public Exception GetException()
		{
			return new IndexedParseException(Location, HelpText);
		}
		
		/// <inheritdoc />
		public string HelpText { get; }
	}

	/// <summary>
	///		Defines an error that occured when parsing the template that has an invalid token
	/// </summary>
	public class MorestachioSyntaxError : IMorestachioError
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MorestachioSyntaxError"/> class.
		/// </summary>
		/// <param name="location">The location.</param>
		public MorestachioSyntaxError(Tokenizer.CharacterLocation location, string operation, string tokenOccured, string syntaxExpected, string extra = null)
		{
			Location = location;
			HelpText = $"The syntax to {operation} the '{tokenOccured}' block should be: '{syntaxExpected}'.{extra}";
		}
		
		/// <inheritdoc />
		public Tokenizer.CharacterLocation Location { get; }

		/// <inheritdoc />
		public Exception GetException()
		{
			return new IndexedParseException(Location, HelpText);
		}
		
		/// <inheritdoc />
		public string HelpText { get; }
	}

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