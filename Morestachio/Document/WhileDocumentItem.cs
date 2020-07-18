using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;

namespace Morestachio.Document
{
	/// <summary>
	///		Emits the template as long as the condition is true
	/// </summary>
	[System.Serializable]
	public class WhileLoopDocumentItem : ExpressionDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal WhileLoopDocumentItem()
		{

		}

		/// <inheritdoc />
		public WhileLoopDocumentItem(IMorestachioExpression value)
		{
			MorestachioExpression = value;
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected WhileLoopDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			
		}
		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var index = 0;

			var collectionContext = new ContextCollection(index, false, context.Options, context.Key, context.Parent,
				context.Value);

			while (ContinueBuilding(outputStream, context) && await (await MorestachioExpression.GetValue(collectionContext, scopeData)).Exists())
			{
				//TODO get a way how to execute this on the caller
				await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, collectionContext, scopeData);
				collectionContext = new ContextCollection(++index, false, context.Options, context.Key, context.Parent, context.Value);
			}
			return new DocumentItemExecution[0];
		}

		/// <inheritdoc />
		public override string Kind { get; } = nameof(WhileLoopDocumentItem);
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}