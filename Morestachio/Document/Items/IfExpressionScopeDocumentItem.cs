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
			Else = info.GetValue(nameof(Else), typeof(IDocumentItem)) as IDocumentItem;
			Inverted = info.GetBoolean(nameof(Inverted));
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Else), Else);
			info.AddValue(nameof(Inverted), Inverted);
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			if (Inverted)
			{
				writer.WriteAttributeString(nameof(Inverted), bool.TrueString);
			}
			base.SerializeXml(writer);
			if (Else != null)
			{
				writer.WriteStartElement(nameof(Else));
				Else.SerializeXmlCore(writer);
				writer.WriteEndElement();//else
			}
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			if (reader.GetAttribute(nameof(Inverted)) == bool.TrueString)
			{
				Inverted = true;
			}
			base.DeSerializeXml(reader);
			if (reader.Name == nameof(Else))
			{
				reader.ReadStartElement();
				var elseDocument = DocumentExtensions.CreateDocumentItemInstance(reader.Name);
				
				var childTree = reader.ReadSubtree();
				childTree.Read();
				elseDocument.DeSerializeXmlCore(childTree);
				reader.Skip();
				Else = elseDocument;
				reader.ReadEndElement();//Else
			}
		}

		/// <summary>
		///		If present, the document items to be printed if the condition does not match
		/// </summary>
		public IDocumentItem Else { get; internal set; }

		/// <summary>
		///		If set to true the expression must evaluate to false instead of true
		/// </summary>
		public bool Inverted { get; set; }

		/// <inheritdoc />
		public virtual Compilation Compile()
		{
			var elseCompiled = Else != null ? MorestachioDocument.CompileItemsAndChildren(new[] { Else }) : null;

			var children = MorestachioDocument.CompileItemsAndChildren(Children);
			var expression = MorestachioExpression.Compile();
			return async (stream, context, scopeData) =>
			{
				var c = await expression(context, scopeData);
				if (c.Exists() != Inverted)
				{
					await children(stream, context.IsNaturalContext || context.Parent == null ? context : context.Parent,
						scopeData);
				}
				else if (elseCompiled != null)
				{
					await elseCompiled(stream, context.IsNaturalContext || context.Parent == null ? context : context.Parent,
						scopeData);
				}
			};
		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			var c = await MorestachioExpression.GetValue(context, scopeData);

			if (c.Exists() != Inverted)
			{
				return Children
					.WithScope(context.IsNaturalContext || context.Parent == null ? context : context.Parent);
			}
			else if (Else != null)
			{
				return new[] { Else }
					.WithScope(context.IsNaturalContext || context.Parent == null ? context : context.Parent);
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