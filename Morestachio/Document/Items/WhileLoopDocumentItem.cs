#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Emits the template as long as the condition is true
	/// </summary>
	[Serializable]
	public class WhileLoopDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal WhileLoopDocumentItem()
		{

		}

		/// <inheritdoc />
		public WhileLoopDocumentItem(CharacterLocation location,
			 IMorestachioExpression value,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, value, tagCreationOptions)
		{

		}

		/// <inheritdoc />
		
		protected WhileLoopDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{

		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var index = 0;

			var collectionContext = new ContextCollection(index, false, context.Key, context.Parent,
				context.Value);

			while (ContinueBuilding(outputStream, scopeData) && (await MorestachioExpression.GetValue(collectionContext, scopeData)).Exists())
			{
				//TODO get a way how to execute this on the caller
				await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, collectionContext, scopeData);
				collectionContext = new ContextCollection(++index, false, context.Key, context.Parent, context.Value);
			}
			return Enumerable.Empty<DocumentItemExecution>();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <param name="compiler"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler)
		{
			var children = compiler.Compile(Children);
			var expression = MorestachioExpression.Compile();

			return async (outputStream, context, scopeData) =>
			{
				var index = 0;

				var collectionContext = new ContextCollection(index, false, context.Key,
					context.Parent,
					context.Value);

				while (ContinueBuilding(outputStream, scopeData) &&
					   (await expression(collectionContext, scopeData)).Exists())
				{
					await children(outputStream, collectionContext, scopeData);
					collectionContext = new ContextCollection(++index, false, context.Key,
						context.Parent, context.Value);
				}
			};
		}
	}
}