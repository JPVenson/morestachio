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

namespace Morestachio.Document.Items;

/// <summary>
///		Prints a partial
/// </summary>
[Serializable, Obsolete("Use the #Import keyword")]
public class RenderPartialDocumentItem : ValueDocumentItemBase
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal RenderPartialDocumentItem()
	{

	}

	/// <inheritdoc />
	public RenderPartialDocumentItem(CharacterLocation location,  string value,  IMorestachioExpression context,
									IEnumerable<ITokenOption> tagCreationOptions)
		: base(location, value,tagCreationOptions)
	{
		Context = context;
	}

	/// <inheritdoc />
		
	protected RenderPartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		Context = info.GetValue(nameof(Context), typeof(IMorestachioExpression)) as IMorestachioExpression;
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
	protected override void SerializeXml(XmlWriter writer)
	{
		base.SerializeXml(writer);
		if (Context != null)
		{
			writer.WriteStartElement("With");
			writer.WriteExpressionToXml(Context);
			writer.WriteEndElement();//</with>
		}
	}

	/// <inheritdoc />
	protected override void DeSerializeXml(XmlReader reader)
	{
		base.DeSerializeXml(reader);
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

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		string partialName = Value;
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
					return Array.Empty<DocumentItemExecution>();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		var cnxt = context;
		scopeData.AddVariable("$name",
			(scope) => scopeData.ParserOptions.CreateContextObject("$name", partialName, cnxt), 0);

		if (Context != null)
		{
			cnxt = (await Context.GetValue(context, scopeData).ConfigureAwait(false));
		}

		scopeData.AddVariable("$recursion",
			(scope) => scopeData.ParserOptions.CreateContextObject("$recursion", scope.PartialDepth.Count, cnxt), 0);

		if (scopeData.Partials.TryGetValue(partialName, out var partialWithContext))
		{
			return new[]
			{
				new DocumentItemExecution(partialWithContext, cnxt),
				new DocumentItemExecution(new RenderPartialDoneDocumentItem(), cnxt),
			};
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

				return new[]
				{
					new DocumentItemExecution(partialFromStore.Document, cnxt),
					new DocumentItemExecution(new RenderPartialDoneDocumentItem(), cnxt),
				};
			}
		}


		throw new MorestachioRuntimeException($"Could not obtain a partial named '{partialName}' from the template nor the Partial store");
	}
	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}
}