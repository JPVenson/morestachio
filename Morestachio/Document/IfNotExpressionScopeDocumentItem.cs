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
	public class IfNotExpressionScopeDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IfNotExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public IfNotExpressionScopeDocumentItem(string value)
		{
			Value = value;
		}

		[UsedImplicitly]
		protected IfNotExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "IFNotExpressionScope";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, 
			ContextObject context, 
			ScopeData scopeData)
		{
			//we are checking the parent value not our current value
			var contextObject = context.Parent ?? context;
			var c = await context.GetContextForPath(Value, scopeData);
			if (!await c.Exists())
			{
				scopeData.ExecuteElse = false;
				return Children.WithScope(contextObject.FindNextNaturalContextObject());
			}

			scopeData.ExecuteElse = true;
			return new DocumentItemExecution[0];
		}
	}
}