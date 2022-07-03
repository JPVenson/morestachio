using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items.Base;

/// <summary>
///     Base class for Document items
/// </summary>
[Serializable]
public abstract class DocumentItemBase : IMorestachioDocument,
										IEquatable<DocumentItemBase>
{
	internal DocumentItemBase()
	{
		this.Location = TextRange.Unknown;
	}

	/// <summary>
	///		Creates a new base object for encapsulating document items
	/// </summary>
	protected DocumentItemBase(in TextRange location, IEnumerable<ITokenOption> tagCreationOptions)
	{
		Location = location;
		TagCreationOptions = tagCreationOptions;
	}

	/// <summary>
	///		Creates a new DocumentItemBase from a Serialization context
	/// </summary>
	/// <param name="info"></param>
	/// <param name="c"></param>
	protected DocumentItemBase(SerializationInfo info, StreamingContext c)
	{
		Location = TextRangeSerializationHelper.ReadTextRangeFromBinary(nameof(Location), info, c);
		TagCreationOptions =
			info.GetValue(nameof(TagCreationOptions), typeof(IEnumerable<ITokenOption>)) as IEnumerable<ITokenOption>;
	}


	/// <inheritdoc />
	public virtual bool Equals(DocumentItemBase other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		if (!Location.Equals(other.Location))
		{
			return false;
		}
		if (Equals(TagCreationOptions, other.TagCreationOptions))
		{
			return true;
		}

		return (TagCreationOptions?.SequenceEqual(other.TagCreationOptions) ?? false);
	}

	/// <inheritdoc />
	public abstract ItemExecutionPromise Render(IByteCounterStream outputStream,
												ContextObject context,
												ScopeData scopeData);

	/// <inheritdoc />
	public TextRange Location { get; private set; }

	/// <inheritdoc />
	public IEnumerable<ITokenOption> TagCreationOptions { get; set; }

	/// <inheritdoc />
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		TextRangeSerializationHelper.WriteTextRangeExtendedToBinary(nameof(Location), info, context, Location);
		info.AddValue(nameof(TagCreationOptions), TagCreationOptions);
		SerializeBinaryCore(info, context);
	}

	/// <summary>
	///		Gets the desired name for xml serialization
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	protected static string GetSerializedMarkerName(Type type)
	{
		return type.Name;
	}
	
	protected virtual void SerializeXmlHeaderCore(XmlWriter writer)
	{
		TextRangeSerializationHelper.WriteTextRangeToXml(writer, Location, "Location");
	}
	
	protected virtual void DeSerializeXmlHeaderCore(XmlReader reader)
	{
		Location = TextRangeSerializationHelper.ReadTextRangeFromXml(reader, "Location");
	}
	
	protected virtual void SerializeXmlBodyCore(XmlWriter writer)
	{
		writer.WriteOptions(TagCreationOptions, nameof(TagCreationOptions));
	}
	
	protected virtual void DeSerializeXmlBodyCore(XmlReader reader)
	{
		if (!reader.IsEmptyElement)
		{
			reader.ReadStartElement();
			TagCreationOptions = reader.ReadOptions(nameof(TagCreationOptions));
		}
	}

	void IDocumentItem.SerializeToXml(XmlWriter writer)
	{
		writer.WriteStartElement(GetSerializedMarkerName(GetType()));
		SerializeXmlHeaderCore(writer);
		SerializeXmlBodyCore(writer);
		writer.WriteEndElement(); //GetType().Name
	}

	void IDocumentItem.DeserializeFromXml(XmlReader reader)
	{
		var serializedMarkerName = GetSerializedMarkerName(GetType());
		AssertElement(reader, serializedMarkerName);
		DeSerializeXmlHeaderCore(reader);
		DeSerializeXmlBodyCore(reader);
		if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(serializedMarkerName))
		{
			//there are no children and we have reached the end of the document
			reader.ReadEndElement(); //GetType().Name
		}
	}

	/// <inheritdoc />
	public abstract void Accept(IDocumentItemVisitor visitor);

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		var xmlReaderSettings = reader.Settings?.Clone() ?? new XmlReaderSettings();
		xmlReaderSettings.IgnoreWhitespace = true;
		xmlReaderSettings.IgnoreComments = true;
		xmlReaderSettings.IgnoreProcessingInstructions = true;
		var xmlReader = XmlReader.Create(reader, xmlReaderSettings);
		xmlReader.Read();
		((IDocumentItem)this).DeserializeFromXml(xmlReader);
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		SerializeXmlHeaderCore(writer);
		SerializeXmlBodyCore(writer);
	}

	/// <summary>
	///     Can be called to check if any stop is requested. If return true no stop is requested
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ContinueBuilding(IByteCounterStream builder, ScopeData scopeData)
	{
		return !builder.ReachedLimit && !scopeData.CancellationToken.IsCancellationRequested;

		//return (scopeData.HasCancellationToken && !scopeData.CancellationToken.IsCancellationRequested) &&
		//	   !builder.ReachedLimit;
	}

	/// <summary>
	///     Can be overwritten to extend the binary serialization process
	/// </summary>
	/// <param name="info"></param>
	/// <param name="context"></param>
	protected virtual void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
	}

	/// <summary>
	///		Internal Only
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="elementName"></param>
	protected internal static void AssertElement(XmlReader reader, string elementName)
	{
		if (!reader.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase))
		{
			throw new XmlSchemaException($"Unexpected Element '{reader.Name}' expected '{elementName}'");
		}
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

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((DocumentItemBase)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Location.GetHashCode();
			hashCode = (hashCode * 397) ^ (TagCreationOptions.Any() ? TagCreationOptions.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
			return hashCode;
		}
	}
}