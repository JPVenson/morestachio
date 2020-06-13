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
	///		Defines the start of a Scope
	/// </summary>
	[System.Serializable]
	public class ExpressionScopeDocumentItem : ExpressionDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public ExpressionScopeDocumentItem(IExpression value)
		{
			Expression = value;
		}

		[UsedImplicitly]
		protected ExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "ExpressionScope";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await Expression.GetValue(context, scopeData);
			if (await c.Exists())
			{
				return Children.WithScope(c);
			}
			return new DocumentItemExecution[0];
		}
	}
}