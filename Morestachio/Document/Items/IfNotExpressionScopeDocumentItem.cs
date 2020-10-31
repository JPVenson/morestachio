#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines the start of a Scope
	/// </summary>
	[Serializable]
	public class IfNotExpressionScopeDocumentItem : ExpressionDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IfNotExpressionScopeDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}

		/// <inheritdoc />
		public IfNotExpressionScopeDocumentItem(CharacterLocation location, IMorestachioExpression value) : base(location, value)
		{
		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected IfNotExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

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

		public Compilation Compile()
		{
			var children = MorestachioDocument.CompileItemsAndChildren(Children);
			return async (stream, context, scopeData) =>
			{
				//we are checking the parent value not our current value
				var contextObject = context.Parent ?? context;

				//var c = await context.GetContextForPath(Value, scopeData);
				var c = await MorestachioExpression.GetValue(contextObject, scopeData);
				if (!c.Exists())
				{
					scopeData.ExecuteElse = false;
					await children(stream, contextObject.FindNextNaturalContextObject(), scopeData);
					return;
				}

				scopeData.ExecuteElse = true;
			};
		}
	}
}