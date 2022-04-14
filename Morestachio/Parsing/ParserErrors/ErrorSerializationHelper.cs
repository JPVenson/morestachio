using System.Xml;

namespace Morestachio.Parsing.ParserErrors;

internal static class ErrorSerializationHelper
{
	public static CharacterLocationExtended ReadCharacterLocationExtendedFromXml(XmlReader reader)
	{
		var line = reader.GetAttribute(nameof(CharacterLocationExtended.Line));
		var character = reader.GetAttribute(nameof(CharacterLocationExtended.Character));
		reader.ReadStartElement();
		var snipped = ReadCharacterSnippedLocationFromXml(reader);

		if (!reader.IsEmptyElement)
		{
			reader.ReadEndElement();
		}
		return new CharacterLocationExtended(int.Parse(line), int.Parse(character), snipped);
	}

	public static CharacterSnippedLocation ReadCharacterSnippedLocationFromXml(XmlReader reader)
	{
		var line = reader.GetAttribute(nameof(CharacterSnippedLocation.Line));
		var character = reader.GetAttribute(nameof(CharacterSnippedLocation.Character));
		var snipped = reader.GetAttribute(nameof(CharacterSnippedLocation.Snipped));
		return new CharacterSnippedLocation(int.Parse(line), int.Parse(character), snipped);
	}

	public static void WriteCharacterLocationExtendedFromXml(XmlWriter writer, CharacterLocationExtended location)
	{
		writer.WriteAttributeString(nameof(CharacterLocationExtended.Line), location.Line.ToString());
		writer.WriteAttributeString(nameof(CharacterLocationExtended.Character), location.Character.ToString());
		writer.WriteStartElement(nameof(CharacterLocationExtended.Snipped));
		WriteCharacterSnippedLocationFromXml(writer, location.Snipped);
		writer.WriteEndElement();
	}

	public static void WriteCharacterSnippedLocationFromXml(XmlWriter writer, CharacterSnippedLocation snipped)
	{
		writer.WriteAttributeString(nameof(CharacterSnippedLocation.Line), snipped.Line.ToString());
		writer.WriteAttributeString(nameof(CharacterSnippedLocation.Character), snipped.Character.ToString());
		writer.WriteAttributeString(nameof(CharacterSnippedLocation.Snipped), snipped.Snipped.ToString());
	}

	private class CharacterLocationFassade
	{
		public int Line { get; set; }
		public int Character { get; set; }
		public CharacterLocationSnippedFassade Snipped { get; set; }

		public CharacterLocationExtended AsLocation()
		{
			return new CharacterLocationExtended(Line, Character, Snipped.AsSnipped());
		}
	}

	private class CharacterLocationSnippedFassade
	{
		public int Line { get; set; }
		public int Character { get; set; }
		public string Snipped { get; set; }

		public CharacterSnippedLocation AsSnipped()
		{
			return new CharacterSnippedLocation(Line, Character, Snipped);
		}
	}

	public static CharacterLocationExtended ReadCharacterLocationExtendedFromBinary(SerializationInfo info, StreamingContext c)
	{
		var fassade = info.GetValue(nameof(CharacterLocationExtended), typeof(CharacterLocationFassade)) as CharacterLocationFassade;
		return fassade.AsLocation();
	}

	public static void WriteCharacterLocationExtendedToBinary(SerializationInfo info, StreamingContext context, CharacterLocationExtended location)
	{
		info.AddValue(nameof(CharacterLocationExtended), new CharacterLocationFassade()
		{
			Line = location.Line,
			Character = location.Character,
			Snipped = new CharacterLocationSnippedFassade()
			{
				Character = location.Snipped.Character,
				Line = location.Snipped.Line,
				Snipped = location.Snipped.Snipped
			}
		});
	}
}