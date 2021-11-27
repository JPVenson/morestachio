using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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
		public ContentDocumentItem(CharacterLocation location, string content,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, content, tagCreationOptions)
		{
		}

		/// <inheritdoc />

		protected ContentDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <param name="compiler"></param>
		/// <param name="parserOptions"></param>
		/// <inheritdoc />
		public Compilation Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
		{
			//return DocumentCompiler.NopAction;
			//return (stream, context, scopeData) => {};

			var value = Value;
			foreach (var textEditDocumentItem in Children.OfType<TextEditDocumentItem>())
			{
				value = textEditDocumentItem.Operation.Apply(value);
			}

			if (value == null)
			{
				return null;
			}
			
#if Span
			var memValue = value.AsMemory();
			return (stream, context, scopeData) =>
			{
				stream.Write(memValue);
			};
			
#else
			return (stream, context, scopeData) =>
			{
				stream.Write(value);
			};

#endif

		}

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			var value = Value;
			foreach (var textEditDocumentItem in Children.OfType<TextEditDocumentItem>())
			{
				value = textEditDocumentItem.Operation.Apply(value);
			}

			if (value != string.Empty)
			{
				outputStream.Write(value);
			}
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
			//return Children.WithScope(context).ToPromise();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}