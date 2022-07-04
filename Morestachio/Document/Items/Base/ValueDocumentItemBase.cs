using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using Morestachio.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items.Base;

/// <summary>
///		A common base class for emitting a single string value
/// </summary>
[Serializable]
public abstract class ValueDocumentItemBase : BlockDocumentItemBase, IEquatable<ValueDocumentItemBase>
{
	internal ValueDocumentItemBase()
	{

	}
	/// <summary>
	/// 
	/// </summary>
	protected ValueDocumentItemBase(in TextRange location, string value,
									IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{
		Value = value;
	}

	/// <summary>
	///		A value from the Template
	/// </summary>
	public string Value { get; private set; }
		
	/// <inheritdoc />
	protected ValueDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		Value = info.GetValueOrDefault<string>(c, nameof(Value));
	}
		
	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		info.AddValue(nameof(Value), Value);
	}

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);
		if (string.IsNullOrEmpty(Value))
		{
			return;
		}
		writer.WriteStartElement(nameof(Value));
		writer.WriteAttributeString("xml", "space", null, "preserve");
		writer.WriteString(Value);
		writer.WriteEndElement();
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);

		if (!reader.IsEmptyElement)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.Name == GetSerializedMarkerName(GetType()))
			{
				reader.ReadStartElement();
			}
			if (reader.Name == nameof(Value))
			{
				if (reader.IsEmptyElement)
				{
					return;
				}
				Value = reader.ReadString();
				reader.ReadEndElement();
			}
		}
	}

	/// <inheritdoc />
	public bool Equals(ValueDocumentItemBase other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		if (!base.Equals(other))
		{
			return false;
		}

		if(string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(other.Value))
		{
			return true;
		}

		return Value == other.Value;
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

		return Equals((ValueDocumentItemBase)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		int hashCode = base.GetHashCode();
		hashCode = (hashCode * 397) ^ (!string.IsNullOrWhiteSpace(Value) ? Value.GetHashCode() : 0);
		return hashCode;
	}
}