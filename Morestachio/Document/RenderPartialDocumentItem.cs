using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;

namespace Morestachio.Document
{
	/// <summary>
	///		Prints a partial
	/// </summary>
	[System.Serializable]
	public class RenderPartialDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RenderPartialDocumentItem()
		{

		}

		/// <inheritdoc />
		public RenderPartialDocumentItem(string value, IMorestachioExpression context)
		{
			Context = context;
			Value = value;
		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected RenderPartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Context = info.GetValue(nameof(Context), typeof(IMorestachioExpression)) as IMorestachioExpression;
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Include";

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
				writer.WriteExpressionToXml(Context);
			}
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			base.DeSerializeXml(reader);
			AssertElement(reader, nameof(Value));
			reader.ReadEndElement();

			if (reader.Name == ExpressionTokenizer.ExpressionNodeName)
			{
				var subtree = reader.ReadSubtree();
				subtree.Read();
				Context = subtree.ParseExpressionFromKind();
				reader.Skip();
			}
		}

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, 
			ContextObject context,
			ScopeData scopeData)
		{
			await Task.CompletedTask;
			string partialName = Value;
			var currentPartial = partialName + "_" + scopeData.PartialDepth.Count;
			scopeData.PartialDepth.Push(currentPartial);
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

			if (Context != null)
			{
				cnxt = (await Context.GetValue(context, scopeData));
			}

			if (scopeData.Partials.TryGetValue(partialName, out var partialWithContext))
			{
				return new[]
				{
					new DocumentItemExecution(partialWithContext, cnxt),
				};
			}

			var partialFromStore = context.Options.PartialsStore?.GetPartial(partialName)?.Document;

			if (partialFromStore != null)
			{
				return new[]
				{
					new DocumentItemExecution(partialFromStore, cnxt),
				};
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