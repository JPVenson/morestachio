#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Emits N items that are in the collection
	/// </summary>
	[Serializable]
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
		
		/// <exception cref="IndexedParseException"></exception>
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
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