using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Document.Visitor
{
	/// <summary>
	///		Reconstructs the DocumentTree to a Template string
	/// </summary>
	public class ToParsableStringDocumentVisitor : IDocumentItemVisitor
	{
		public ToParsableStringDocumentVisitor()
		{
			StringBuilder = new StringBuilder();
		}

		/// <summary>
		///		Contains the reconstructed DocumentTree
		/// </summary>
		public StringBuilder StringBuilder { get; set; }

		public void Visit(AliasDocumentItem documentItem)
		{
			VisitChildren(documentItem);
		}

		public void Visit(ContentDocumentItem documentItem)
		{
			StringBuilder.Append(documentItem.Value);
		}

		public string ReparseExpression(IMorestachioExpression expression)
		{
			var visitor = new ToParsableStringExpressionVisitor();
			expression.Accept(visitor);
			return visitor.StringBuilder.ToString();
		}

		public void VisitChildren(IDocumentItem documentItem)
		{
			foreach (var documentItemChild in documentItem.Children)
			{
				documentItemChild.Accept(this);
			}
		}

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

		public void Visit(DoLoopDocumentItem documentItem)
		{
			Visit(documentItem, "DO ");
		}

		public void Visit(EachDocumentItem documentItem)
		{
			Visit(documentItem, "EACH ");
		}

		public void Visit(ElseExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "ELSE");
		}

		public void Visit(EvaluateVariableDocumentItem documentItem)
		{
			StringBuilder.Append("{{#VAR ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append(" = ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			StringBuilder.Append("}}");
		}

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
		
		public void Visit(IfExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "IF ");
		}

		public void Visit(IfNotExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "IF ", "^");
		}

		public void Visit(InvertedExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "", "^");
		}

		public void Visit(MorestachioDocument documentItem)
		{
			VisitChildren(documentItem);
		}

		public void Visit(PartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#DECLARE ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append("}}");

			Visit(documentItem.Partial as MorestachioDocument);

			StringBuilder.Append("{{/DECLARE}}");
		}

		public void Visit(PathDocumentItem documentItem)
		{
			Visit(documentItem, "", "");
		}

		public void Visit(RemoveAliasDocumentItem documentItem)
		{
		}

		public void Visit(RenderPartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#INCLUDE ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append("}}");
		}

		public void Visit(RenderPartialDoneDocumentItem documentItem)
		{
		}

		public void Visit(WhileLoopDocumentItem documentItem)
		{
			Visit(documentItem, "WHILE ");
		}

		public void Visit(IDocumentItem documentItem)
		{
			throw new NotImplementedException();
		}
	}
}
