using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Helper;
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
	///		Defines an else Expression. This expression MUST come ether directly or only separated by <see cref="ContentDocumentItem"/> after an <see cref="IfExpressionScopeDocumentItem"/> or an <see cref="InvertedExpressionScopeDocumentItem"/>
	/// </summary>
	public class ElseExpressionScopeDocumentItem : DocumentItemBase
	{       
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		public ElseExpressionScopeDocumentItem()
		{
			
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected ElseExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			if (scopeData.ExecuteElse)
			{
				scopeData.ExecuteElse = false;
				return Children.WithScope(context).ToPromise();
			}

			scopeData.ExecuteElse = false;
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}
		
		/// <inheritdoc />
		public override string Kind { get; } = "ElseExpressionScope";

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}