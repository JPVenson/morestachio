using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items;

/// <summary>
///		Emits the template as long as the condition is true
/// </summary>
[Serializable]
public class DoLoopDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal DoLoopDocumentItem() 
	{

	}

	/// <summary>
	///		Creates a new DoLoop DocumentItem that will render its children as long as the value expression meets the <see cref="ContextObject.DefinitionOfFalse"/>
	/// </summary>
	/// <param name="value"></param>
	public DoLoopDocumentItem(CharacterLocation location,
							IMorestachioExpression value,
							IEnumerable<ITokenOption> tagCreationOptions) : base(location, value, tagCreationOptions)
	{
	}
		
	/// <inheritdoc />
		
	protected DoLoopDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{

	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var children = compiler.Compile(Children, parserOptions);
		return async (stream, context, scopeData) =>
		{
			await CoreAction(stream, context, scopeData, async (streamInner, o, data) =>
			{
				await children(streamInner, o, data);
			});
		};
	}
		
	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
	{
		await CoreAction(outputStream, context, scopeData, async (stream, o, data) =>
		{
			await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, o, scopeData);
		});
		return Array.Empty<DocumentItemExecution>();
	}
		
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private async Task CoreAction(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData, CompilationAsync action)
	{
		var index = 0;
		while (ContinueBuilding(outputStream, scopeData))
		{
			var collectionContext = new ContextCollection(index++, false, context.Key,
				context.Parent, context.Value);

			//TODO get a way how to execute this on the caller
			await action(outputStream, collectionContext, scopeData);

			if (!(await MorestachioExpression.GetValue(collectionContext, scopeData)).Exists())
			{
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}

}