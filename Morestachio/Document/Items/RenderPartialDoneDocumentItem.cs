#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
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
	///		The end of a Partial declaration
	/// </summary>
	[Serializable]
	public class RenderPartialDoneDocumentItem : ValueDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RenderPartialDoneDocumentItem()
		{

		}

		/// <inheritdoc />
		public RenderPartialDoneDocumentItem(CharacterLocation location, [NotNull] string partialName,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, partialName,tagCreationOptions)
		{
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected RenderPartialDoneDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			CoreAction(scopeData);
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CoreAction(ScopeData scopeData)
		{
			scopeData.PartialDepth.Pop();
			if (!scopeData.PartialDepth.Any())
			{
				scopeData.RemoveVariable("$name", 0);
				scopeData.RemoveVariable("$recursion", 0);
			}
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		public Compilation Compile()
		{
			return async (stream, context, scopeData) =>
			{
				CoreAction(scopeData);
			};
		}
	}
}