using System;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Error
{
	/// <summary>
	///     Indicates a parse error including line and character info.
	/// </summary>
	public class IndexedParseException : MustachioException
	{
		internal static string FormatMessage(string message, CharacterLocationExtended location)
		{
			return $"{location.Line}:{location.Character} {message}" +
				   Environment.NewLine +
					location.Render();
		}

		internal IndexedParseException(CharacterLocationExtended location, string message)
			: base(FormatMessage(message, location))
		{
			Location = location;
		}

		/// <summary>
		///		The location of the error within the original template
		/// </summary>
		public CharacterLocationExtended Location { get; set; }
	}
}