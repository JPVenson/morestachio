using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.ParserErrors;

#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Document
{
	/// <summary>
	///		Emits N items that are in the collection
	/// </summary>
	[System.Serializable]
	public class EachDocumentItem : ExpressionDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal EachDocumentItem()
		{

		}

		/// <inheritdoc />
		public EachDocumentItem(IMorestachioExpression value)
		{
			MorestachioExpression = value;
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		protected EachDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override string Kind { get; } = "OpenCollection";

		//private class WrapperCounterStream : ByteCounterStream
		//{
		//	private readonly IByteCounterStream _source;

		//	public WrapperCounterStream(
		//		IByteCounterStream source,
		//		ParserOptions options)
		//		: base(new MemoryStream(), 2024, false, options)
		//	{
		//		_source = source;
		//		BytesWritten = _source.BytesWritten;
		//		ReachedLimit = _source.ReachedLimit;
		//	}

		//	public string Read()
		//	{
		//		BaseWriter.Flush();
		//		return Options.Encoding.GetString((BaseWriter.BaseStream as MemoryStream).ToArray());
		//	}
		//}


		/// <exception cref="IndexedParseException"></exception>
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//if we're in the same scope, just negating, then we want to use the same object
			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await MorestachioExpression.GetValue(context, scopeData);

			if (!c.Exists())
			{
				return new DocumentItemExecution[0];
			}

			if (!(c.Value is IEnumerable value) || value is string || value is IDictionary<string, object>)
			{
				var path = new Stack<string>();
				var parent = context.Parent;
				while (parent != null)
				{
					path.Push(parent.Key);
					parent = parent.Parent;
				}

				throw new IndexedParseException(CharacterLocationExtended.Empty, 
					string.Format("{1}'{0}' is used like an array by the template, but is a scalar value or object in your model." + " Complete Expression until Error:{2}",
						MorestachioExpression.ToString(), base.ExpressionStart, (path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
			}

			var scopes = new List<DocumentItemExecution>();

			//var items = value.OfType<object>().ToArray();
			//var result = new WrapperCounterStream[items.Length];

			//Parallel.ForEach(items, async (item, state, index) =>
			//{
			//	var innerContext =
			//		new ContextCollection(index, items.Length == index, context.Options, $"[{index}]", c, item)
			//			.MakeNatural();
			//	var stream = new WrapperCounterStream(outputStream, context.Options);
			//	await MorestachioDocument.ProcessItemsAndChildren(Children, stream, innerContext, scopeData);
			//	result[index] = stream;
			//});

			//foreach (var byteCounterStream in result)
			//{
			//	byteCounterStream.Write(byteCounterStream.Read());
			//}

			//return Enumerable.Empty<DocumentItemExecution>();
			//Use this "lookahead" enumeration to allow the $last keyword
			var index = 0;
			var enumerator = value.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return Enumerable.Empty<DocumentItemExecution>();
			}

			var current = enumerator.Current;
			do
			{
				var next = enumerator.MoveNext() ? enumerator.Current : null;

				var innerContext =
					new ContextCollection(index, next == null, context.Options, $"[{index}]", c, current)
						.MakeNatural();
				scopes.AddRange(Children.WithScope(innerContext));
				index++;
				current = next;
			} while (current != null && ContinueBuilding(outputStream, context));

			return scopes;
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}