using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
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
	///		Emits the template as long as the condition is true
	/// </summary>
	[System.Serializable]
	public class DoLoopDocumentItem : ExpressionDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal DoLoopDocumentItem()
		{

		}

		/// <inheritdoc />
		public DoLoopDocumentItem(IMorestachioExpression value)
		{
			MorestachioExpression = value;
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected DoLoopDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{

		}
		
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var index = 0;
			while (ContinueBuilding(outputStream, context))
			{
				var collectionContext = new ContextCollection(index++, false, context.Options, context.Key,
					context.Parent, context.Value);
				
				//TODO get a way how to execute this on the caller
				await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, collectionContext, scopeData);

				if (!(await MorestachioExpression.GetValue(collectionContext, scopeData)).Exists())
				{
					break;
				}
			}
			return new DocumentItemExecution[0];
		}

		/// <inheritdoc />
		public override string Kind { get; } = nameof(DoLoopDocumentItem);
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}