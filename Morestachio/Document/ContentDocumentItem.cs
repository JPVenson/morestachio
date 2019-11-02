using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Defines a area that has no morestachio keywords and can be rendered as is
	/// </summary>
	[System.Serializable]
	public class ContentDocumentItem : ValueDocumentItemBase, IValueDocumentItem
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ContentDocumentItem()
		{

		}

		/// <inheritdoc />
		public ContentDocumentItem(string content)
		{
			Value = content;
		}

		[UsedImplicitly]
		protected ContentDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Content";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			WriteContent(outputStream, Value, context);
			await Task.CompletedTask;
			return Children.WithScope(context);
		}

		internal static void WriteContent(IByteCounterStream builder, string content, ContextObject context)
		{
			content = content ?? context.Options.Null?.ToString();

			var sourceCount = builder.BytesWritten;

			if (context.Options.MaxSize == 0)
			{
				builder.Write(content);
				return;
			}

			if (sourceCount >= context.Options.MaxSize - 1)
			{
				builder.ReachedLimit = true;
				return;
			}

			//TODO this is a performance critical operation. As we might deal with variable-length encodings this cannot be compute initial
			var cl = context.Options.Encoding.GetByteCount(content);

			var overflow = sourceCount + cl - context.Options.MaxSize;
			if (overflow <= 0)
			{
				builder.Write(content, cl);
				return;
			}

			if (overflow < content.Length)
			{
				builder.Write(content.ToCharArray(0, (int)(cl - overflow)), cl - overflow);
			}
			else
			{
				builder.Write(content, cl);
			}
		}

		public async Task<ContextObject> GetValue(ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			return new ContextObject(context.Options, ".", context)
			{
				Value = Value
			};
		}
	}
}