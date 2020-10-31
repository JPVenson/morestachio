#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Emits the template as long as the condition is true
	/// </summary>
	[Serializable]
	public class DoLoopDocumentItem : ExpressionDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal DoLoopDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}

		/// <summary>
		///		Creates a new DoLoop DocumentItem that will render its children as long as the value expression meets the <see cref="ContextObject.DefinitionOfFalse"/>
		/// </summary>
		/// <param name="value"></param>
		public DoLoopDocumentItem(CharacterLocation location, IMorestachioExpression value) : base(location, value)
		{
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected DoLoopDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{

		}

		public Compilation Compile()
		{
			var children = MorestachioDocument.CompileItemsAndChildren(Children);
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
			return new DocumentItemExecution[0];
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task CoreAction(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData, Compilation action)
		{
			var index = 0;
			while (ContinueBuilding(outputStream, context))
			{
				var collectionContext = new ContextCollection(index++, false, context.Options, context.Key,
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
}