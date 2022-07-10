using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.TextOperations;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
/// 
/// </summary>
[Serializable]
public class TextEditDocumentItem : DocumentItemBase, ISupportCustomCompilation
{
	/// <summary>
	///		The TextOperation
	/// </summary>
	public ITextOperation Operation { get; private set; }

	/// <summary>
	///		If set to true, indicates that this text operation is used as an appendix or suffix to another keyword
	/// </summary>
	public EmbeddedInstructionOrigin EmbeddedInstructionOrigin { get; private set; }

	internal TextEditDocumentItem()
	{

	}

	/// <summary>
	/// 
	/// </summary>
	public TextEditDocumentItem(TextRange location,
								ITextOperation operation,
								EmbeddedInstructionOrigin embeddedInstructionOrigin,
								IEnumerable<ITokenOption> tagCreationOptions)
		: base(location, tagCreationOptions)
	{
		Operation = operation ?? throw new ArgumentNullException(nameof(operation));
		EmbeddedInstructionOrigin = embeddedInstructionOrigin;
	}

	/// <inheritdoc />
		
	protected TextEditDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		EmbeddedInstructionOrigin = (EmbeddedInstructionOrigin)info.GetValue(nameof(EmbeddedInstructionOrigin), typeof(EmbeddedInstructionOrigin));
		Operation = info.GetValue(nameof(Operation), typeof(ITextOperation)) as ITextOperation;
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public Compilation Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		return (outputStream, context, scopeData) =>
		{
			CoreAction(outputStream, scopeData);
		};
	}

	/// <inheritdoc />
	public override ItemExecutionPromise Render(
		IByteCounterStream outputStream,
		ContextObject context,
		ScopeData scopeData)
	{
		CoreAction(outputStream, scopeData);
		return Enumerable.Empty<DocumentItemExecution>().ToPromise();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CoreAction(IByteCounterStream outputStream, ScopeData scopeData)
	{
		if (Operation.IsModificator)
		{
			throw new MorestachioRuntimeException("Cannot execute a Text-Modification on its own.");
		}
		else
		{
			outputStream.Write(Operation.Apply(string.Empty));
		}
	}

	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		info.AddValue(nameof(Operation), Operation);
		info.AddValue(nameof(EmbeddedInstructionOrigin), EmbeddedInstructionOrigin);
	}

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);
		writer.WriteStartElement("TextOperation");
		writer.WriteAttributeString(nameof(ITextOperation.TextOperationType), Operation.TextOperationType.ToString());
		writer.WriteAttributeString(nameof(EmbeddedInstructionOrigin), EmbeddedInstructionOrigin.ToString());
		Operation.WriteXml(writer);
		writer.WriteEndElement(); //</TextOperation>
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);
		
		reader.ReadStartElement(); //<TextOperation>
		AssertElement(reader, "TextOperation");
		var embeddedState = reader.GetAttribute(nameof(EmbeddedInstructionOrigin));
		if (!string.IsNullOrEmpty(embeddedState))
		{
			EmbeddedInstructionOrigin = (EmbeddedInstructionOrigin)Enum.Parse(typeof(EmbeddedInstructionOrigin), embeddedState);
		}

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
		reader.ReadEndElement(); //</TextOperation>
	}
	

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}
}