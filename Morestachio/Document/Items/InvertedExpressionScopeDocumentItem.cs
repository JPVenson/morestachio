﻿using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		Defines an inverted scope
/// </summary>
/// <seealso cref="ExpressionScopeDocumentItem"/>
[Serializable]
public class InvertedExpressionScopeDocumentItem : BlockExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal InvertedExpressionScopeDocumentItem()
	{
	}

	/// <inheritdoc />
	public InvertedExpressionScopeDocumentItem(TextRange location,
												IMorestachioExpression value,
												IEnumerable<ITokenOption> tagCreationOptions) : base(location, value,
		tagCreationOptions)
	{
	}

	/// <inheritdoc />
	protected InvertedExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var children = compiler.Compile(Children, parserOptions);
		var expression = MorestachioExpression.Compile(parserOptions);

		return async (stream, context, scopeData) =>
		{
			var c = await expression(context, scopeData).ConfigureAwait(false);

			if (!c.Exists())
			{
				await children(stream, c, scopeData).ConfigureAwait(false);
			}
		};
	}

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		//var c = await context.GetContextForPath(Value, scopeData);
		var c = await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false);

		if (!c.Exists())
		{
			return Children.WithScope(c);
		}

		return Enumerable.Empty<DocumentItemExecution>();
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}
}