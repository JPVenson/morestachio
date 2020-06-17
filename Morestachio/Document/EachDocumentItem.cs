using System.Collections;
using System.Collections.Generic;
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

namespace Morestachio.Document
{
	/// <summary>
	///		Emits N items that are in the <see cref="Value"/>
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

		/// <exception cref="IndexedParseException"></exception>
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//if we're in the same scope, just negating, then we want to use the same object
			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await MorestachioExpression.GetValue(context, scopeData);

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

				throw new IndexedParseException(CharacterLocationExtended.Empty, 
					string.Format("{1}'{0}' is used like an array by the template, but is a scalar value or object in your model." + " Complete Expression until Error:{2}",
						MorestachioExpression.ToString(), base.ExpressionStart, (path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
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
				var innerContext = new ContextCollection(index, next == null, context.Options, $"[{index}]", c)
				{
					Value = current,
				};
				scopes.AddRange(Children.WithScope(innerContext.MakeNatural()));
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