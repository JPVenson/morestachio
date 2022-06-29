using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		Repeats the template a number of times
/// </summary>
[Serializable]
public class RepeatDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	internal RepeatDocumentItem()
	{
			
	}

	/// <summary>
	///		Creates a new repeat document item
	/// </summary>
	public RepeatDocumentItem(TextRange location, IMorestachioExpression value,
							IEnumerable<ITokenOption> tagCreationOptions) : base(location, value,tagCreationOptions)
	{
	}

	/// <inheritdoc />
		
	protected RepeatDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{

	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var children = compiler.Compile(Children, parserOptions);
		var expression = MorestachioExpression.Compile(parserOptions);
		return async (stream, context, scopeData) =>
		{
			var c = await expression(context, scopeData).ConfigureAwait(false);

			if (!c.Exists())
			{
				return;
			}

			if (c.Value is not Number && !(Number.IsIntegralNumber(c.Value)))
			{
				var path = new Stack<string>();
				var parent = context.Parent;
				while (parent != null)
				{
					path.Push(parent.Key);
					parent = parent.Parent;
				}

				throw new IndexedParseException(TextRangeExtended.Empty,
					string.Format(
						"{1}'{0}' is expected to return a integral number but did not." +
						" Complete Expression until Error:{2}",
						MorestachioExpression.AsStringExpression(), 
						base.ExpressionStart,
						(path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
			}

			var nr = c.Value is Number value ? value : new Number(c.Value as IConvertible);
			for (int i = 0; i < nr; i++)
			{
				var contextCollection = new ContextCollection(i, i + 1 == nr, $"[{i}]", context, context.Value);
				await children(stream, contextCollection, scopeData).ConfigureAwait(false);
			}
		};
	}
		
	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
	{
		var c = await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false);

		if (!c.Exists())
		{
			return Array.Empty<DocumentItemExecution>();
		}
		
		if (c.Value is not Number && !(Number.IsIntegralNumber(c.Value)))
		{
			var path = new Stack<string>();
			var parent = context.Parent;
			while (parent != null)
			{
				path.Push(parent.Key);
				parent = parent.Parent;
			}

			throw new IndexedParseException(TextRangeExtended.Empty, 
				string.Format("{1}'{0}' is expected to return a integral number but did not." + " Complete Expression until Error:{2}",
					MorestachioExpression.AsStringExpression(),
					ExpressionStart,
					(path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
		}
		
		var nr = c.Value is Number value ? value : new Number(c.Value as IConvertible);
		var scopes = new List<DocumentItemExecution>();
		for (int i = 0; i < nr; i++)
		{
			var contextCollection = new ContextCollection(i, i + 1 == nr, $"[{i}]", context, context.Value);
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