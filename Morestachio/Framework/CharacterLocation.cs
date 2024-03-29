﻿//using System;
//using Morestachio.Framework.Expression.Framework;
//using Morestachio.Parsing.ParserErrors;

//namespace Morestachio.Framework;

///// <summary>
/////		Describes an Position within the Template
///// </summary>
//public readonly struct TextRange : IEquatable<TextRange>, IComparable<TextRange>
//{
//	private readonly int _index;
//	private readonly int _line;
//	private readonly int _character;

//	/// <summary>
//	/// 
//	/// </summary>
//	public TextRange(int line, int character, int index)
//	{
//		_line = line;
//		_character = character;
//		_index = index;
//	}

//	/// <summary>
//	///		The line of the Template
//	/// </summary>
//	public int Index
//	{
//		get { return _index; }
//	}

//	/// <summary>
//	///		The line of the Template
//	/// </summary>
//	public int Line
//	{
//		get { return _line; }
//	}

//	/// <summary>
//	///		The Character at the <see cref="Line"/>
//	/// </summary>
//	public int Character
//	{
//		get { return _character; }
//	}

//	/// <summary>
//	///		Returns the Unknown Location
//	/// </summary>
//	public static TextRange Unknown { get; } = new TextRange(-1, -1, -1);

//	internal TextRangeExtended AddWindow(CharacterSnippedLocation window)
//	{
//		return new TextRangeExtended(Line, Character, window);
//	}

//	/// <summary>
//	///		Formats the Character Location into a readable string
//	/// </summary>
//	/// <returns></returns>
//	public string ToFormatString()
//	{
//		return $"{_line}:{_character},{_index}";
//	}

//	/// <summary>
//	///		Parses the result of ToFormatString
//	/// </summary>
//	/// <param name="formatString"></param>
//	/// <returns></returns>
//	public static TextRange FromFormatString(string formatString)
//	{
//		if (!formatString.Contains(":"))
//		{
//			throw new ArgumentException("The formatstring for a TextRange is invalid");
//		}

//		var parts = formatString.Split(':');
//		var line = parts[0];
//		var charIndex = parts[1].Split(',');
//		var character = charIndex[0];
//		var index = charIndex[1];
//		var charLoc = new TextRange(int.Parse(line), int.Parse(character), int.Parse(index));
//		return charLoc;
//	}

//	/// <inheritdoc />
//	public override string ToString()
//	{
//		return $"Line: {_line}, Column: {_character}, Index: {_index}";
//	}

//	/// <summary>
//	///		Creates a copy that is length chars advanced to this location
//	/// </summary>
//	/// <param name="length"></param>
//	/// <returns></returns>
//	public TextRange Offset(int length)
//	{
//		//bug this does not take care of linebreaks!
//		return new TextRange(_line, _character + length, _index + length);
//	}

//	/// <inheritdoc />
//	public int CompareTo(TextRange other)
//	{
//		var lineComparison = Line.CompareTo(other.Line);
//		if (lineComparison != 0)
//		{
//			return lineComparison;
//		}

//		return Character.CompareTo(other.Character);
//	}

//	/// <summary>
//	///		Returns the current char position in relation to the context
//	/// </summary>
//	/// <param name="context"></param>
//	/// <returns></returns>
//	public int ToPosition(TokenzierContext context)
//	{
//		return _index;
//	}

//	/// <inheritdoc />
//	public bool Equals(TextRange other)
//	{
//		return Line == other.Line && Character == other.Character && Index == other.Index;
//	}

//	/// <inheritdoc />
//	public override bool Equals(object obj)
//	{
//		return obj is TextRange other && Equals(other);
//	}

//	/// <inheritdoc />
//	public override int GetHashCode()
//	{
//		unchecked
//		{
//			return (Line * 397) ^ Character ^ Index;
//		}
//	}
//}

