using System;
using System.Linq;
using Morestachio.Framework;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines a line within the template and the char that should be marked
/// </summary>
public struct CharacterLocationExtended : IEquatable<CharacterLocationExtended>, IEquatable<CharacterLocation>
{
	/// <summary>
	/// 
	/// </summary>
	public static CharacterLocationExtended Empty { get; } = new CharacterLocationExtended(0, -1, new CharacterSnippedLocation(0, 0, ""));

	internal CharacterLocationExtended(int line, int character, CharacterSnippedLocation snipped)
	{
		Line = line;
		Character = character;
		Snipped = snipped;
	}

	/// <summary>
	///		The snipped of text the error occured in
	/// </summary>
	public CharacterSnippedLocation Snipped { get; private set; }

	/// <summary>
	///		Renderes the Snipped with an indicator
	/// </summary>
	/// <returns></returns>
	public string Render()
	{
		string posMarker;
		if (Snipped.Character - 1 > 0)
		{
			posMarker = Enumerable.Repeat("-", Snipped.Character - 1).Aggregate((e, f) => e + f) + "^";
		}
		else
		{
			posMarker = "^";
		}

		return $"{Snipped.Snipped}" +
			Environment.NewLine +
			posMarker;

	}

	/// <summary>
	///		The line of the Template
	/// </summary>
	public int Line { get; }

	/// <summary>
	///		The Character at the <see cref="Line"/>
	/// </summary>
	public int Character { get; }

	/// <inheritdoc />
	public bool Equals(CharacterLocationExtended other)
	{
		return Snipped.Equals(other.Snipped) && Line == other.Line && Character == other.Character;
	}
		
	/// <inheritdoc />
	public bool Equals(CharacterLocation other)
	{
		return Line == other.Line && Character == other.Character;
	}
		
	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is CharacterLocationExtended other && Equals(other);
	}
		
	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (Line * 397) ^ Character;
		}
	}
}