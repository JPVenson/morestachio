#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.TextOperations;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines a area that has no morestachio keywords and can be rendered as is
	/// </summary>
	[System.Serializable]
	public class ContentDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ContentDocumentItem()
		{

		}

		/// <summary>
		///		Creates a new ContentDocumentItem that represents some static content
		/// </summary>
		/// <param name="content">The content to write</param>
		public ContentDocumentItem(string content)
		{
			Value = content;
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected ContentDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			if (scopeData.CustomData.TryGetValue("TextOperationData", out var textOperations) 
			    && textOperations is IList<ITextOperation> textOps)
			{
				foreach (var textOperation in textOps.ToArray())
				{
					Value = textOperation.Apply(Value);
					if (textOperation.TransientEdit)
					{
						textOps.Remove(textOperation);
					}
				}
			}

			if (Value != string.Empty)
			{
				outputStream.Write(Value);
			}
			return Children.WithScope(context).ToPromise();
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}