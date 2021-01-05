using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items.Base
{
	/// <summary>
	///		Defines a Document item that contains an opening tag and an closing tag
	/// </summary>
	public abstract class BlockDocumentItemBase : DocumentItemBase, IBlockDocumentItem, IEquatable<BlockDocumentItemBase>
	{
		internal BlockDocumentItemBase()
		{
			Children = new List<IDocumentItem>();
		}

		/// <summary>
		///		Creates a new base object for encapsulating document items
		/// </summary>
		protected BlockDocumentItemBase(CharacterLocation location,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
		{
			Children = new List<IDocumentItem>();
		}

		/// <summary>
		///		Creates a new DocumentItemBase from a Serialization context
		/// </summary>
		/// <param name="info"></param>
		/// <param name="c"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected BlockDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			var documentItemBases = info.GetValue(nameof(Children), typeof(IDocumentItem[])) as IDocumentItem[];
			Children = new List<IDocumentItem>(documentItemBases ?? throw new InvalidOperationException());
			
			BlockClosingOptions =
				info.GetValue(nameof(BlockClosingOptions), typeof(ITokenOption[])) as IEnumerable<ITokenOption>;
		}

		/// <inheritdoc />
		public IList<IDocumentItem> Children { get; internal set; }

		/// <inheritdoc />
		public IEnumerable<ITokenOption> BlockClosingOptions { get; set; }

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

			return base.Equals(other) &&
				   Children.SequenceEqual(other.Children)
				   && (BlockClosingOptions == other.BlockClosingOptions
					   || (BlockClosingOptions?.SequenceEqual(other.BlockClosingOptions) ?? false)
					   )
				   ;
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
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Children), Children.ToArray(), typeof(IDocumentItem[]));
			info.AddValue(nameof(BlockClosingOptions), BlockClosingOptions?.ToArray(), typeof(ITokenOption[]));
		}

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			if (Children.Any())
			{
				writer.WriteStartElement(nameof(Children));
				foreach (var documentItem in Children)
				{
					documentItem.SerializeXmlCore(writer);
				}

				writer.WriteEndElement(); //nameof(Children)	
			}

			writer.WriteOptions(BlockClosingOptions, nameof(BlockClosingOptions));
		}

		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			if (reader.NodeType == XmlNodeType.Element && reader.Name == GetSerializedMarkerName(GetType()))
			{
				reader.ReadStartElement();
			}
			if (reader.Name == "Children")
			{
				reader.ReadStartElement(); //nameof(Children)
				while (!reader.Name.Equals(nameof(Children)) && reader.NodeType != XmlNodeType.EndElement)
				{
					var child = DocumentExtensions.CreateDocumentItemInstance(reader.Name);

					var childTree = reader.ReadSubtree();
					childTree.Read();
					child.DeSerializeXmlCore(childTree);
					reader.Skip();
					Children.Add(child);
				}

				reader.ReadEndElement(); //nameof(Children)
			}
			BlockClosingOptions = reader.ReadOptions(nameof(BlockClosingOptions));
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (Children.Any() ? Children.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
				hashCode = (hashCode * 397) ^ (BlockClosingOptions.Any() ? BlockClosingOptions.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
				return hashCode;
			}
		}
	}
}