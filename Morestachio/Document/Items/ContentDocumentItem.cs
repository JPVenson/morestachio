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
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.TextOperations;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines a area that has no morestachio keywords and can be rendered as is
	/// </summary>
	[Serializable]
	public class ContentDocumentItem : ValueDocumentItemBase, ISupportCustomCompilation
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
		public ContentDocumentItem(CharacterLocation location, string content,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, content, tagCreationOptions)
		{
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected ContentDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		public Compilation Compile()
		{
			return async (stream, context, scopeData) =>
			{
				CoreAction(stream, scopeData);
			};
		}
		
		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			CoreAction(outputStream, scopeData);
			return Children.WithScope(context).ToPromise();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CoreAction(IByteCounterStream outputStream, ScopeData scopeData)
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

			foreach (var textEditDocumentItem in Children.OfType<TextEditDocumentItem>())
			{
				value = textEditDocumentItem.Operation.Apply(value);
			}

			if (value != string.Empty)
			{
				outputStream.Write(value);
			}
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}