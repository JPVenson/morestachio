using System.Text;
using System.Xml;
using System.Xml.Schema;
using Morestachio.Framework.Error;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Base class for morestachio errors that support serialization
/// </summary>
public abstract class MorestachioErrorBase : IMorestachioError
{
	/// <summary>
	///		Serialization constructor
	/// </summary>
	protected MorestachioErrorBase()
	{
		
	}

	/// <summary>
	///		Serialization constructor
	/// </summary>
	/// <param name="location"></param>
	/// <param name="helpText"></param>
	protected MorestachioErrorBase(TextRange location, string helpText = null)
	{
		Location = location;
		HelpText = helpText;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="info"></param>
	/// <param name="c"></param>
	protected MorestachioErrorBase(SerializationInfo info, StreamingContext c)
	{
		HelpText = info.GetString(nameof(HelpText));
		Location = TextRangeSerializationHelper.ReadTextRangeFromBinary(nameof(Location), info, c);
	}

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <inheritdoc />
	public virtual void ReadXml(XmlReader reader)
	{
		HelpText = reader.GetAttribute(nameof(HelpText));
		Location = TextRangeSerializationHelper.ReadTextRangeFromXml(reader, "Location");
		
		if (!reader.IsEmptyElement)
		{
			reader.ReadEndElement();
		}
	}

	/// <inheritdoc />
	public virtual void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString(nameof(HelpText), HelpText);
		TextRangeSerializationHelper.WriteTextRangeToXml(writer, Location, "Location");
	}

	/// <inheritdoc />
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(HelpText), HelpText);
		TextRangeSerializationHelper.WriteTextRangeExtendedToBinary(nameof(Location), info, context, Location);
	}

	/// <inheritdoc />
	public TextRange Location { get; private set; }

	/// <inheritdoc />
	public string HelpText { get; private set; }

	/// <inheritdoc />
	public virtual Exception GetException()
	{
		return new IndexedParseException(Location, HelpText);
	}

	/// <inheritdoc />
	public virtual void Format(StringBuilder sb)
	{
		sb.Append(IndexedParseException.FormatMessage(HelpText, Location));
	}

	/// <inheritdoc />
	public virtual bool Equals(IMorestachioError other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Location.Equals(other.Location) && HelpText == other.HelpText;
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

		return Equals((IMorestachioError)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (Location.GetHashCode() * 397) ^ (HelpText != null ? HelpText.GetHashCode() : 0);
		}
	}
}