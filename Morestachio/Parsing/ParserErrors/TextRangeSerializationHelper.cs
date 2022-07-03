using System.Text;
using System.Xml;

namespace Morestachio.Parsing.ParserErrors;

internal static class TextRangeSerializationHelper
{
	public static TextRange ReadTextRangeFromXml(XmlReader reader, string name)
	{
		var text = reader.GetAttribute(name);

		var ranges = text.Split('|')
						.Select(f => f.Split(':').ToArray())
						.ToArray();

		return new TextRange(BuildIndex(ranges[0]), BuildIndex(ranges[1]));
	}

	private static TextIndex BuildIndex(IReadOnlyList<string> range)
	{
		return new TextIndex(int.Parse(range[0]), int.Parse(range[1]), int.Parse(range[2]));
	}

	public static TextIndex ReadTextIndexFromXml(XmlReader reader)
	{
		var index = reader.GetAttribute(nameof(TextIndex.Index));
		var row = reader.GetAttribute(nameof(TextIndex.Row));
		var column = reader.GetAttribute(nameof(TextIndex.Column));

		return new TextIndex(int.Parse(index), int.Parse(row), int.Parse(column));
	}

	public static void WriteTextRangeToXml(XmlWriter writer, TextRange range, string name)
	{
		writer.WriteAttributeString(name, range.ToString());
	}

	public static void WriteTextIndexToXml(XmlWriter writer, TextIndex index)
	{
		writer.WriteAttributeString(nameof(TextRange.RangeStart.Index), index.Index.ToString());
		writer.WriteAttributeString(nameof(TextRange.RangeStart.Row), index.Row.ToString());
		writer.WriteAttributeString(nameof(TextRange.RangeStart.Column), index.Column.ToString());
	}

	private class TextRangeFassade
	{
		public TextRangeFassade()
		{
			RangeEnd = new TextIndexFassade();
			RangeStart = new TextIndexFassade();
		}

		public TextIndexFassade RangeStart { get; }
		public TextIndexFassade RangeEnd { get; }

		public TextRange AsRange()
		{
			return new TextRange(RangeStart.AsTextIndex(), RangeEnd.AsTextIndex());
		}
	}

	private class TextIndexFassade
	{
		public int Index { get; set; }
		public int Row { get; set; }
		public int Column { get; set; }

		public TextIndex AsTextIndex()
		{
			return new TextIndex(Index, Row, Column);
		}
	}

	public static TextRange ReadTextRangeFromBinary(string name, SerializationInfo info, StreamingContext c)
	{
		var fassade = info.GetValue(name ?? nameof(TextRange), typeof(TextRangeFassade)) as TextRangeFassade;
		return fassade.AsRange();
	}

	public static void WriteTextRangeExtendedToBinary(string name, SerializationInfo info, StreamingContext context, TextRange textRange)
	{
		var textRangeFassade = new TextRangeFassade()
		{
			RangeStart =
			{
				Index = textRange.RangeStart.Index,
				Row = textRange.RangeStart.Row,
				Column = textRange.RangeStart.Column,
			},
			RangeEnd =
			{
				Index = textRange.RangeEnd.Index,
				Row = textRange.RangeEnd.Row,
				Column = textRange.RangeEnd.Column,
			}
		};
		info.AddValue(name ?? nameof(TextRange), textRangeFassade);
	}
}