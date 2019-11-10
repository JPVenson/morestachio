using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Defines the start of a Scope
	/// </summary>
	[System.Serializable]
	public class IfExpressionScopeDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IfExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public IfExpressionScopeDocumentItem(string value)
		{
			Value = value;
		}

		[UsedImplicitly]
		protected IfExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "IFExpressionScope";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, 
			ContextObject context, 
			ScopeData scopeData)
		{
			//we are checking the parent value not our current value
			var contextObject = context.Parent ?? context;
			var c = await context.GetContextForPath(Value, scopeData);
			if (await c.Exists())
			{
				scopeData.ExecuteElse = false;
				return Children.WithScope(contextObject);
			}
			scopeData.ExecuteElse = true;
			return new DocumentItemExecution[0];
		}
	}
}