using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Defines an inverted scope
	/// </summary>
	/// <seealso cref="ExpressionScopeDocumentItem"/>
	[System.Serializable]
	public class InvertedExpressionScopeDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal InvertedExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public InvertedExpressionScopeDocumentItem(string value)
		{
			Value = value;
		}

		[UsedImplicitly]
		protected InvertedExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "InvertedExpressionScope";

		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var c = await context.GetContextForPath(Value, scopeData);
			if (!await c.Exists())
			{
				return Children.WithScope(c);
			}
			return new DocumentItemExecution[0];
		}
	}
}