using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using dotless.Core;
using dotless.Core.configuration;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.LessCompiler
{
	/// <summary>
	///		Wraps the dotless into an Document provider 
	/// </summary>
	[Serializable]
	public class CompileLessDocumentItem : BlockDocumentItemBase, ToParsableStringDocumentVisitor.IStringVisitor
	{
		internal CompileLessDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}
		
		public CompileLessDocumentItem(CharacterLocation location, IEnumerable<ITokenOption> tagTokenOptions) 
			: base(location, tagTokenOptions)
		{

		}

		/// <summary>
		///		Serialization Constructor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="c"></param>
		protected CompileLessDocumentItem(SerializationInfo info, StreamingContext c)
			: base(info, c)
		{

		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			using (var tempStream = outputStream.GetSubStream())
			{
				await MorestachioDocument.ProcessItemsAndChildren(Children, tempStream, context, scopeData);
				var lessCode = tempStream.Read();
				outputStream.Write(Less.Parse(lessCode, new DotlessConfiguration()
				{
					CacheEnabled = false,
				}));
			}
			return Enumerable.Empty<DocumentItemExecution>();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public void Render(ToParsableStringDocumentVisitor visitor)
		{
			visitor.StringBuilder.Append("{{#LESS}}");
			visitor.VisitChildren(this);
			visitor.StringBuilder.Append("{{/LESS}}");
		}
	}
}