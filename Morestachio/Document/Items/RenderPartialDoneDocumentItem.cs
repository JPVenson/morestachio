#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
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
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		The end of a Partial declaration
	/// </summary>
	[System.Serializable]
	public class RenderPartialDoneDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RenderPartialDoneDocumentItem()
		{

		}

		/// <inheritdoc />
		public RenderPartialDoneDocumentItem([NotNull] string partialName)
		{
			Value = partialName ?? throw new ArgumentNullException(nameof(partialName));
		}

		[UsedImplicitly]
		protected RenderPartialDoneDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			scopeData.PartialDepth.Pop();
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}