using System;

namespace Morestachio.Parsing.ParserErrors
{
	public struct CharacterSnippedLocation : IEquatable<CharacterSnippedLocation>
	{
		internal CharacterSnippedLocation(int line, int character, string snipped)
		{
			Line = line;
			Character = character;
			Snipped = snipped;
		}

		public string Snipped { get; private set; }

		/// <summary>
		///		The line of the Template
		/// </summary>
		public int Line { get; }

		/// <summary>
		///		The Character at the <see cref="Line"/>
		/// </summary>
		public int Character { get; }

		public bool Equals(CharacterSnippedLocation other)
		{
			return Snipped == other.Snipped && Line == other.Line && Character == other.Character;
		}

		public override bool Equals(object obj)
		{
			return obj is CharacterSnippedLocation other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Snipped != null ? Snipped.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Line;
				hashCode = (hashCode * 397) ^ Character;
				return hashCode;
			}
		}
	}
}