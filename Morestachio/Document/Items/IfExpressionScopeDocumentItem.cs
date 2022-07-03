using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		Defines the start of a Scope
/// </summary>
[Serializable]
public class IfExpressionScopeDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal IfExpressionScopeDocumentItem()
	{
	}

	/// <inheritdoc />
	public IfExpressionScopeDocumentItem(in TextRange location, IMorestachioExpression value,
										IEnumerable<ITokenOption> tagCreationOptions,
										bool inverted)
		: base(location, value, tagCreationOptions)
	{
		Inverted = inverted;
	}

	/// <inheritdoc />
	protected IfExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		Inverted = info.GetBoolean(nameof(Inverted));
	}

	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		info.AddValue(nameof(Inverted), Inverted);
	}

	/// <inheritdoc />
	protected override void SerializeXmlHeaderCore(XmlWriter writer)
	{
		base.SerializeXmlHeaderCore(writer);
		if (Inverted)
		{
			writer.WriteAttributeString(nameof(Inverted), bool.TrueString);
		}
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlHeaderCore(XmlReader reader)
	{
		base.DeSerializeXmlHeaderCore(reader);
		Inverted = reader.GetAttribute(nameof(Inverted)) == bool.TrueString;
	}
		
	/// <summary>
	///		Filters <see cref="BlockDocumentItemBase.Children"/> to only return anything before the first else block
	/// </summary>
	/// <returns></returns>
	protected virtual IList<IDocumentItem> GetIfContents()
	{
		return Children.TakeWhile(e => !(e is ElseIfExpressionScopeDocumentItem)
			&&
			!(e is ElseExpressionScopeDocumentItem)).ToArray();
	}

	/// <summary>
	///		Filters <see cref="BlockDocumentItemBase.Children"/> to return all elseif blocks
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerable<ElseIfExpressionScopeDocumentItem> GetNestedElseConditions()
	{
		return Children.OfType<ElseIfExpressionScopeDocumentItem>();
	}

	/// <summary>
	///		Filters <see cref="BlockDocumentItemBase.Children"/> to return the only occurrence of an else block or null
	/// </summary>
	/// <returns></returns>
	protected virtual ElseExpressionScopeDocumentItem GetNestedElse()
	{
		return Children.OfType<ElseExpressionScopeDocumentItem>().FirstOrDefault();
	}

	/// <summary>
	///		If set to true the expression must evaluate to false instead of true
	/// </summary>
	public bool Inverted { get; set; }


	internal class IfExecutionContainer
	{
		public CompiledExpression Expression { get; set; }
		public CompilationAsync Callback { get; set; }
	}

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		var c = await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false);
		context = context.IsNaturalContext || context.Parent == null ? context : context.Parent;

		if (c.Exists() != Inverted)
		{
			return GetIfContents()
				.WithScope(context.IsNaturalContext || context.Parent == null ? context : context.Parent);
		}

		var elseBlocks = GetNestedElseConditions().ToArray();
		if (elseBlocks.Length > 0)
		{
			foreach (var documentItem in elseBlocks)
			{
				var elseContext = await documentItem.MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false);
				if (elseContext.Exists() != Inverted)
				{
					return documentItem.Children
						.WithScope(context.IsNaturalContext || context.Parent == null ? context : context.Parent);
				}
			}
		}

		var elseBlock = GetNestedElse();

		if (elseBlock != null)
		{
			return new[] { elseBlock }
				.WithScope(context.IsNaturalContext || context.Parent == null ? context : context.Parent);
		}

		return Enumerable.Empty<DocumentItemExecution>();
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public virtual CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var elseChildren = GetNestedElseConditions()
			.Select(e => new IfExecutionContainer()
			{
				Callback = compiler.Compile(e.Children, parserOptions),
				Expression = e.MorestachioExpression.Compile(parserOptions)
			}).ToArray();

		var elseDocument = GetNestedElse();
		CompilationAsync elseBlock = null;
		if (elseDocument != null)
		{
			elseBlock = compiler.Compile(new[] { elseDocument }, parserOptions);
		}

		var children = compiler.Compile(GetIfContents(), parserOptions);

		var expression = MorestachioExpression.Compile(parserOptions);
		return async (stream, context, scopeData) =>
		{
			var c = await expression(context, scopeData).ConfigureAwait(false);

			context = context.IsNaturalContext || context.Parent == null ? context : context.Parent;
			if (c.Exists() != Inverted)
			{
				await children(stream, context, scopeData).ConfigureAwait(false);
				return;
			}
			else if (elseChildren.Length > 0)
			{
				foreach (var ifExecutionContainer in elseChildren)
				{
					if ((await ifExecutionContainer.Expression(context, scopeData).ConfigureAwait(false)).Exists() == Inverted)
					{
						continue;
					}

					await ifExecutionContainer.Callback(stream, context, scopeData).ConfigureAwait(false);
					return;
				}
			}

			if (elseBlock != null)
			{
				await elseBlock(stream, context, scopeData).ConfigureAwait(false);
				return;
			}
		};
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}
}