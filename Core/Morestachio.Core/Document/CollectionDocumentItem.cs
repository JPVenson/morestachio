using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Emits N items that are in the <see cref="Value"/>
	/// </summary>
	public class CollectionDocumentItem : DocumentItemBase
	{
		/// <inheritdoc />
		public CollectionDocumentItem(string value)
		{
			Value = value;
		}

		/// <inheritdoc />
		public override string Kind { get; } = "OpenCollection";

		/// <summary>
		///		Defines the expression from which the collection should be taken
		/// </summary>
		public string Value { get; }

		/// <exception cref="IndexedParseException"></exception>
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//if we're in the same scope, just negating, then we want to use the same object
			var c = await context.GetContextForPath(Value, scopeData);

			if (!await c.Exists())
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

				throw new IndexedParseException(
					"{1}'{0}' is used like an array by the template, but is a scalar value or object in your model." + " Complete Expression until Error:{2}",
					Value, base.ExpressionStart, (path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f)));
			}

			var scopes = new List<DocumentItemExecution>();

			//Use this "lookahead" enumeration to allow the $last keyword
			var index = 0;
			var enumerator = value.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return new DocumentItemExecution[0];
			}

			var current = enumerator.Current;
			do
			{
				var next = enumerator.MoveNext() ? enumerator.Current : null;
				var innerContext = new ContextCollection(index, next == null, context.Options, $"[{index}]")
				{
					Value = current,
					Parent = c
				};
				scopes.AddRange(Children.WithScope(innerContext));
				index++;
				current = next;
			} while (current != null && ContinueBuilding(outputStream, context));

			return scopes;
		}
	}
}