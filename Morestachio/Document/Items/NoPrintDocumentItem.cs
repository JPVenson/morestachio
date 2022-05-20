using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.IO.SingleStream;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines a block of entries that will execute its children but will not print to output
	/// </summary>
	[Serializable]
	public class NoPrintDocumentItem : BlockDocumentItemBase,
										ISupportCustomAsyncCompilation
	{

		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal NoPrintDocumentItem() 
		{

		}

		/// <summary>
		///		Creates a new NoPrint DocumentItem
		/// </summary>
		public NoPrintDocumentItem(CharacterLocation location,
									IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
		{
		}
		
		/// <inheritdoc />
		
		protected NoPrintDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{

		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			await MorestachioDocument.ProcessItemsAndChildren(Children, new NullStream(scopeData.ParserOptions), context, scopeData).ConfigureAwait(false);
			return Enumerable.Empty<DocumentItemExecution>();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
		{
			var children = compiler.Compile(Children, parserOptions);

			return (stream, context, scopeData) => children(new NullStream(scopeData.ParserOptions), context, scopeData);
		}
	}
}
