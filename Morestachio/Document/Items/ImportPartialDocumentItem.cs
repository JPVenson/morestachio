using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using CoreActionPromise = System.Threading.Tasks.ValueTask<System.Tuple<Morestachio.Document.Contracts.IDocumentItem, Morestachio.Framework.Context.ContextObject>>;
using BooleanPromise = System.Threading.Tasks.ValueTask<bool>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using CoreActionPromise = System.Threading.Tasks.Task<System.Tuple<Morestachio.Document.Contracts.IDocumentItem, Morestachio.Framework.Context.ContextObject>>;
using BooleanPromise = System.Threading.Tasks.Task<bool>;
#endif

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Prints a partial
	/// </summary>
	[Serializable]
	public class ImportPartialDocumentItem : ExpressionDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ImportPartialDocumentItem()
		{

		}

		/// <inheritdoc />
		public ImportPartialDocumentItem(CharacterLocation location,
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

		private async CoreActionPromise CoreAction(
			ContextObject context,
			ScopeData scopeData,
			Func<string, ContextObject, BooleanPromise> obtainPartialFromStore,
			string partialName)
		{
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
						return null;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			scopeData.AddVariable("$name",
				(scope) => context.Options.CreateContextObject("$name", context.CancellationToken,
					scope.PartialDepth.Peek().Item1, context), 0);

			var cnxt = context;
			if (Context != null)
			{
				cnxt = (await Context.GetValue(context, scopeData));
			}

			cnxt = cnxt.Copy().MakeNatural();

			scopeData.AddVariable("$recursion",
				(scope) => cnxt.Options.CreateContextObject("$recursion", context.CancellationToken,
					scope.PartialDepth.Count, cnxt), 0);

			if (await obtainPartialFromStore(partialName, cnxt))
			{
				return null;
			}

			if (context.Options.PartialsStore != null)
			{
				MorestachioDocumentInfo partialFromStore;
				if (context.Options.PartialsStore is IAsyncPartialsStore asyncPs)
				{
					partialFromStore = await asyncPs.GetPartialAsync(partialName, context.Options);
				}
				else
				{
					partialFromStore = context.Options.PartialsStore.GetPartial(partialName, context.Options);
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

		/// <inheritdoc />
		public Compilation Compile()
		{
			var doneAction = new RenderPartialDoneDocumentItem().Compile();
			var expression = MorestachioExpression.Compile();
			return async (stream, context, scopeData) =>
			{
				var partialName = (await expression(context, scopeData)).Value?.ToString();

				if (partialName == null)
				{
					throw new MorestachioRuntimeException($"Get partial requested by the expression: '{MorestachioExpression.ToString()}' returned null and is therefor not valid");
				}

				var toExecute = await CoreAction(context, scopeData, async (pn, cnxt) =>
				 {
					 if (scopeData.CompiledPartials.TryGetValue(pn, out var partialWithContext))
					 {
						 await partialWithContext(stream, cnxt, scopeData);
						 await doneAction(stream, cnxt, scopeData);
						 return true;
					 }

					 return false;
				 }, partialName);
				if (toExecute != null)
				{
					await MorestachioDocument.CompileItemsAndChildren(new IDocumentItem[]
					{
						toExecute.Item1
					})(stream, toExecute.Item2, scopeData);
					await doneAction(stream, toExecute.Item2, scopeData);
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

			var partialName = (await MorestachioExpression.GetValue(context, scopeData)).Value?.ToString();

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
			 }, partialName);
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
}