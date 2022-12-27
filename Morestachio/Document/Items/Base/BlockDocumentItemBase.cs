using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items.Base;

/// <summary>
///		Defines a Document item that contains an opening tag and an closing tag
/// </summary>
public abstract class BlockDocumentItemBase : DocumentItemBase, 
											IBlockDocumentItem, 
											IEquatable<BlockDocumentItemBase>,
											IReportUsage
{
	internal BlockDocumentItemBase()
	{
		Children = new List<IDocumentItem>();
	}

	/// <summary>
	///		Creates a new base object for encapsulating document items
	/// </summary>
	protected BlockDocumentItemBase(in TextRange location,
									IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{
		Children = new List<IDocumentItem>();
	}

	/// <summary>
	///		Creates a new DocumentItemBase from a Serialization context
	/// </summary>
	/// <param name="info"></param>
	/// <param name="c"></param>
	protected BlockDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		Children = new List<IDocumentItem>(info.GetValueOrEmpty<IDocumentItem>(c, nameof(Children)));
		BlockClosingOptions = info.GetValueOrDefault<ITokenOption[]>(c, nameof(BlockClosingOptions), () => null);
		BlockLocation = TextRangeSerializationHelper.ReadTextRange(nameof(BlockLocation), info, c);
	}
	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		TextRangeSerializationHelper.WriteTextRangeToBinary(nameof(BlockLocation), info, context, BlockLocation);
		info.AddValue(nameof(Children), Children.ToArray(), typeof(IDocumentItem[]));
		info.AddValue(nameof(BlockClosingOptions), BlockClosingOptions?.ToArray(), typeof(ITokenOption[]));
	}

	/// <inheritdoc />
	protected override void SerializeXmlHeaderCore(XmlWriter writer)
	{
		base.SerializeXmlHeaderCore(writer);
		TextRangeSerializationHelper.WriteTextRangeToXml(writer, BlockLocation, nameof(BlockLocation));
	}

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);
		if (Children.Any())
		{
			writer.WriteStartElement(nameof(Children));
			foreach (var documentItem in Children)
			{
				documentItem.SerializeToXml(writer);
			}

			writer.WriteEndElement(); //nameof(Children)	
		}

		writer.WriteOptions(BlockClosingOptions, nameof(BlockClosingOptions));
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlHeaderCore(XmlReader reader)
	{
		base.DeSerializeXmlHeaderCore(reader);
		BlockLocation = TextRangeSerializationHelper.ReadTextRangeFromXml(reader, nameof(BlockLocation));
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);
		if (reader.Name == nameof(Children))
		{
			reader.ReadStartElement(); //nameof(Children)
			while (!reader.Name.Equals(nameof(Children)) && reader.NodeType != XmlNodeType.EndElement)
			{
				var child = SerializationHelper.CreateDocumentItemInstance(reader.Name);

				var childTree = reader.ReadSubtree();
				childTree.Read();
				child.DeserializeFromXml(childTree);
				reader.Skip();
				Children.Add(child);
			}

			reader.ReadEndElement(); //nameof(Children)
		}
		BlockClosingOptions = reader.ReadOptions(nameof(BlockClosingOptions));
	}
	
	/// <inheritdoc />
	public IList<IDocumentItem> Children { get; internal set; }

	/// <inheritdoc />
	public IEnumerable<ITokenOption> BlockClosingOptions { get; set; }

	/// <inheritdoc />
	public TextRange BlockLocation { get; set; }

	/// <inheritdoc />
	public virtual bool Equals(BlockDocumentItemBase other)
	{
		if (other is null)
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

		if (!Children.SequenceEqual(other.Children))
		{
			return false;
		}

		if (Equals(BlockClosingOptions, other.BlockClosingOptions))
		{
			return true;
		}

		if (Equals(BlockLocation, other.BlockLocation))
		{
			return true;
		}

		return (BlockClosingOptions?.SequenceEqual(other.BlockClosingOptions) ?? false);
	}

	/// <inheritdoc />
	public void Add(params IDocumentItem[] documentChildren)
	{
		foreach (var documentItem in documentChildren)
		{
			//documentItem.Parent = this;
			Children.Add(documentItem);
		}
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = base.GetHashCode();
			hashCode = (hashCode * 397) ^ BlockLocation.GetHashCode();
			hashCode = (hashCode * 397) ^ (Children.Any() ? Children.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
			hashCode = (hashCode * 397) ^ (BlockClosingOptions.Any() ? BlockClosingOptions.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
			return hashCode;
		}
	}

	/// <inheritdoc />
	public virtual void ReportUsage(UsageData data)
	{
		foreach (var reportUsage in Children.OfType<IReportUsage>())
		{
			reportUsage.ReportUsage(data);
		}
	}
}