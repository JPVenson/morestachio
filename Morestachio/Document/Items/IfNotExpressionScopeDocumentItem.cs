#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

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
		internal IfNotExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public IfNotExpressionScopeDocumentItem(CharacterLocation location, IMorestachioExpression value,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, value, tagCreationOptions)
		{
		}

		/// <inheritdoc />
		
		protected IfNotExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await MorestachioExpression.GetValue(context, scopeData);
			if (!c.Exists())
			{
				scopeData.ExecuteElse = false;
				return Children.WithScope(context.FindNextNaturalContextObject());
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
			var expression = MorestachioExpression.Compile();
			return async (stream, context, scopeData) =>
			{
				//var c = await context.GetContextForPath(Value, scopeData);
				var c = await expression(context, scopeData);
				if (!c.Exists())
				{
					scopeData.ExecuteElse = false;
					await children(stream, context.FindNextNaturalContextObject(), scopeData);
					return;
				}

				scopeData.ExecuteElse = true;
			};
		}
	}
}