#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
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
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines an else Expression. This expression MUST come ether directly or only separated by <see cref="ContentDocumentItem"/> after an <see cref="IfExpressionScopeDocumentItem"/> or an <see cref="InvertedExpressionScopeDocumentItem"/>
	/// </summary>
	[Serializable]
	public class ElseExpressionScopeDocumentItem : BlockDocumentItemBase, ISupportCustomCompilation
	{       
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ElseExpressionScopeDocumentItem()
		{
			
		}
		
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ElseExpressionScopeDocumentItem(CharacterLocation location,
			IEnumerable<ITokenOption> tagCreationOptions): base(location,tagCreationOptions)
		{
			
		}
		
		/// <inheritdoc />
		
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
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		public Compilation Compile()
		{
			var children = MorestachioDocument.CompileItemsAndChildren(Children);

			return async (stream, context, scopeData) =>
			{
				if (scopeData.ExecuteElse)
				{
					scopeData.ExecuteElse = false;
					await children(stream, context, scopeData);
				}
			};
		}
	}
}