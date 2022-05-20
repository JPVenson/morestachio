using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
///		Defines an else Expression. This expression MUST come ether directly or only separated by <see cref="ContentDocumentItem"/> after an <see cref="IfExpressionScopeDocumentItem"/> or an <see cref="InvertedExpressionScopeDocumentItem"/>
/// </summary>
[Serializable]
public class ElseExpressionScopeDocumentItem : BlockDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal ElseExpressionScopeDocumentItem()
	{

	}

	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal ElseExpressionScopeDocumentItem(CharacterLocation location,
											IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{

	}

	/// <inheritdoc />

	protected ElseExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
	}

	/// <inheritdoc />
	public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
	{
		return Children.WithScope(context).ToPromise();
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var children = compiler.Compile(Children, parserOptions);

		return async (stream, context, scopeData) =>
		{
			await children(stream, context, scopeData).ConfigureAwait(false);
		};
	}
}