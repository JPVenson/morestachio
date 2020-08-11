using System.Collections.Generic;
using System.Linq;
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
	///		Defines the start of a Scope
	/// </summary>
	[System.Serializable]
	public class IfNotExpressionScopeDocumentItem : ExpressionDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IfNotExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public IfNotExpressionScopeDocumentItem(IMorestachioExpression value)
		{
			MorestachioExpression = value;
		}

		[UsedImplicitly]
		protected IfNotExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "IFNotExpressionScope";

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, 
			ContextObject context, 
			ScopeData scopeData)
		{
			//we are checking the parent value not our current value
			var contextObject = context.Parent ?? context;

			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await MorestachioExpression.GetValue(contextObject, scopeData);
			if (!c.Exists())
			{
				scopeData.ExecuteElse = false;
				return Children.WithScope(contextObject.FindNextNaturalContextObject());
			}

			scopeData.ExecuteElse = true;
			return Enumerable.Empty<DocumentItemExecution>();
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}