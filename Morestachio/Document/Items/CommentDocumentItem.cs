using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items;

/// <summary>
///		Defines a comment within the template. Will only be produced when <see cref="IParserOptionsBuilder.WithTokenizeComments(bool)"/> is enabled.
/// </summary>
[Serializable]
public class CommentDocumentItem : ValueDocumentItemBase
{
		
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal CommentDocumentItem() 
	{

	}
		
	/// <summary>
	///		Represents a Comment
	/// </summary>
	/// <param name="location"></param>
	/// <param name="content"></param>
	/// <param name="tagCreationOptions"></param>
	/// <param name="isBlockComment"></param>
	public CommentDocumentItem(CharacterLocation location, string content,
								IEnumerable<ITokenOption> tagCreationOptions, bool isBlockComment) : base(location, content, tagCreationOptions)
	{
		IsBlockComment = isBlockComment;
	}

	/// <summary>
	///		Defines the current comment as a block of comments {{!}} {{/!}}
	/// </summary>
	public bool IsBlockComment { get; private set; }
		
	/// <inheritdoc />
	protected CommentDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		IsBlockComment = info.GetBoolean(nameof(IsBlockComment));
	}
	
	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(IsBlockComment), IsBlockComment);
		base.SerializeBinaryCore(info, context);
	}
	
	/// <inheritdoc />
	protected override void SerializeXml(XmlWriter writer)
	{
		writer.WriteAttributeString(nameof(IsBlockComment), IsBlockComment.ToString());
		base.SerializeXml(writer);
	}
	
	/// <inheritdoc />
	protected override void DeSerializeXml(XmlReader reader)
	{
		IsBlockComment = reader.GetAttribute(nameof(IsBlockComment)) == bool.TrueString;
		base.DeSerializeXml(reader);
	}

	/// <inheritdoc />
	public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
	{
		return Enumerable.Empty<DocumentItemExecution>().ToPromise();
	}
		
	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}
}