using System;
using Morestachio.ParserErrors;

namespace Morestachio.Framework
{
	/// <summary>
	///		Describes an Position within the Template
	/// </summary>
	public class CharacterLocation : IEquatable<CharacterLocation>
	{
		/// <summary>
		///		The line of the Template
		/// </summary>
		public int Line { get; set; }

		/// <summary>
		///		The Character at the <see cref="Line"/>
		/// </summary>
		public int Character { get; set; }

		internal CharacterLocationExtended AddWindow(CharacterSnippedLocation window)
		{
			return new CharacterLocationExtended(Line, Character, window);
		}

		/// <summary>
		///		Formats the Character Location into a readable string
		/// </summary>
		/// <returns></returns>
		public string ToFormatString()
		{
			return $"{Line}:{Character}";
		}

		/// <summary>
		///		Parses the result of ToFormatString
		/// </summary>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public static CharacterLocation FromFormatString(string formatString)
		{
			if (!formatString.Contains(":"))
			{
				return null;
			}

			var parts = formatString.Split(':');
			var charLoc = new CharacterLocation();
			charLoc.Line = int.Parse(parts[0]);
			charLoc.Character = int.Parse(parts[1]);
			return charLoc;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Line: {Line}, Column: {Character}";
		}
		
		/// <inheritdoc />
		public bool Equals(CharacterLocation other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Line == other.Line && Character == other.Character;
		}
		
		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((CharacterLocation)obj);
		}
		
		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (Line * 397) ^ Character;
			}
		}

		/// <summary>
		///		Creates a copy that is length chars advanced to this location
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public CharacterLocation Offset(int length)
		{
			return new CharacterLocation()
			{
				Character = Character + length,
				Line = Line
			};
		}
	}
}