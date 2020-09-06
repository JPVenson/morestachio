using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Repeats the template a number of times
	/// </summary>
	[System.Serializable]
	public class RepeatDocumentItem : ExpressionDocumentItemBase
	{
		internal RepeatDocumentItem()
		{
			
		}

		/// <summary>
		///		Creates a new repeat document item
		/// </summary>
		/// <param name="value"></param>
		public RepeatDocumentItem(IMorestachioExpression value)
		{
			MorestachioExpression = value;
		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected RepeatDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{

		}
		
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var c = await MorestachioExpression.GetValue(context, scopeData);

			if (!c.Exists())
			{
				return new DocumentItemExecution[0];
			}

			if (!(Number.IsIntegralNumber(c.Value)))
			{
				var path = new Stack<string>();
				var parent = context.Parent;
				while (parent != null)
				{
					path.Push(parent.Key);
					parent = parent.Parent;
				}

				throw new IndexedParseException(CharacterLocationExtended.Empty, 
					string.Format("{1}'{0}' is expected to return a integral number but did not." + " Complete Expression until Error:{2}",
						MorestachioExpression.ToString(), base.ExpressionStart, (path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
			}

			var nr = new Number(c.Value as IConvertible);
			var scopes = new List<DocumentItemExecution>();
			for (int i = 0; i < nr; i++)
			{
				var contextCollection = new ContextCollection(i, i + 1 == nr, context.Options, $"[{i}]", context, context.Value);
				scopes.AddRange(Children.WithScope(contextCollection));
			}

			return scopes;
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}