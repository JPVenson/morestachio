using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Contains the Declaration of a Partial item
	/// </summary>
	[System.Serializable]
	public class PartialDocumentItem : ValueDocumentItemBase
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
		public PartialDocumentItem(string partialName, IDocumentItem partial)
		{
			Value = partialName;
			Partial = partial;
		}

		[UsedImplicitly]
		protected PartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Partial = info.GetValue(nameof(Partial), typeof(IDocumentItem)) as IDocumentItem;
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Partial), Partial, Partial.GetType());
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			writer.WriteStartElement(nameof(Partial));
			Partial.SerializeXmlCore(writer);
			writer.WriteEndElement();//nameof(Partial)
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			reader.ReadStartElement();
			var child = DocumentExtenstions.CreateDocumentItemInstance(reader.Name);
			var childTree = reader.ReadSubtree();
			childTree.Read();
			child.DeSerializeXmlCore(childTree);
			reader.Skip();
			reader.ReadEndElement();
			Partial = child;
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Partial";

		/// <summary>
		///		The partial Document
		/// </summary>
		public IDocumentItem Partial { get; private set; }

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			scopeData.Partials[Value] = Partial;
			await Task.CompletedTask;
			return new DocumentItemExecution[0];
		}
	}
}