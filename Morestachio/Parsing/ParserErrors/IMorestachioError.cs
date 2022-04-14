using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Morestachio.Framework.Error;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines a Error while parsing a Template
/// </summary>
public interface IMorestachioError : IXmlSerializable, ISerializable, IEquatable<IMorestachioError>
{
	/// <summary>
	///		The location within the Template where the error occured
	/// </summary>
	CharacterLocationExtended Location { get; }

	/// <summary>
	/// Gets the exception.
	/// </summary>
	/// <returns></returns>
	Exception GetException();

	/// <summary>
	///		Gets a string that describes the Error
	/// </summary>
	string HelpText { get; }

	/// <summary>
	///		Formats the contents of the error into an <see cref="StringBuilder"/>
	/// </summary>
	/// <param name="sb"></param>
	void Format(StringBuilder sb);
}

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
	protected MorestachioErrorBase(CharacterLocationExtended location, string helpText = null)
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
		Location = ErrorSerializationHelper.ReadCharacterLocationExtendedFromBinary(info, c);
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
		reader.ReadStartElement();
		Location = ErrorSerializationHelper.ReadCharacterLocationExtendedFromXml(reader);
		
		if (!reader.IsEmptyElement)
		{
			reader.ReadEndElement();
		}
	}

	/// <inheritdoc />
	public virtual void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString(nameof(HelpText), HelpText);
		writer.WriteStartElement(nameof(Location));
		ErrorSerializationHelper.WriteCharacterLocationExtendedFromXml(writer, Location);
		writer.WriteEndElement();
	}

	/// <inheritdoc />
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(HelpText), HelpText);
		info.AddValue(nameof(Location), Location);
		ErrorSerializationHelper.WriteCharacterLocationExtendedToBinary(info, context, Location);
	}

	/// <inheritdoc />
	public CharacterLocationExtended Location { get; private set; }

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