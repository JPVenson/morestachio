#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
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
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines the start of a Scope
	/// </summary>
	[Serializable]
	public class IfExpressionScopeDocumentItem : ExpressionDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IfExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public IfExpressionScopeDocumentItem(CharacterLocation location, IMorestachioExpression value,
			IEnumerable<ITokenOption> tagCreationOptions) 
			: base(location, value,tagCreationOptions)
		{
			
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected IfExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		public Compilation Compile()
		{
			var children = MorestachioDocument.CompileItemsAndChildren(Children);
			var expression = MorestachioExpression.Compile();
			return async (stream, context, scopeData) =>
			{
				var c = await expression(context, scopeData);
				if (c.Exists())
				{
					await children(stream, context.IsNaturalContext || context.Parent == null ? context : context.Parent,
						scopeData);
					scopeData.ExecuteElse = false;
					return;
				}
				scopeData.ExecuteElse = true;
			};
		}
		
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, 
			ContextObject context, 
			ScopeData scopeData)
		{
			//we are checking the parent value not our current value
			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await MorestachioExpression.GetValue(context, scopeData);
			if (c.Exists())
			{
				return Children
					.Concat(new []
					{
						new IfExpressionScopeEndDocumentItem()
					})
					.WithScope(context.IsNaturalContext || context.Parent == null ? context : context.Parent);
			}
			scopeData.ExecuteElse = true;
			return Enumerable.Empty<DocumentItemExecution>();
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	/// <summary>
	///		Virtual document item for setting the <see cref="ScopeData.ExecuteElse"/> flag to false
	/// </summary>
	public class IfExpressionScopeEndDocumentItem : DocumentItemBase
	{
		public IfExpressionScopeEndDocumentItem()
		{
			
		}

		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.ExecuteElse = false;
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}

		public override void Accept(IDocumentItemVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}
}