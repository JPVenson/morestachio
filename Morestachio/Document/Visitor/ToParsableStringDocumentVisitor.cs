using System;
using System.Linq;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.TextOperations;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Document.Visitor
{
	/// <summary>
	///		Reconstructs the DocumentTree to a Template string
	/// </summary>
	public class ToParsableStringDocumentVisitor : IDocumentItemVisitor
	{
		/// <summary>
		/// 
		/// </summary>
		public ToParsableStringDocumentVisitor()
		{
			StringBuilder = new StringBuilder();
		}

		/// <summary>
		///		Contains the reconstructed DocumentTree
		/// </summary>
		public StringBuilder StringBuilder { get; set; }

		/// <inheritdoc />
		public void Visit(AliasDocumentItem documentItem)
		{
			VisitChildren(documentItem);
		}

		/// <inheritdoc />
		public void Visit(ContentDocumentItem documentItem)
		{
			StringBuilder.Append(documentItem.Value);
		}

		/// <summary>
		///		Parses an expression for use in an original template
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public string ReparseExpression(IMorestachioExpression expression)
		{
			var visitor = new ToParsableStringExpressionVisitor();
			expression.Accept(visitor);
			return visitor.StringBuilder.ToString();
		}

		/// <summary>
		///		Loops through all the document items children
		/// </summary>
		/// <param name="documentItem"></param>
		public void VisitChildren(IDocumentItem documentItem)
		{
			foreach (var documentItemChild in documentItem.Children)
			{
				documentItemChild.Accept(this);
			}
		}
		

		/// <summary>
		///		Writes the tag with the leading char as well as all of the documentItems children
		/// </summary>
		/// <param name="documentItem"></param>
		/// <param name="tag"></param>
		/// <param name="cmdChar"></param>
		public void Visit(ExpressionDocumentItemBase documentItem, string tag, string cmdChar = "#")
		{
			StringBuilder.Append("{{");
			StringBuilder.Append(cmdChar);
			StringBuilder.Append(tag);
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			var children = documentItem.Children.ToList();
			var aliasDocumentItem = children.FirstOrDefault() as AliasDocumentItem;
			if (!(aliasDocumentItem is null))
			{
				StringBuilder.Append(" AS ");
				StringBuilder.Append(aliasDocumentItem.Value);
			}

			StringBuilder.Append("}}");

			if (children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{/");
				if (!(aliasDocumentItem is null))
				{
					StringBuilder.Append(aliasDocumentItem.Value);
				}
				else
				{
					StringBuilder.Append(tag.Trim());
				}
				StringBuilder.Append("}}");
			}
		}
		
		/// <summary>
		///		Writes the Tag and all of the documentItems children
		/// </summary>
		/// <param name="documentItem"></param>
		/// <param name="tag"></param>
		public void Visit(DocumentItemBase documentItem, string tag)
		{
			StringBuilder.Append("{{#");
			StringBuilder.Append(tag);
			StringBuilder.Append("}}");

			if (documentItem.Children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{/");
				StringBuilder.Append(tag);
				StringBuilder.Append("}}");
			}
		}
		

		/// <inheritdoc />
		public void Visit(DoLoopDocumentItem documentItem)
		{
			Visit(documentItem, "DO ");
		}

		/// <inheritdoc />
		public void Visit(EachDocumentItem documentItem)
		{
			Visit(documentItem, "EACH ");
		}

		/// <inheritdoc />
		public void Visit(ElseExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "ELSE");
		}

		/// <inheritdoc />
		public void Visit(EvaluateVariableDocumentItem documentItem)
		{
			StringBuilder.Append("{{#");
			StringBuilder.Append(documentItem.IdVariableScope == 0 ? "VAR " : "LET ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append(" = ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(ExpressionScopeDocumentItem documentItem)
		{
			StringBuilder.Append("{{#");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			var children = documentItem.Children.ToList();
			var aliasDocumentItem = children.FirstOrDefault() as AliasDocumentItem;
			if (!(aliasDocumentItem is null))
			{
				StringBuilder.Append(" AS ");
				StringBuilder.Append(aliasDocumentItem.Value);
			}

			StringBuilder.Append("}}");

			if (children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{/");
				if (!(aliasDocumentItem is null))
				{
					StringBuilder.Append(aliasDocumentItem.Value);
				}
				else
				{
					StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
				}
				StringBuilder.Append("}}");
			}
		}
		
		/// <inheritdoc />
		public void Visit(IfExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "IF ");
		}
		
		/// <inheritdoc />
		public void Visit(IfNotExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "IF ", "^");
		}
		
		/// <inheritdoc />
		public void Visit(InvertedExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "", "^");
		}
		
		/// <inheritdoc />
		public void Visit(MorestachioDocument documentItem)
		{
			VisitChildren(documentItem);
		}
		
		/// <inheritdoc />
		public void Visit(PartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#DECLARE ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append("}}");

			Visit(documentItem.Partial as MorestachioDocument);

			StringBuilder.Append("{{/DECLARE}}");
		}
		
		/// <inheritdoc />
		public void Visit(PathDocumentItem documentItem)
		{
			Visit(documentItem, "", "");
		}
		
		/// <inheritdoc />
		public void Visit(RemoveAliasDocumentItem documentItem)
		{
		}
		
		/// <inheritdoc />
		public void Visit(RenderPartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#INCLUDE ");
			StringBuilder.Append(documentItem.Value);
			if (documentItem.Context != null)
			{
				StringBuilder.Append(" WITH ");
				StringBuilder.Append(ReparseExpression(documentItem.Context));
			}
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(RenderPartialDoneDocumentItem documentItem)
		{
		}

		/// <inheritdoc />
		public void Visit(WhileLoopDocumentItem documentItem)
		{
			Visit(documentItem, "WHILE ");
		}
		
		/// <inheritdoc />
		public void Visit(TextEditDocumentItem documentItem)
		{
			switch (documentItem.Operation)
			{
				case AppendLineBreakTextOperation _:
					StringBuilder.Append("{{#NL}}");
					break;
				case TrimLineBreakTextOperation op:
					if (op.LineBreaks == 1)
					{
						StringBuilder.Append("{{#TNL}}");
					}
					else if (op.LineBreaks == -1)
					{
						StringBuilder.Append("{{#TNLS}}");
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <inheritdoc />
		public void Visit(IDocumentItem documentItem)
		{
			if (documentItem is IStringVisitor renderer)
			{
				renderer.Render(this);
			}
			else
			{
				throw new InvalidOperationException(
					$"The type '{documentItem.GetType()}' cannot be rendered with the '{nameof(ToParsableStringDocumentVisitor)}'" +
					$" as its an custom document item." +
					$" Please implement '{nameof(IStringVisitor)}'");
			}
		}

		/// <summary>
		///		Defines custom rendering logic for custom <see cref="IDocumentItem"/>
		/// </summary>
		public interface IStringVisitor
		{
			/// <summary>
			///		Renders the object
			/// </summary>
			void Render(ToParsableStringDocumentVisitor visitor);
		}
	}
}
