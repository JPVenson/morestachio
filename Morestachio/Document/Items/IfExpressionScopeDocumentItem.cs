#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
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
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines the start of a Scope
	/// </summary>
	[Serializable]
	public class IfExpressionScopeDocumentItem : ExpressionDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal IfExpressionScopeDocumentItem()
		{
		}

		/// <inheritdoc />
		public IfExpressionScopeDocumentItem(CharacterLocation location, IMorestachioExpression value,
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
		protected override void SerializeXml(XmlWriter writer)
		{
			if (Inverted)
			{
				writer.WriteAttributeString(nameof(Inverted), bool.TrueString);
			}
			base.SerializeXml(writer);
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			if (reader.GetAttribute(nameof(Inverted)) == bool.TrueString)
			{
				Inverted = true;
			}
			base.DeSerializeXml(reader);
		}
		
		/// <summary>
		///		Filters <see cref="BlockDocumentItemBase.Children"/> to only return anything before the first else block
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<IDocumentItem> GetIfContents()
		{
			return Children.TakeWhile(e => !(e is ElseIfExpressionScopeDocumentItem)
										   &&
										   !(e is ElseExpressionScopeDocumentItem));
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
			public Compilation Callback { get; set; }
		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			var c = await MorestachioExpression.GetValue(context, scopeData);
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
					var elseContext = await documentItem.MorestachioExpression.GetValue(context, scopeData);
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

		/// <inheritdoc />
		public virtual Compilation Compile()
		{
			var elseChildren = GetNestedElseConditions()
				.Select(e => new IfExecutionContainer()
				{
					Callback = MorestachioDocument.CompileItemsAndChildren(e.Children),
					Expression = e.MorestachioExpression.Compile()
				}).ToArray();

			var elseDocument = GetNestedElse();
			Compilation elseBlock = null;
			if (elseDocument != null)
			{
				elseBlock = MorestachioDocument.CompileItemsAndChildren(new[] { elseDocument });
			}

			var children = MorestachioDocument.CompileItemsAndChildren(GetIfContents());

			var expression = MorestachioExpression.Compile();
			return async (stream, context, scopeData) =>
			{
				var c = await expression(context, scopeData);

				context = context.IsNaturalContext || context.Parent == null ? context : context.Parent;
				if (c.Exists() != Inverted)
				{
					await children(stream, context, scopeData);
					return;
				}
				else if (elseChildren.Length > 0)
				{
					foreach (var ifExecutionContainer in elseChildren)
					{
						if ((await ifExecutionContainer.Expression(context, scopeData)).Exists() == Inverted)
						{
							continue;
						}

						await ifExecutionContainer.Callback(stream, context, scopeData);
						return;
					}
				}

				if (elseBlock != null)
				{
					await elseBlock(stream, context, scopeData);
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
}