using Morestachio.Parsing.ParserErrors;
using System.Text;
using System.Xml;

namespace Morestachio.Helper.Serialization;

/// <summary>
///		Contains methods for serializing <see cref="TextRange"/>
/// </summary>
public static class TextRangeSerializationHelper
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

	[Serializable]
	private class TextRangeFassade : ISerializable
	{
		public TextRangeFassade()
		{
			RangeEnd = new TextIndexFassade();
			RangeStart = new TextIndexFassade();
		}

		public TextRangeFassade(SerializationInfo serializationInfo, StreamingContext context)
		{
			RangeEnd = serializationInfo.GetValue(nameof(RangeEnd), typeof(TextIndexFassade)) as TextIndexFassade;
			RangeStart = serializationInfo.GetValue(nameof(RangeStart), typeof(TextIndexFassade)) as TextIndexFassade;
		}

		public TextIndexFassade RangeStart { get; set; }
		public TextIndexFassade RangeEnd { get; set; }

		public TextRange AsRange()
		{
			return new TextRange(RangeStart.AsTextIndex(), RangeEnd.AsTextIndex());
		}

		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(RangeStart), RangeStart);
			info.AddValue(nameof(RangeEnd), RangeEnd);
		}
	}

	[Serializable]
	private class TextIndexFassade : ISerializable
	{
		public TextIndexFassade()
		{

		}

		public TextIndexFassade(SerializationInfo serializationInfo, StreamingContext context)
		{
			Index = serializationInfo.GetInt32(nameof(Index));
			Row = serializationInfo.GetInt32(nameof(Row));
			Column = serializationInfo.GetInt32(nameof(Column));
		}
		public int Index { get; set; }
		public int Row { get; set; }
		public int Column { get; set; }

		public TextIndex AsTextIndex()
		{
			return new TextIndex(Index, Row, Column);
		}

		/// <inheritdoc />
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Index), Index);
			info.AddValue(nameof(Row), Row);
			info.AddValue(nameof(Column), Column);
		}
	}

	public static TextRange ReadTextRange(string name, SerializationInfo info, StreamingContext c)
	{
		return info.GetValueOrDefault<TextRangeFassade>(c, name ?? nameof(TextRange))?.AsRange() ?? default;
	}

	public static void WriteTextRangeToBinary(string name, SerializationInfo info, StreamingContext context, TextRange textRange)
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