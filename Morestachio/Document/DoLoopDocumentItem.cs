using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.Framework.Expression;

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
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var index = 0;
			while (ContinueBuilding(outputStream, context))
			{
				var collectionContext = new ContextCollection(index++, false, context.Options, context.Key, context.Parent)
				{
					Value = context.Value
				};
				
				//TODO get a way how to execute this on the caller
				await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, collectionContext, scopeData);

				if (!await (await MorestachioExpression.GetValue(context, scopeData)).Exists())
				{
					break;
				}
			}
			return new DocumentItemExecution[0];
		}

		/// <inheritdoc />
		public override string Kind { get; } = nameof(DoLoopDocumentItem);

	}
}