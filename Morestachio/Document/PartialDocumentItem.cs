using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Helper;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif
namespace Morestachio.Document
{
	/// <summary>
	///		Contains the Declaration of a Partial item
	/// </summary>
	[System.Serializable]
	public class PartialDocumentItem : ValueDocumentItemBase, IEquatable<PartialDocumentItem>
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PartialDocumentItem()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PartialDocumentItem"/> class.
		/// </summary>
		/// <param name="partialName">The partial name.</param>
		/// <param name="partial">The partial.</param>
		public PartialDocumentItem([NotNull] string partialName, [NotNull] IDocumentItem partial)
		{
			Value = partialName ?? throw new ArgumentNullException(nameof(partialName));
			Partial = partial ?? throw new ArgumentNullException(nameof(partial));
		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected PartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Partial = info.GetValue(nameof(Partial), typeof(IDocumentItem)) as IDocumentItem;
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Partial), Partial, Partial.GetType());
		}

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			writer.WriteStartElement(nameof(Partial));
			Partial.SerializeXmlCore(writer);
			writer.WriteEndElement();//nameof(Partial)
		}

		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);

			AssertElement(reader, nameof(Value));
			reader.ReadEndElement();

			AssertElement(reader, nameof(Partial));
			reader.ReadStartElement();
			var child = DocumentExtenstions.CreateDocumentItemInstance(reader.Name);
			var childTree = reader.ReadSubtree();
			childTree.Read();
			child.DeSerializeXmlCore(childTree);
			reader.Skip();
			Partial = child;
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Partial";

		/// <summary>
		///		The partial Document
		/// </summary>
		public IDocumentItem Partial { get; private set; }

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			scopeData.Partials[Value] = Partial;
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}

		/// <inheritdoc />
		public bool Equals(PartialDocumentItem other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && Partial.Equals(other.Partial);
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

			return Equals((PartialDocumentItem) obj);
		}
		
		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (Partial != null ? Partial.GetHashCode() : 0);
				return hashCode;
			}
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}