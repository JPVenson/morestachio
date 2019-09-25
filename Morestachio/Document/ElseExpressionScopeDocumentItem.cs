using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

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
		internal ElseExpressionScopeDocumentItem()
		{
			
		}

		[UsedImplicitly]
		protected ElseExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			if (scopeData.ExecuteElse)
			{
				scopeData.ExecuteElse = false;
				return Children.WithScope(context);
			}

			scopeData.ExecuteElse = false;
			return new DocumentItemExecution[0];
		}

		public override string Kind { get; } = "ElseExpressionScope";
	}
}