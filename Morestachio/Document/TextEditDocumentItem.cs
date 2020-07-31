using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.TextOperations;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
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
	/// 
	/// </summary>
	[Serializable]
	public class TextEditDocumentItem : DocumentItemBase
	{
		/// <summary>
		///		The TextOperation
		/// </summary>
		public ITextOperation Operation { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="operation"></param>
		public TextEditDocumentItem(ITextOperation operation)
		{
			Operation = operation;
		}

		private TextEditDocumentItem()
		{

		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected TextEditDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(
			IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			if (Operation.IsModificator)
			{
				if (!scopeData.CustomData.TryGetValue("TextOperationData", out var operationList))
				{
					operationList = new List<ITextOperation>();
					scopeData.CustomData["TextOperationData"] = operationList;
				}
				(operationList as IList<ITextOperation>).Add(Operation);
			}
			else
			{
				outputStream.Write(Operation.Apply(string.Empty));
			}

			
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}
		
		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Operation), Operation);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			base.SerializeXml(writer);
			writer.WriteStartElement("TextOperation");
			writer.WriteAttributeString(nameof(ITextOperation.TextOperationType), Operation.TextOperationType.ToString());
			Operation.WriteXml(writer);
			writer.WriteEndElement();//</TextOperation>
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			reader.ReadStartElement();//<TextOperation>
			AssertElement(reader, "TextOperation");
			var attribute = reader.GetAttribute(nameof(ITextOperation.TextOperationType));
			switch (attribute)
			{
				case "LineBreak":
					Operation = new AppendLineBreakTextOperation();
					break;
				case "TrimLineBreaks":
					Operation = new TrimLineBreakTextOperation();
					break;
				default:
					throw new InvalidOperationException($"The TextOperation '{attribute}' is invalid");
			}

			Operation.ReadXml(reader);
			reader.ReadEndElement();//</TextOperation>
		}
		
		/// <inheritdoc />
		public override string Kind { get; } = "TextOperation";

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
