using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines a range of characters within an template
/// </summary>
public struct TextRange
{
	public TextRange(TextIndex rangeStart, TextIndex rangeEnd)
	{
		RangeStart = rangeStart;
		RangeEnd = rangeEnd;
	}

	public TextIndex RangeStart { get; }
	public TextIndex RangeEnd { get; }

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

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{RangeStart} / {RangeEnd}";
	}

	public static TextRange All(string text)
	{
		return new TextRange(TextIndex.Start, TextIndex.End(text));
	}
}
/// <summary>
///		Defines a range of characters within an template
/// </summary>
public struct TextRangeContent
{
	public TextRangeContent(TextRange textRange, string text)
	{
		TextRange = textRange;
		Text = text;
	}

	public TextRange TextRange { get; }
	public string Text { get; }
}

public struct TextIndex
{
	public static readonly TextIndex Start = new TextIndex(0, 0, 0);

	public TextIndex(int index, int row, int column)
	{
		Index = index;
		Row = row;
		Column = column;
	}

	public int Index { get; }
	public int Row { get; }
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
		return $"{Row}:{Column}-{Index}";
	}

	public static TextIndex End(string text)
	{
		if (text.IndexOf('\n') == -1)
		{
			return new TextIndex(text.Length, 0, text.Length);
		}

		return GetIndex(Tokenizer.FindNewLines(text), text.Length);
	}
}
