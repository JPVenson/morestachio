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
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.TextOperations;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines a area that has no morestachio keywords and can be rendered as is
	/// </summary>
	[Serializable]
	public class ContentDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ContentDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}

		/// <summary>
		///		Creates a new ContentDocumentItem that represents some static content
		/// </summary>
		/// <param name="content">The content to write</param>
		public ContentDocumentItem(CharacterLocation location, string content) : base(location, content)
		{
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
			var value = Value;
			if (scopeData.CustomData.TryGetValue("TextOperationData", out var textOperations) 
			    && textOperations is IList<ITextOperation> textOps)
			{
				foreach (var textOperation in textOps.ToArray())
				{
					value = textOperation.Apply(value);
					if (textOperation.TransientEdit)
					{
						textOps.Remove(textOperation);
					}
				}
			}

			if (value != string.Empty)
			{
				outputStream.Write(value);
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