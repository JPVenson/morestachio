using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using dotless.Core;
using dotless.Core.configuration;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;


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
	public class CompileLessDocumentItem : DocumentItemBase
	{
		public CompileLessDocumentItem()
		{

		}

		protected CompileLessDocumentItem(SerializationInfo info, StreamingContext c)
			: base(info, c)
		{

		}

		private class WrapperCounterStream : ByteCounterStream
		{
			private readonly IByteCounterStream _source;

			public WrapperCounterStream(
				IByteCounterStream source,
				ParserOptions options)
				: base(new MemoryStream(), 2024, false, options)
			{
				_source = source;
				BytesWritten = _source.BytesWritten;
				ReachedLimit = _source.ReachedLimit;
			}

			public string Read()
			{
				BaseWriter.Flush();
				return Options.Encoding.GetString((BaseWriter.BaseStream as MemoryStream).ToArray());
			}
		}

		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			using (var tempStream = new WrapperCounterStream(outputStream, context.Options))
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

		public override string Kind { get; } = nameof(CompileLessDocumentItem);
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}