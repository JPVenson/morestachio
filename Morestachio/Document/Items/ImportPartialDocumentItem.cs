using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		Prints a partial
/// </summary>
[Serializable]
public class ImportPartialDocumentItem : BlockExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal ImportPartialDocumentItem()
	{

	}

	/// <inheritdoc />
	public ImportPartialDocumentItem(TextRange location,
									IMorestachioExpression value,
									IMorestachioExpression context,
									IEnumerable<ITokenOption> tagCreationOptions)
		: base(location, value,tagCreationOptions)
	{
		Context = context;
	}

	/// <inheritdoc />
		
	protected ImportPartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		Context = info.GetValueOrDefault<IMorestachioExpression>(c, nameof(Context));
	}

	/// <summary>
	///		Gets the context this Partial should run in
	/// </summary>
	public IMorestachioExpression Context { get; private set; }

	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		info.AddValue(nameof(Context), Context);
	}

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);
		if (Context != null)
		{
			writer.WriteStartElement("With");
			writer.WriteExpressionToXml(Context);
			writer.WriteEndElement(); //</with>
		}
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);
		if (reader.Name == "With")
		{
			reader.ReadStartElement();
			var subtree = reader.ReadSubtree();
			subtree.Read();
			Context = subtree.ParseExpressionFromKind();
			reader.Skip();
			reader.ReadEndElement();
		}
	}
	
	private async CoreActionPromise CoreAction(
		ContextObject context,
		ScopeData scopeData,
		Func<string, ContextObject, BooleanPromise> obtainPartialFromStore,
		string partialName)
	{
		scopeData.PartialDepth.Push(new Tuple<string, int>(partialName, scopeData.PartialDepth.Count));
		if (scopeData.PartialDepth.Count >= scopeData.ParserOptions.PartialStackSize)
		{
			switch (scopeData.ParserOptions.StackOverflowBehavior)
			{
				case PartialStackOverflowBehavior.FailWithException:
					throw new MorestachioStackOverflowException(
						$"You have exceeded the maximum stack Size for nested Partial calls of '{scopeData.ParserOptions.PartialStackSize}'. See Data for call stack")
						{
							Data =
							{
								{"Callstack", scopeData.PartialDepth}
							}
						};
				case PartialStackOverflowBehavior.FailSilent:
					return null;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		scopeData.AddVariable("$name",
			(scope) => scopeData.ParserOptions.CreateContextObject("$name", scope.PartialDepth.Peek().Item1, context), 0);

		var cnxt = context;
		if (Context != null)
		{
			cnxt = (await Context.GetValue(context, scopeData).ConfigureAwait(false));
		}

		cnxt = cnxt.Copy().MakeNatural();

		scopeData.AddVariable("$recursion",
			(scope) => scopeData.ParserOptions.CreateContextObject("$recursion", scope.PartialDepth.Count, cnxt), 0);

		if (await obtainPartialFromStore(partialName, cnxt).ConfigureAwait(false))
		{
			return null;
		}

		if (scopeData.ParserOptions.PartialsStore != null)
		{
			MorestachioDocumentInfo partialFromStore;
			if (scopeData.ParserOptions.PartialsStore is IAsyncPartialsStore asyncPs)
			{
				partialFromStore = await asyncPs.GetPartialAsync(partialName, scopeData.ParserOptions).ConfigureAwait(false);
			}
			else
			{
				partialFromStore = scopeData.ParserOptions.PartialsStore.GetPartial(partialName, scopeData.ParserOptions);
			}

			if (partialFromStore != null)
			{
				if (partialFromStore.Errors.Any())
				{
					throw new MorestachioRuntimeException($"The partial named '{partialName}' obtained from external partial store contains one or more errors");
				}

				return Tuple.Create(partialFromStore.Document, cnxt);
			}
		}

		throw new MorestachioRuntimeException($"Could not obtain a partial named '{partialName}' from the template nor the Partial store");
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var doneAction = new RenderPartialDoneDocumentItem().Compile(compiler, parserOptions);
		var expression = MorestachioExpression.Compile(parserOptions);
		return async (stream, context, scopeData) =>
		{
			var partialName = (await expression(context, scopeData).ConfigureAwait(false)).RenderToString(scopeData).ToString();

			if (partialName == null)
			{
				throw new MorestachioRuntimeException($"Get partial requested by the expression: '{MorestachioExpression.ToString()}' returned null and is therefor not valid");
			}

			var toExecute = await CoreAction(context, scopeData, async (pn, cnxt) =>
			{
				if (scopeData.CompiledPartials.TryGetValue(pn, out var partialWithContext))
				{
					await partialWithContext(stream, cnxt, scopeData).ConfigureAwait(false);
					await doneAction(stream, cnxt, scopeData).ConfigureAwait(false);
					return true;
				}

				return false;
			}, partialName).ConfigureAwait(false);
			if (toExecute != null)
			{
				await compiler.Compile(new IDocumentItem[]
				{
					toExecute.Item1
				}, parserOptions)(stream, toExecute.Item2, scopeData).ConfigureAwait(false);
				await doneAction(stream, toExecute.Item2, scopeData).ConfigureAwait(false);
			}
		};
	}

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		Tuple<IDocumentItem, ContextObject> action = null;
		Tuple<IDocumentItem, ContextObject> actiona = null;

		var partialName = (await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false)).RenderToString(scopeData).ToString();

		if (partialName == null)
		{
			throw new MorestachioRuntimeException($"Get partial requested by the expression: '{MorestachioExpression.ToString()}' returned null and is therefor not valid");
		}

		action = await CoreAction(context, scopeData, (pn, cnxt) =>
		{
			if (scopeData.Partials.TryGetValue(pn, out var partialWithContext))
			{
				actiona = new Tuple<IDocumentItem, ContextObject>(partialWithContext, cnxt);
				return true.ToPromise();
			}

			return false.ToPromise();
		}, partialName).ConfigureAwait(false);
		action = action ?? actiona;
		if (action != null)
		{
			return new[]
			{
				new DocumentItemExecution(action.Item1, action.Item2),
				new DocumentItemExecution(new RenderPartialDoneDocumentItem(), action.Item2),
			};
		}
		return Enumerable.Empty<DocumentItemExecution>();
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}
}