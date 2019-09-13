using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Defines the start of a Scope
	/// </summary>
	public class IfExpressionScopeDocumentItem : DocumentItemBase
	{
		/// <inheritdoc />
		public IfExpressionScopeDocumentItem(string value)
		{
			Value = value;
		}

		/// <inheritdoc />
		public override string Kind { get; } = "IFExpressionScope";

		/// <summary>
		///		The expression for the value that should be scoped
		/// </summary>
		public string Value { get; }

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, 
			ContextObject context, 
			ScopeData scopeData)
		{
			var c = await context.GetContextForPath(Value, scopeData);
			if (await c.Exists())
			{
				return Children.WithScope(context);
			}
			return new DocumentItemExecution[0];
		}
	}
}