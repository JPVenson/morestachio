using System;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework
{
	/// <summary>
	///		Describes an Position within the Template
	/// </summary>
	public struct CharacterLocation : IEquatable<CharacterLocation>, IComparable<CharacterLocation>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="character"></param>
		public CharacterLocation(int line, int character)
		{
			Line = line;
			Character = character;
		}

		/// <summary>
		///		The line of the Template
		/// </summary>
		public int Line { get; }

		/// <summary>
		///		The Character at the <see cref="Line"/>
		/// </summary>
		public int Character { get; }

		/// <summary>
		///		Returns the Unknown Location
		/// </summary>
		public static CharacterLocation Unknown { get; } = new CharacterLocation(-1, -1);

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
				throw new ArgumentException("The formatstring for a CharacterLocation is invalid");
			}

			var parts = formatString.Split(':');
			var charLoc = new CharacterLocation(int.Parse(parts[0]), int.Parse(parts[1]));
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
			return new CharacterLocation(Line, Character + length);
		}

		public int CompareTo(CharacterLocation other)
		{
			var lineComparison = Line.CompareTo(other.Line);
			if (lineComparison != 0)
			{
				return lineComparison;
			}

			return Character.CompareTo(other.Character);
		}
	}
}