﻿using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		The end of a Partial declaration
/// </summary>
[Serializable]
public class RenderPartialDoneDocumentItem : ValueDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal RenderPartialDoneDocumentItem()
	{
	}

	/// <inheritdoc />
	public RenderPartialDoneDocumentItem(TextRange location,
										string partialName,
										IEnumerable<ITokenOption> tagCreationOptions) : base(location, partialName,
		tagCreationOptions)
	{
	}

	/// <inheritdoc />
	protected RenderPartialDoneDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
	}

	/// <inheritdoc />
	public override ItemExecutionPromise Render(IByteCounterStream outputStream,
												ContextObject context,
												ScopeData scopeData)
	{
		CoreAction(scopeData);
		return Enumerable.Empty<DocumentItemExecution>().ToPromise();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CoreAction(ScopeData scopeData)
	{
		scopeData.PartialDepth.Pop();

		if (!scopeData.PartialDepth.Any())
		{
			scopeData.RemoveVariable("$name", 0);
			scopeData.RemoveVariable("$recursion", 0);
		}
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
		return async (stream, context, scopeData) =>
		{
			CoreAction(scopeData);
			await AsyncHelper.FakePromise().ConfigureAwait(false);
		};
	}
}