using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Executes the children with a cloned Context
	/// </summary>
	public class IsolatedContextDocumentItem : DocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IsolatedContextDocumentItem()
		{

		}

		/// <inheritdoc />
		public override string Kind { get; } = "IsolatedContext";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			context = context.Clone();
			return Children.WithScope(context);
		}
	}
}