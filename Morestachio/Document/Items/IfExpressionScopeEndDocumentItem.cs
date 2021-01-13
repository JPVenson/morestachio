using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Virtual document item for setting the <see cref="ScopeData.ExecuteElse"/> flag to false
	/// </summary>
	public class IfExpressionScopeEndDocumentItem : DocumentItemBase
	{
		/// <inheritdoc />
		public IfExpressionScopeEndDocumentItem()
		{
			
		}
		
		/// <inheritdoc />
		public override Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.ExecuteElse = false;
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}
		
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}
}