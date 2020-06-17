using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///     Base class for Document items
	/// </summary>
	[Serializable]
	public abstract class DocumentItemBase : IMorestachioDocument, IEquatable<DocumentItemBase>
	{
		/// <summary>
		///		Creates a new base object for encapsulating document items
		/// </summary>
		protected DocumentItemBase()
		{
			Children = new List<IDocumentItem>();
		}

		/// <summary>
		///		Creates a new DocumentItemBase from a Serialization context
		/// </summary>
		/// <param name="info"></param>
		/// <param name="c"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected DocumentItemBase(SerializationInfo info, StreamingContext c)
		{
			var documentItemBases = info.GetValue(nameof(Children), typeof(IDocumentItem[])) as IDocumentItem[];
			Children = new List<IDocumentItem>(documentItemBases ?? throw new InvalidOperationException());
			var expStartLocation = info.GetString(nameof(ExpressionStart));
			if (!string.IsNullOrWhiteSpace(expStartLocation))
			{
				ExpressionStart = CharacterLocation.FromFormatString(expStartLocation);
			}
		}


		/// <inheritdoc />
		public bool Equals(DocumentItemBase other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Children.SequenceEqual(other.Children)
			       && (ReferenceEquals(ExpressionStart, other.ExpressionStart) ||
			           ExpressionStart.Equals(other.ExpressionStart))
			       && string.Equals(Kind, other.Kind);
		}

		/// <inheritdoc />
		public abstract Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData);

		/// <inheritdoc />
		public abstract string Kind { get; }

		/// <inheritdoc />
		public IList<IDocumentItem> Children { get; internal set; }

		/// <inheritdoc />
		public CharacterLocation ExpressionStart { get; set; }

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
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(Kind), Kind);
			info.AddValue(nameof(ExpressionStart), ExpressionStart?.ToFormatString());
			SerializeBinaryCore(info, context);
			info.AddValue(nameof(Children), Children.ToArray(), typeof(IDocumentItem[]));
		}

		/// <inheritdoc />
		void IDocumentItem.SerializeXmlCore(XmlWriter writer)
		{
			writer.WriteStartElement(GetType().Name);
			writer.WriteAttributeString(nameof(Kind), Kind);
			if (ExpressionStart != null)
			{
				writer.WriteAttributeString(nameof(ExpressionStart), ExpressionStart?.ToFormatString() ?? string.Empty);
			}

			SerializeXml(writer);
			if (Children.Any())
			{
				writer.WriteStartElement(nameof(Children));
				foreach (var documentItem in Children)
				{
					documentItem.SerializeXmlCore(writer);
				}

				writer.WriteEndElement(); //nameof(Children)	
			}

			writer.WriteEndElement(); //GetType().Name
		}

		/// <inheritdoc />
		void IDocumentItem.DeSerializeXmlCore(XmlReader reader)
		{
			AssertElement(reader, GetType().Name);

			var charLoc = reader.GetAttribute(nameof(ExpressionStart));
			if (charLoc != null)
			{
				ExpressionStart = CharacterLocation.FromFormatString(charLoc);
			}

			if (!reader.IsEmptyElement)
			{
				var readSubtree = reader.ReadSubtree();
				readSubtree.Read();
				DeSerializeXml(readSubtree);

				if (reader.Name == "Children" || reader.ReadToFollowing(nameof(Children)))
				{
					reader.ReadStartElement(); //nameof(Children)
					while (!reader.Name.Equals(nameof(Children)) && reader.NodeType != XmlNodeType.EndElement)
					{
						var child = DocumentExtenstions.CreateDocumentItemInstance(reader.Name);

						var childTree = reader.ReadSubtree();
						childTree.Read();
						child.DeSerializeXmlCore(childTree);
						reader.Skip();
						Children.Add(child);
					}

					reader.ReadEndElement(); //nameof(Children)
				}

				if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(GetType().Name))
				{
					//there are no children and we have reached the end of the document
					reader.ReadEndElement(); //GetType().Name
				}
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
			((IDocumentItem) this).DeSerializeXmlCore(xmlReader);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(Kind), Kind);
			SerializeXml(writer);
			if (Children.Any())
			{
				writer.WriteStartElement(nameof(Children));
				foreach (var documentItem in Children)
				{
					documentItem.SerializeXmlCore(writer);
				}

				writer.WriteEndElement(); //nameof(Children)	
			}

			//writer.WriteEndElement(); //GetType().Name
		}

		/// <summary>
		///     Can be called to check if any stop is requested. If return true no stop is requested
		/// </summary>
		protected static bool ContinueBuilding(IByteCounterStream builder, ContextObject context)
		{
			return !context.AbortGeneration && !context.CancellationToken.IsCancellationRequested &&
			       !builder.ReachedLimit;
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
		///     Should be overwritten when using custom properties in deviated document items to add the necessary xml information.
		///     When using this method it is ensured that there is already a distinct XML node present. You should not close this
		///     node and always exit before leaving it.
		///     This method will be called right after writing the document node and before writing the children of this node
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void SerializeXml(XmlWriter writer)
		{
		}

		/// <summary>
		///     Will be called to deserialize custom properties. See <see cref="SerializeXml" /> for further info.
		/// </summary>
		/// <param name="reader"></param>
		protected virtual void DeSerializeXml(XmlReader reader)
		{
		}

		/// <summary>
		///		Internal Only
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="elementName"></param>
		protected internal static void AssertElement(XmlReader reader, string elementName)
		{
			if (!reader.Name.Equals(elementName, StringComparison.InvariantCultureIgnoreCase))
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

			return Equals((DocumentItemBase) obj);
		}
		
		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Children.Any() ? Children.Select(f => f.GetHashCode()).Aggregate((e,f) => e ^ f) : 0) * 397) ^
				       (ExpressionStart != null ? ExpressionStart.GetHashCode() : 0) ^
				       Kind.GetHashCode();
			}
		}
	}
}