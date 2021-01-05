#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

namespace Morestachio.Document.Items.Base
{
	/// <summary>
	///     Base class for Document items
	/// </summary>
	[Serializable]
	public abstract class DocumentItemBase : IMorestachioDocument,
		IEquatable<DocumentItemBase>
	{
		internal DocumentItemBase()
		{
			this.ExpressionStart = CharacterLocation.Unknown;
		}

		/// <summary>
		///		Creates a new base object for encapsulating document items
		/// </summary>
		protected DocumentItemBase(CharacterLocation location, IEnumerable<ITokenOption> tagCreationOptions)
		{
			ExpressionStart = location;
			TagCreationOptions = tagCreationOptions;
		}

		/// <summary>
		///		Creates a new DocumentItemBase from a Serialization context
		/// </summary>
		/// <param name="info"></param>
		/// <param name="c"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected DocumentItemBase(SerializationInfo info, StreamingContext c)
		{
			var expStartLocation = info.GetString(nameof(ExpressionStart));
			if (!string.IsNullOrWhiteSpace(expStartLocation))
			{
				ExpressionStart = CharacterLocation.FromFormatString(expStartLocation);
			}

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

			return (ExpressionStart.Equals(other.ExpressionStart))
			       && (TagCreationOptions == other.TagCreationOptions
			           || (TagCreationOptions?.SequenceEqual(other.TagCreationOptions) ?? false));
		}

		/// <inheritdoc />
		public abstract ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData);

		/// <inheritdoc />
		public CharacterLocation ExpressionStart { get; private set; }

		/// <inheritdoc />
		public IEnumerable<ITokenOption> TagCreationOptions { get; set; }

		/// <inheritdoc />
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(ExpressionStart), ExpressionStart.ToFormatString());
			info.AddValue(nameof(TagCreationOptions), TagCreationOptions);
			SerializeBinaryCore(info, context);
		}

		protected static string GetSerializedMarkerName(Type type)
		{
			return type.Name;
		}

		/// <inheritdoc />
		void IDocumentItem.SerializeXmlCore(XmlWriter writer)
		{
			writer.WriteStartElement(GetSerializedMarkerName(GetType()));
			writer.WriteAttributeString(nameof(ExpressionStart), ExpressionStart.ToFormatString() ?? string.Empty);
			SerializeXml(writer);
			writer.WriteOptions(TagCreationOptions, nameof(TagCreationOptions));

			writer.WriteEndElement(); //GetType().Name
		}

		/// <inheritdoc />
		void IDocumentItem.DeSerializeXmlCore(XmlReader reader)
		{
			AssertElement(reader, GetSerializedMarkerName(GetType()));

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
				TagCreationOptions = reader.ReadOptions(nameof(TagCreationOptions));

				if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(GetSerializedMarkerName(GetType())))
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
			((IDocumentItem)this).DeSerializeXmlCore(xmlReader);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			SerializeXml(writer);
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

			return Equals((DocumentItemBase)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = ExpressionStart.GetHashCode();
				hashCode = (hashCode * 397) ^ (TagCreationOptions.Any() ? TagCreationOptions.Select(f => f.GetHashCode()).Aggregate((e, f) => e ^ f) : 0);
				return hashCode;
			}
		}
	}
}