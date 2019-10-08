using System;
using System.Linq;
using JetBrains.Annotations;
using Morestachio.ParserErrors;

namespace Morestachio.Framework
{
	/// <summary>
	///     Indicates a parse error including line and character info.
	/// </summary>
	public class IndexedParseException : MustachioException
	{
		private static string FormatMessage(string message, CharacterLocationExtended location)
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

		public CharacterLocationExtended Location { get; set; }
	}
}