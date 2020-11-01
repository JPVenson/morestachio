#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Prints a partial
	/// </summary>
	[Serializable, Obsolete("Use the #Import keyword")]
	public class RenderPartialDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RenderPartialDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}

		/// <inheritdoc />
		public RenderPartialDocumentItem(CharacterLocation location, [NotNull] string value, [CanBeNull] IMorestachioExpression context)
			: base(location, value)
		{
			Context = context;
		}

		/// <inheritdoc />
		[UsedImplicitly]
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
			AssertElement(reader, nameof(Value));
			reader.ReadEndElement();

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
			if (scopeData.PartialDepth.Count >= context.Options.PartialStackSize)
			{
				switch (context.Options.StackOverflowBehavior)
				{
					case PartialStackOverflowBehavior.FailWithException:
						throw new MustachioStackOverflowException(
							$"You have exceeded the maximum stack Size for nested Partial calls of '{context.Options.PartialStackSize}'. See Data for call stack")
						{
							Data =
							{
								{"Callstack", scopeData.PartialDepth}
							}
						};
					case PartialStackOverflowBehavior.FailSilent:
						return new DocumentItemExecution[0];
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			var cnxt = context;
			scopeData.AddVariable("$name",
				(scope) => cnxt.Options.CreateContextObject("$name", context.CancellationToken,
					partialName, cnxt), 0);

			if (Context != null)
			{
				cnxt = (await Context.GetValue(context, scopeData));
			}

			scopeData.AddVariable("$recursion",
				(scope) => cnxt.Options.CreateContextObject("$recursion", context.CancellationToken,
					scope.PartialDepth.Count, cnxt), 0);

			if (scopeData.Partials.TryGetValue(partialName, out var partialWithContext))
			{
				return new[]
				{
					new DocumentItemExecution(partialWithContext, cnxt),
					new DocumentItemExecution(new RenderPartialDoneDocumentItem(), cnxt),
				};
			}


			if (context.Options.PartialsStore != null)
			{
				MorestachioDocumentInfo partialFromStore;
				if (context.Options.PartialsStore is IAsyncPartialsStore asyncPs)
				{
					partialFromStore = await asyncPs.GetPartialAsync(partialName);
				}
				else
				{
					partialFromStore = context.Options.PartialsStore.GetPartial(partialName);
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
}