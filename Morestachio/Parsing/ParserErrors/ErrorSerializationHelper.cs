using System.Xml;

namespace Morestachio.Parsing.ParserErrors;

internal static class ErrorSerializationHelper
{
	public static TextRange ReadTextRangeFromXml(XmlReader reader)
	{
		reader.ReadStartElement();
		reader.ReadStartElement();
		var rangeStart = ReadTextIndexFromXml(reader);
		reader.ReadStartElement();
		var rangeEnd = ReadTextIndexFromXml(reader);
		reader.ReadEndElement();
		return new TextRange(rangeStart, rangeEnd);
	}

	public static TextIndex ReadTextIndexFromXml(XmlReader reader)
	{
		var index = reader.GetAttribute(nameof(TextIndex.Index));
		var row = reader.GetAttribute(nameof(TextIndex.Row));
		var column = reader.GetAttribute(nameof(TextIndex.Column));

		return new TextIndex(int.Parse(index), int.Parse(row), int.Parse(column));
	}

	public static void WriteTextRangeToXml(XmlWriter writer, TextRange range)
	{
		writer.WriteStartElement(nameof(TextRange));
		writer.WriteStartElement(nameof(range.RangeStart));
		WriteTextIndexToXml(writer, range.RangeStart);
		writer.WriteEndElement();
		writer.WriteStartElement(nameof(range.RangeEnd));
		WriteTextIndexToXml(writer, range.RangeStart);
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void WriteTextIndexToXml(XmlWriter writer, TextIndex index)
	{
		writer.WriteAttributeString(nameof(TextRange.RangeStart.Index), index.Index.ToString());
		writer.WriteAttributeString(nameof(TextRange.RangeStart.Row), index.Row.ToString());
		writer.WriteAttributeString(nameof(TextRange.RangeStart.Column), index.Column.ToString());
	}

	private class TextRangeFassade
	{
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

	public static TextRange ReadTextRangeFromBinary(SerializationInfo info, StreamingContext c)
	{
		var fassade = info.GetValue(nameof(TextRange), typeof(TextRangeFassade)) as TextRangeFassade;
		return fassade.AsRange();
	}

	public static void WriteTextRangeExtendedToBinary(SerializationInfo info, StreamingContext context, TextRange textRange)
	{
		info.AddValue(nameof(TextRangeExtended), new TextRangeFassade()
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
		});
	}
}