using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines a range of characters within an template
/// </summary>
[Serializable]
public readonly struct TextRange : ISerializable
{
	/// <summary>
	///		Creates a new Range within a text template
	/// </summary>
	/// <param name="rangeStart"></param>
	/// <param name="rangeEnd"></param>
	public TextRange(TextIndex rangeStart, TextIndex rangeEnd)
	{
		RangeStart = rangeStart;
		RangeEnd = rangeEnd;
	}

	public TextRange(SerializationInfo serializationInfo, StreamingContext context)
	{
		RangeEnd = (TextIndex)serializationInfo.GetValue(nameof(RangeEnd), typeof(TextIndex));
		RangeStart = (TextIndex)serializationInfo.GetValue(nameof(RangeStart), typeof(TextIndex));
	}

	/// <summary>
	///		The index where the range starts
	/// </summary>
	public TextIndex RangeStart { get; }

	/// <summary>
	///		The index where the range end
	/// </summary>
	public TextIndex RangeEnd { get; }

	/// <inheritdoc />
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(RangeStart), RangeStart);
		info.AddValue(nameof(RangeEnd), RangeEnd);
	}

	/// <summary>
	///		Defines an unknown text range
	/// </summary>
	public static readonly TextRange Unknown = new TextRange(TextIndex.Unknown, TextIndex.Unknown);

	internal static TextRange Range(TokenzierContext context, int index, int length)
	{
		return Range(context.Lines, index, length);
	}

	internal static TextRange Range(List<int> lines, int index, int length)
	{
		return new TextRange(TextIndex.GetIndex(lines, index), TextIndex.GetIndex(lines, index + length));
	}

	internal static TextRange Range(TokenzierContext context, int index, int length, TextRange within)
	{
		return Range(context.Lines, index, length, within);
	}

	internal static TextRange Range(List<int> lines, int index, int length, TextRange within)
	{
		return new TextRange(TextIndex.GetIndex(lines, within.RangeStart.Index + index), TextIndex.GetIndex(lines, within.RangeStart.Index + index + length));
	}

	internal static TextRange RangeIndex(TokenzierContext context, int startIndex, int endIndex)
	{
		return RangeIndex(context.Lines, startIndex, endIndex);
	}

	internal static TextRange RangeIndex(List<int> lines, int startIndex, int endIndex)
	{
		return new TextRange(TextIndex.GetIndex(lines, startIndex), TextIndex.GetIndex(lines, endIndex));
	}

	internal static TextRange RangeIndex(TokenzierContext context, int startIndex, int endIndex, TextRange within)
	{
		return RangeIndex(context.Lines, startIndex, endIndex, within);
	}

	internal static TextRange RangeIndex(List<int> lines, int startIndex, int endIndex, TextRange within)
	{
		return new TextRange(TextIndex.GetIndex(lines, within.RangeStart.Index + startIndex), TextIndex.GetIndex(lines, within.RangeStart.Index + endIndex));
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{RangeStart}|{RangeEnd}";
	}

	/// <summary>
	///		Helper to create a text range that spans the whole string provided
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static TextRange All(string text)
	{
		return new TextRange(TextIndex.Start, TextIndex.End(text));
	}


	/// <summary>
	///		Checks wherever the index is in this range.
	/// </summary>
	public bool Includes(TextIndex index)
	{
		return 
			RangeStart.Index <= index.Index &&
			RangeEnd.Index >= index.Index;
	}
}
/// <summary>
///		Defines a range of characters within an template
/// </summary>
public struct TextRangeContent
{
	/// <summary>
	///		Creates a new TextRange with its defining content
	/// </summary>
	/// <param name="textRange"></param>
	/// <param name="text"></param>
	public TextRangeContent(TextRange textRange, string text)
	{
		TextRange = textRange;
		Text = text;
	}

	/// <summary>
	///		The Range within the source string
	/// </summary>
	public TextRange TextRange { get; }

	/// <summary>
	///		The text within the range provided by the source template
	/// </summary>
	public string Text { get; }
}

/// <summary>
///		Defines an index within a text
/// </summary>
[Serializable]
public readonly struct TextIndex : IComparable<TextIndex>, ISerializable
{
	/// <summary>
	///		Defines an unknown index within a template
	/// </summary>
	public static readonly TextIndex Unknown = new TextIndex(-1, -1, -1);

	/// <summary>
	///		The static reference for the start of text within a string 0,0,0
	/// </summary>
	public static readonly TextIndex Start = new TextIndex(0, 0, 0);

	/// <summary>
	///		Creates a new Text index
	/// </summary>
	/// <param name="index"></param>
	/// <param name="row"></param>
	/// <param name="column"></param>
	public TextIndex(int index, int row, int column)
	{
		Index = index;
		Row = row;
		Column = column;
	}

	public TextIndex(SerializationInfo serializationInfo, StreamingContext context)
	{
		Index = serializationInfo.GetInt32(nameof(Index));
		Row = serializationInfo.GetInt32(nameof(Row));;
		Column = serializationInfo.GetInt32(nameof(Column));;
	}

	/// <inheritdoc />
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(Index), Index);
		info.AddValue(nameof(Row), Row);
		info.AddValue(nameof(Column), Column);
	}

	/// <summary>
	///		The index within the source text.
	/// </summary>
	public int Index { get; }

	/// <summary>
	///		The calculated row within the source text.
	/// </summary>
	public int Row { get; }

	/// <summary>
	///		The calculated column in the given <see cref="Row"/> within the source text.
	/// </summary>
	public int Column { get; }

	internal static TextIndex GetIndex(TokenzierContext context, int index)
	{
		return GetIndex(context.Lines, index);
	}

	internal static TextIndex GetIndex(List<int> lines, int index)
	{
		var line = lines.BinarySearch(index);
		line = line < 0 ? ~line : line;
		var charIdx = index;

		//in both of these cases, we want to increment the char index by one to account for the '\n' that is skipped in the indexes.
		if (line < lines.Count && line > 0)
		{
			charIdx = index - (lines[line - 1]);
		}
		else if (line > 0)
		{
			charIdx = index - (lines.LastOrDefault());
		}

		return new TextIndex(index, line, charIdx);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Index}:{Row}:{Column}";
	}

	/// <summary>
	///		Calculates an Index that is set at the end of the text.
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static TextIndex End(string text)
	{
		if (text.IndexOf('\n') == -1)
		{
			return new TextIndex(text.Length, 0, text.Length);
		}

		return GetIndex(Tokenizer.FindNewLines(text), text.Length);
	}

	/// <inheritdoc />
	public int CompareTo(TextIndex other)
	{
		return Index.CompareTo(other.Index);
	}
}
