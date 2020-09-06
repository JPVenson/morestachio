#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System.Collections;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;

namespace Morestachio.Document.Items
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

		/// <summary>
		///		Creates a new DoLoop DocumentItem that will render its children as long as the value expression meets the <see cref="ContextObject.DefinitionOfFalse"/>
		/// </summary>
		/// <param name="value"></param>
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
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}